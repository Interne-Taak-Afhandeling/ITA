using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaak
{
    public interface IInternetaakService
    {
        Task<Internetaak?> Get(InterneTaakQuery interneTaakQueryParameters);
        Task<Internetaak?> GetByKlantcontactNummer(string klantcontactNummer);
    }
    public class InternetaakDetailsService(IOpenKlantApiClient openKlantApiClient, IZakenApiClient zakenApiClient, IContactmomentenService contactmomentenService, ILogger<InternetaakDetailsService> logger) : IInternetaakService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IZakenApiClient _zakenApiClient = zakenApiClient;
        private readonly IContactmomentenService _contactmomentenService = contactmomentenService;
        private readonly ILogger<InternetaakDetailsService> _logger = logger;

        public async Task<Internetaak?> Get(InterneTaakQuery interneTaakQueryParameters)
        {
            var interneTaakQuery = new InterneTaakQuery
            {
                Nummer = interneTaakQueryParameters.Nummer
            };

            var internetaken = await _openKlantApiClient.QueryInterneTakenAsync(interneTaakQuery);


            if(internetaken?.Count != 1)
            {
                return null;
            }

            var interntaak = internetaken.Single();

            await EnrichWithZaakAsync(interntaak);
            await EnrichBetrokkenenWithPartijDigitaleAdressenAsync(interntaak);

            return interntaak;
        }

        public async Task<Internetaak?> GetByKlantcontactNummer(string klantcontactNummer)
        {
            var query = new InterneTaakQuery
            {
                Klantcontact__Nummer = klantcontactNummer
            };

            var internetaken = await _openKlantApiClient.QueryInterneTakenAsync(query);

            if (internetaken == null || internetaken.Count == 0)
            {
                return null;
            }

            if (internetaken.Count > 1)
            {
                var uuids = internetaken.Select(t => t.Uuid).ToList();

                _logger.LogWarning(
                    "Meerdere interne taken ({Count}) gevonden voor klantcontact nummer. Kan niet bepalen welke de juiste is. Interne taak UUID's: {Uuids}",
                    internetaken.Count,
                    uuids);

                throw new ConflictException(
                    $"Meerdere interne taken gevonden voor klantcontact met nummer: {klantcontactNummer}. Kan niet bepalen welke de juiste is.");
            }

            var interntaak = internetaken.Single();

            await EnrichWithZaakAsync(interntaak);
            await EnrichBetrokkenenWithPartijDigitaleAdressenAsync(interntaak);

            return interntaak;
        }

        private async Task EnrichWithZaakAsync(Internetaak? interntaak)
        {
            if (interntaak?.AanleidinggevendKlantcontact != null)
            {
                var onderwerpObjectId = _contactmomentenService.GetZaakOnderwerpObject(interntaak.AanleidinggevendKlantcontact);

                if (!string.IsNullOrEmpty(onderwerpObjectId))
                {
                    interntaak.Zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
                }
            }
        }

        private async Task EnrichBetrokkenenWithPartijDigitaleAdressenAsync(Internetaak? internetaak)
        {
            var betrokkenen = internetaak?.AanleidinggevendKlantcontact?.Expand?.HadBetrokkenen;
            if (betrokkenen == null) return;

            foreach (var betrokkene in betrokkenen)
            {
                if (betrokkene.Expand?.DigitaleAdressen != null && betrokkene.Expand.DigitaleAdressen.Count > 0)
                    continue;

                var partijUuid = betrokkene.Expand?.WasPartij?.Uuid;
                if (string.IsNullOrEmpty(partijUuid))
                    continue;

                try
                {
                    _logger.LogInformation("Fallback: ophalen digitale adressen van partij {PartijUuid} voor betrokkene {BetrokkeneUuid}",
                        partijUuid, betrokkene.Uuid);

                    var partijAdressen = await _openKlantApiClient.GetPartijDigitaleAdressenAsync(partijUuid);

                    betrokkene.Expand ??= new BetrokkeneExpand();
                    betrokkene.Expand.DigitaleAdressen = partijAdressen;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fallback partij-lookup gefaald voor partij {PartijUuid}: {Message}",
                        partijUuid, ex.Message);
                }
            }
        }
    }
}
