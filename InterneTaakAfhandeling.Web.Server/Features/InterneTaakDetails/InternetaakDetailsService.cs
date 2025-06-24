using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Web.Server.Features.Internetaken;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaak
{
    public interface IInternetaakService
    {
        Task<Internetaak?> Get(InterneTaakQuery interneTaakQueryParameters);
    }
    public class InternetaakDetailsService(IOpenKlantApiClient openKlantApiClient, IZakenApiClient zakenApiClient, IContactmomentenService contactmomentenService) : IInternetaakService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IZakenApiClient _zakenApiClient = zakenApiClient;
        private readonly IContactmomentenService _contactmomentenService = contactmomentenService;

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
            }


            return interntaak;
        }
    }
}
