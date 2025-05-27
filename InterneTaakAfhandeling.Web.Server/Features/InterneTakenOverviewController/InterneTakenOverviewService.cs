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

            _logger.LogDebug("Fetching interne taken with status 'te_verwerken', page {Page}, pageSize {PageSize}", page, pageSize);

            // Use the clean API method - only does the HTTP call
            var internetakenResponse = await _openKlantApiClient.GetAllInternetakenAsync(new InterneTaakQuery
            {
                Status = "te_verwerken",
                Page = page,
                PageSize = pageSize
            });

            var overviewItems = new List<InterneTaakOverviewItem>();

            foreach (var internetaak in internetakenResponse.Results)
            {
                // Do the business logic here in the service layer
                var overviewItem = await MapToOverviewItemAsync(internetaak);
                overviewItems.Add(overviewItem);
            }

            // Sort by oldest first (ToegewezenOp ascending)
            overviewItems = overviewItems.OrderBy(x => x.ToegewezenOp).ToList();

            return new InterneTakenOverviewResponse
            {
                Count = internetakenResponse.Count,
                Next = internetakenResponse.Next,
                Previous = internetakenResponse.Previous,
                Results = overviewItems
            };
        }

        private async Task<InterneTaakOverviewItem> MapToOverviewItemAsync(Internetaken internetaak)
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

            // Load contact information (business logic in service)
            if (internetaak.AanleidinggevendKlantcontact != null)
            {
                var klantcontact = await _openKlantApiClient.GetKlantcontactAsync(internetaak.AanleidinggevendKlantcontact.Uuid);
                if (klantcontact != null)
                {
                    item.Onderwerp = klantcontact.Onderwerp;
                    item.ContactDatum = klantcontact.PlaatsgevondenOp;
                    item.KlantNaam = GetKlantNaam(klantcontact);
                }
            }

            // Load assignment information (business logic in service)
            await MapAssignmentInfoAsync(internetaak, item);

            return item;
        }

        private async Task MapAssignmentInfoAsync(Internetaken internetaak, InterneTaakOverviewItem item)
        {
            if (internetaak.ToegewezenAanActoren?.Any() == true)
            {
                var actorTasks = internetaak.ToegewezenAanActoren
                    .Where(actorRef => !string.IsNullOrEmpty(actorRef.Uuid))
                    .Select(actorRef => _openKlantApiClient.GetActorAsync(actorRef.Uuid));

                var actors = await Task.WhenAll(actorTasks);

                // Find the first medewerker (employee) for behandelaar
                var medewerkerActor = actors.FirstOrDefault(a => a?.SoortActor == SoortActor.medewerker);
                if (medewerkerActor != null)
                {
                    item.BehandelaarNaam = medewerkerActor.Naam;
                }

                // Find department/organizational unit actors for afdeling
                var afdelingActors = actors
                    .Where(a => a?.SoortActor != SoortActor.medewerker && !string.IsNullOrEmpty(a?.Naam))
                    .Select(a => a!.Naam)
                    .ToList();

                if (afdelingActors.Any())
                {
                    item.AfdelingNaam = string.Join(", ", afdelingActors);
                }
            }
        }

        private static string? GetKlantNaam(Klantcontact klantcontact)
        {
            return klantcontact.Expand?.HadBetrokkenen?
                .Select(b => b.VolledigeNaam ?? b.Organisatienaam)
                .FirstOrDefault(naam => !string.IsNullOrEmpty(naam));
        }
    }
}