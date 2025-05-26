using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IInternetakenService
    {
        Task<Internetaken> Get(InterneTaakQueryParameters interneTaakQueryParameters);
    }
    public class InternetakenService(IOpenKlantApiClient openKlantApiClient, IZakenApiClient zakenApiClient, IContactmomentenService contactmomentenService) : IInternetakenService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IZakenApiClient _zakenApiClient = zakenApiClient;
        private readonly IContactmomentenService _contactmomentenService = contactmomentenService;

        public async Task<Internetaken> Get(InterneTaakQueryParameters interneTaakQueryParameters)
        {
            var interneTaakQuery = new InterneTaakQuery
            {
                Nummer = interneTaakQueryParameters.Nummer
            };

            var internetaken = await _openKlantApiClient.QueryInterneTaakAsync(interneTaakQuery);
            if (internetaken?.AanleidinggevendKlantcontact != null)
            {
                var onderwerpObjectId = _contactmomentenService.GetZaakOnderwerpObject(internetaken.AanleidinggevendKlantcontact);

                if (!string.IsNullOrEmpty(onderwerpObjectId))
                {
                    internetaken.Zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);                                     
                }
            }


            return internetaken;
        }
    }
}
