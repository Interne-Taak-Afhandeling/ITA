using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaak
{
    public interface IInternetaakService
    {
        Task<Internetaak?> Get(InterneTaakQuery interneTaakQueryParameters);
    }
    public class InternetaakDetailsService(
        IOpenKlantApiClient openKlantApiClient,
        IZakenApiClient zakenApiClient,
        IContactmomentenService contactmomentenService,
        IKlantcontactService klantcontactService,
        ILogger<InternetaakDetailsService> logger) : IInternetaakService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IZakenApiClient _zakenApiClient = zakenApiClient;
        private readonly IContactmomentenService _contactmomentenService = contactmomentenService;
        private readonly IKlantcontactService _klantcontactService = klantcontactService;
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

            if (interntaak?.AanleidinggevendKlantcontact != null)
            {
                var onderwerpObjectId = _contactmomentenService.GetZaakOnderwerpObject(interntaak.AanleidinggevendKlantcontact);

                if (!string.IsNullOrEmpty(onderwerpObjectId))
                {
                    interntaak.Zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
                }

                await ResolveOrigineleContactmomentNummerAsync(interntaak);
            }


            return interntaak;
        }

        private async Task ResolveOrigineleContactmomentNummerAsync(Internetaak internetaak)
        {
            try
            {
                var eersteKlantcontact = await _klantcontactService.GetEersteKlantcontactInKetenAsync(
                    internetaak.AanleidinggevendKlantcontact.Uuid);

                internetaak.OrigineleContactmomentNummer = eersteKlantcontact?.Nummer
                    ?? internetaak.AanleidinggevendKlantcontact.Nummer;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Kon originele contactmoment-nummer niet bepalen voor internetaak {Nummer}, fallback naar aanleidinggevend klantcontact",
                    internetaak.Nummer);
                internetaak.OrigineleContactmomentNummer = internetaak.AanleidinggevendKlantcontact.Nummer;
            }
        }
    }
}
