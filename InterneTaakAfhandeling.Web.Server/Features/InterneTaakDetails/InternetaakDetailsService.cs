using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaak
{
    public interface IInternetaakService
    {
        Task<Internetaak?> Get(InterneTaakQuery interneTaakQueryParameters);
        Task<Internetaak?> GetByKlantcontactNummer(string klantcontactNummer);
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

            await EnrichWithZaakAsync(interntaak);

            return interntaak;
        }

        public async Task<Internetaak?> GetByKlantcontactNummer(string klantcontactNummer)
        {
            var query = new InterneTaakQuery
            {
                Klantcontact__Nummer = klantcontactNummer
            };

            var internetaken = await _openKlantApiClient.QueryInterneTakenAsync(query);

            if (internetaken == null || internetaken.Count != 1)
            {
                return null;
            }

            var interntaak = internetaken.Single();

            await EnrichWithZaakAsync(interntaak);

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
    }
}
