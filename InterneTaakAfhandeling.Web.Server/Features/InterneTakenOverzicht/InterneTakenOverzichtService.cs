using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using System;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht
{
    public interface IInterneTakenOverzichtService
    {
        Task<InterneTakenOverzichtResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTaakQuery);
    }

    public class InterneTakenOverzichtService : IInterneTakenOverzichtService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly ILogger<InterneTakenOverzichtService> _logger;

        public InterneTakenOverzichtService(
            IOpenKlantApiClient openKlantApiClient,
            ILogger<InterneTakenOverzichtService> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<InterneTakenOverzichtResponse> GetInterneTakenOverzichtAsync(InterneTaakQuery interneTakenQuery)
        {                   
            var internetakenResponse = await _openKlantApiClient.GetAllInternetakenAsync(interneTakenQuery);

            var OverzichtItems = (await Task.WhenAll(internetakenResponse.Results
                .Select(ExtendAndMapInternetaakToOverzichtItemAsync))).ToList();

            return new InterneTakenOverzichtResponse
            {
                Count = internetakenResponse.Count,
                Next = internetakenResponse.Next,
                Previous = internetakenResponse.Previous,
                Results = OverzichtItems
            };
        }

        private async Task<InterneTaakOverzichtItem> ExtendAndMapInternetaakToOverzichtItemAsync(Internetaak internetaak)
        {
            var item = new InterneTaakOverzichtItem
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

            return item;
        }

        private async Task LoadKlantcontactInfoAsync(Guid aanleidinggevendKlantcontactUuid, InterneTaakOverzichtItem item)
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading klantcontact {KlantcontactUuid}",
                    aanleidinggevendKlantcontactUuid);
            }
        }

        private async Task LoadActorInfoAsync(Internetaak internetaak, InterneTaakOverzichtItem item)
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
