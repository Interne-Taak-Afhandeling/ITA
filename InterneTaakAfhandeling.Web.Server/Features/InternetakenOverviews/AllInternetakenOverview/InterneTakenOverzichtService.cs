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
        Task<InterneTakenOverviewResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTaakQuery);
    }

    public class InterneTakenOverzichtService : IInterneTakenOverzichtService
    {
        private readonly IUrgentieBerekenService _urgentieBerekenService;
        private readonly IKlantcontactService _klantcontactService;
        private readonly IInternetaakService  _internetaakService;
        private readonly IActorService _actorService;
        private readonly ILogger<InterneTakenOverzichtService> _logger;
        private readonly IOpenKlantApiClient _openKlantApiClient;

        public InterneTakenOverzichtService(
            IUrgentieBerekenService urgentieBerekenService,
            IKlantcontactService klantcontactService,
            IInternetaakService internetaakService,
            IActorService actorService,
            ILogger<InterneTakenOverzichtService> logger,
            IOpenKlantApiClient openKlantApiClient)
        {
            _urgentieBerekenService = urgentieBerekenService ?? throw new ArgumentNullException(nameof(urgentieBerekenService));
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _internetaakService = internetaakService ?? throw new ArgumentNullException(nameof(internetaakService));
            _actorService = actorService ?? throw new ArgumentNullException(nameof(actorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        }

        public async Task<InterneTakenOverviewResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTakenQuery)
        {                   
            var internetakenResponse = await GetInternetakenPaginatedAsync(interneTakenQuery);

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

            //urgentie berekenen op basis van ContactDatum
            item.Urgentie = _urgentieBerekenService.Bereken(item.ContactDatum);

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
