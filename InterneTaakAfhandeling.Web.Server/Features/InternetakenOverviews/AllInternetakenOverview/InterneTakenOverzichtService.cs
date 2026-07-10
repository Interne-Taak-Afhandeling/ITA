using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.AllInternetakenOverview
{
    public interface IInterneTakenOverzichtService
    {
        Task<InterneTakenOverviewResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTaakQuery);
    }

    public class InterneTakenOverzichtService : IInterneTakenOverzichtService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly IUrgentieBerekenService _urgentieBerekenService;
        private readonly ILogger<InterneTakenOverzichtService> _logger;

        public InterneTakenOverzichtService(
            IOpenKlantApiClient openKlantApiClient,
            IUrgentieBerekenService urgentieBerekenService,
            ILogger<InterneTakenOverzichtService> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _urgentieBerekenService = urgentieBerekenService ?? throw new ArgumentNullException(nameof(urgentieBerekenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<InterneTakenOverviewResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTakenQuery)
        {                   
            var internetakenResponse = await _openKlantApiClient.GetInternetakenPaginatedAsync(interneTakenQuery);

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
