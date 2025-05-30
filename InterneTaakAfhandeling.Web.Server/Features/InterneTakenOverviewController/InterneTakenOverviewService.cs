using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverview;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IInterneTakenOverviewService
    {
        Task<InterneTakenOverviewResponse> GetInterneTakenOverviewAsync(InterneTakenOverviewQueryParameters queryParameters);
    }

    public class InterneTakenOverviewService : IInterneTakenOverviewService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly ILogger<InterneTakenOverviewService> _logger;

        public InterneTakenOverviewService(
            IOpenKlantApiClient openKlantApiClient,
            ILogger<InterneTakenOverviewService> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<InterneTakenOverviewResponse> GetInterneTakenOverviewAsync(InterneTakenOverviewQueryParameters queryParameters)
        {
            var page = queryParameters.GetValidatedPage();
            var pageSize = queryParameters.GetValidatedPageSize();

            //refactoring suggestion: there is a _openKlantApiClient.QueryInterneTakenAsync that could be used for this (with some minor refactoring)
            var internetakenResponse = await _openKlantApiClient.GetAllInternetakenAsync(new InterneTaakQuery
            {
                Status = "te_verwerken",
                Page = page,
                PageSize = pageSize
            });

            var overviewItemTasks = internetakenResponse.Results
                .Select(internetaak => MapInternetaakToOverviewItemAsync(internetaak))
                .ToList();

            var overviewItems = (await Task.WhenAll(overviewItemTasks))
               .OrderByDescending(x => x.ToegewezenOp)
               .ToList();

            return new InterneTakenOverviewResponse
            {
                Count = internetakenResponse.Count,
                Next = internetakenResponse.Next,
                Previous = internetakenResponse.Previous,
                Results = overviewItems
            };
        }

        private async Task<InterneTaakOverviewItem> MapInternetaakToOverviewItemAsync(Internetaak internetaak)
        {
            var item = new InterneTaakOverviewItem
            {
                Uuid = internetaak.Uuid,
                Nummer = internetaak.Nummer ?? string.Empty,
                GevraagdeHandeling = internetaak.GevraagdeHandeling ?? string.Empty,
                Status = internetaak.Status ?? string.Empty,
                ToegewezenOp = internetaak.ToegewezenOp ?? DateTimeOffset.MinValue,
                AfgehandeldOp = internetaak.AfgehandeldOp
            };

            await LoadKlantcontactInfoAsync(internetaak, item);
            await LoadActorInfoAsync(internetaak, item);

            return item;
        }

        private async Task LoadKlantcontactInfoAsync(Internetaak internetaak, InterneTaakOverviewItem item)
        {
            if (internetaak.AanleidinggevendKlantcontact == null)
                return;

            try
            {
                var klantcontact = await GetKlantcontactAsync(internetaak.AanleidinggevendKlantcontact.Uuid);
                if (klantcontact != null)
                {
                    item.Onderwerp = klantcontact.Onderwerp;
                    item.ContactDatum = klantcontact.PlaatsgevondenOp;
                    item.KlantNaam = ExtractKlantNaamFromKlantcontact(klantcontact);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading klantcontact {KlantcontactUuid} for internetaak {InternetaakUuid}",
                    internetaak.AanleidinggevendKlantcontact.Uuid, internetaak.Uuid);
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

            if (afdelingActors.Any())
            {
                item.AfdelingNaam = string.Join(", ", afdelingActors);
            }
        }

        private async Task<Klantcontact?> GetKlantcontactAsync(string uuid)
        {
            try
            {
                _logger.LogInformation("Klantcontact {Uuid} fetched directly from API", uuid);
                return await _openKlantApiClient.GetKlantcontactAsync(uuid);
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
                _logger.LogInformation("Actor {Uuid} fetched directly from API", uuid);
                return await _openKlantApiClient.GetActorAsync(uuid);
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