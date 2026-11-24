using System.Text.Json;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.Actors;
using InterneTaakAfhandeling.Common.Services.Internetaken;
using InterneTaakAfhandeling.Common.Services.Klantcontacten;
using InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.AllInternetakenOverview
{
    public interface IInterneTakenOverzichtService
    {
        /// <param name="interneTaakQuery">Filters that OpenKlant applies, including paging.</param>
        /// <param name="medewerkerIdentificatie">Optional objectregister identificatie of a medewerker; only contactverzoeken assigned to that medewerker are returned.</param>
        Task<InterneTakenOverviewResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTaakQuery, string? medewerkerIdentificatie = null);
    }

    public class InterneTakenOverzichtService : IInterneTakenOverzichtService
    {
        // Used when a medewerker is known under several actors and the lists have to be merged in memory.
        private const int MergePageSize = 100;
        private const int MaxMergePages = 10;

        private readonly IUrgentieBerekenService _urgentieBerekenService;
        private readonly IKlantcontactService _klantcontactService;
        private readonly IInternetaakService  _internetaakService;
        private readonly IActorService _actorService;
        private readonly IMedewerkerActorenService _medewerkerActorenService;
        private readonly ILogger<InterneTakenOverzichtService> _logger;
        private readonly IOpenKlantApiClient _openKlantApiClient;

        public InterneTakenOverzichtService(
            IUrgentieBerekenService urgentieBerekenService,
            IKlantcontactService klantcontactService,
            IInternetaakService internetaakService,
            IActorService actorService,
            IMedewerkerActorenService medewerkerActorenService,
            ILogger<InterneTakenOverzichtService> logger,
            IOpenKlantApiClient openKlantApiClient)
        {
            _urgentieBerekenService = urgentieBerekenService ?? throw new ArgumentNullException(nameof(urgentieBerekenService));
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _internetaakService = internetaakService ?? throw new ArgumentNullException(nameof(internetaakService));
            _actorService = actorService ?? throw new ArgumentNullException(nameof(actorService));
            _medewerkerActorenService = medewerkerActorenService ?? throw new ArgumentNullException(nameof(medewerkerActorenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        }

        public async Task<InterneTakenOverviewResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTakenQuery, string? medewerkerIdentificatie = null)
        {
            var internetakenResponse = string.IsNullOrWhiteSpace(medewerkerIdentificatie)
                ? await GetInternetakenPaginatedAsync(interneTakenQuery)
                : await GetInternetakenForMedewerkerAsync(interneTakenQuery, medewerkerIdentificatie);

            var OverzichtItems = (await Task.WhenAll(internetakenResponse.Results
                .Select(ExtendAndMapInternetaakToOverzichtItemAsync))).ToList();

            return new InterneTakenOverviewResponse
            {
                Count = internetakenResponse.Count,
                Next = internetakenResponse.Next,
                Previous = internetakenResponse.Previous,
                Results = OverzichtItems
            };
        }

        private async Task<InternetakenResponse> GetInternetakenForMedewerkerAsync(InterneTaakQuery query, string medewerkerIdentificatie)
        {
            var actorUuids = await _medewerkerActorenService.GetActorUuidsAsync(medewerkerIdentificatie);

            switch (actorUuids.Count)
            {
                // A medewerker that never had a contactverzoek has no actor in OpenKlant yet
                case 0:
                    return new InternetakenResponse { Results = [], Count = 0 };

                // Common case: let OpenKlant filter and paginate
                case 1:
                    return await GetInternetakenPaginatedAsync(query with { ToegewezenAanActor__Uuid = actorUuids[0] });

                // OpenKlant can only filter on one actor at a time: fetch per actor, merge and paginate here
                default:
                    return await GetMergedInternetakenPageAsync(query, actorUuids);
            }
        }

        private async Task<InternetakenResponse> GetMergedInternetakenPageAsync(InterneTaakQuery query, IReadOnlyList<Guid> actorUuids)
        {
            var perActor = await Task.WhenAll(actorUuids.Select(uuid => GetAllInternetakenForActorAsync(query, uuid)));

            var merged = perActor
                .SelectMany(x => x)
                .DistinctBy(x => x.Uuid)
                .OrderByDescending(x => x.ToegewezenOp)
                .ToList();

            var page = query.Page ?? 1;
            var pageSize = query.PageSize ?? 20;

            // Same behaviour as OpenKlant: a page beyond the last page is a 404
            if (page > 1 && (page - 1) * pageSize >= merged.Count)
            {
                throw new HttpRequestException("Page not found", null, System.Net.HttpStatusCode.NotFound);
            }

            return new InternetakenResponse
            {
                Count = merged.Count,
                Next = page * pageSize < merged.Count ? $"?page={page + 1}" : null,
                Previous = page > 1 ? $"?page={page - 1}" : null,
                Results = merged.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        private async Task<List<Internetaak>> GetAllInternetakenForActorAsync(InterneTaakQuery query, Guid actorUuid)
        {
            var result = new List<Internetaak>();

            for (var page = 1; page <= MaxMergePages; page++)
            {
                InternetakenResponse response;
                try
                {
                    response = await GetInternetakenPaginatedAsync(query with { ToegewezenAanActor__Uuid = actorUuid, Page = page, PageSize = MergePageSize });
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    break;
                }

                result.AddRange(response.Results);

                if (response.Next == null)
                {
                    return result;
                }
            }

            _logger.LogWarning("Meer dan {Max} contactverzoeken voor actor {ActorUuid}; de lijst is afgekapt",
                MaxMergePages * MergePageSize, actorUuid);
            return result;
        }


        private async Task<InternetakenResponse> GetInternetakenPaginatedAsync(InterneTaakQuery query)
        {
            var queryString = query.BuildQueryString();
            var response = await _openKlantApiClient.GetAsyncHttpResponse($"/klantinteracties/api/v1/internetaken?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Deserialize<InternetakenResponse>(content, options)
                       ?? new InternetakenResponse { Results = new List<Internetaak>(), Count = 0 };
            }

            throw new HttpRequestException($"Failed to fetch internetaken: {response.ReasonPhrase}", null, response.StatusCode);
        }

        private async Task<InterneTaakOverviewItem> ExtendAndMapInternetaakToOverzichtItemAsync(Internetaak internetaak)
        {
            var item = new InterneTaakOverviewItem
            {
                Uuid = internetaak.Uuid,
                Nummer = internetaak.Nummer,
                GevraagdeHandeling = internetaak.GevraagdeHandeling,
                Status = internetaak.Status,
                ToegewezenOp = internetaak.ToegewezenOp,
                AfgehandeldOp = internetaak.AfgehandeldOp
            };

            //onderwerp en klantnaam toevoegen
            await LoadKlantcontactInfoAsync(internetaak.AanleidinggevendKlantcontact.Uuid, item);

            //behandelaar en afdelingnaam toevoegen 
            await LoadActorInfoAsync(internetaak, item);

            //urgentie alleen voor openstaande contactverzoeken, berekend op basis van ContactDatum
            item.Urgentie = internetaak.Status == KnownInternetaakStatussen.Verwerkt
                ? null
                : _urgentieBerekenService.Bereken(item.ContactDatum);

            return item;
        }

        private async Task LoadKlantcontactInfoAsync(Guid aanleidinggevendKlantcontactUuid, InterneTaakOverviewItem item)
        {
            if (aanleidinggevendKlantcontactUuid == default)
                return;

            try
            {
                var klantcontact = await GetKlantcontactAsync(aanleidinggevendKlantcontactUuid);
                if (klantcontact != null)
                {
                    item.Onderwerp = klantcontact.Onderwerp;
                    item.ContactDatum = klantcontact.PlaatsgevondenOp;
                    item.KlantNaam = ExtractKlantNaamFromKlantcontact(klantcontact);
                    item.ContactmomentNummer = klantcontact.Nummer;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading klantcontact {KlantcontactUuid}",
                    aanleidinggevendKlantcontactUuid);
            }
        }

        private async Task LoadActorInfoAsync(Internetaak internetaak, InterneTaakOverviewItem item)
        {
            if (internetaak.ToegewezenAanActoren?.Any() != true)
                return;

            var actorTasks = internetaak.ToegewezenAanActoren
                .Where(actorRef => !string.IsNullOrEmpty(actorRef.Uuid))
                .Select(actorRef => GetActorAsync(actorRef.Uuid));

            var actors = await Task.WhenAll(actorTasks);

            var medewerkerActor = actors.FirstOrDefault(a => a?.SoortActor == SoortActor.medewerker);
            if (medewerkerActor != null)
            {
                item.BehandelaarNaam = medewerkerActor.Naam;
            }

            var afdelingActors = actors
                .Where(a => a?.SoortActor != SoortActor.medewerker && !string.IsNullOrEmpty(a?.Naam))
                .Select(a => a!.Naam)
                .ToList();

            if (afdelingActors.Count != 0)
            {
                item.AfdelingNaam = string.Join(", ", afdelingActors);
            }
        }

        private async Task<Klantcontact?> GetKlantcontactAsync(Guid uuid)
        {
            try
            {
                return await _klantcontactService.GetKlantcontactAsync(uuid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch klantcontact {Uuid}", uuid);
                return null;
            }
        }

        private async Task<Actor?> GetActorAsync(string uuid)
        {
            try
            {
                return await _actorService.GetActorAsync(uuid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch actor {Uuid}", uuid);
                return null;
            }
        }

        private static string? ExtractKlantNaamFromKlantcontact(Klantcontact klantcontact)
        {
            return klantcontact.Expand?.HadBetrokkenen?
                .Select(b => b.VolledigeNaam ?? b.Organisatienaam)
                .FirstOrDefault(naam => !string.IsNullOrEmpty(naam));
        }
    }
}
