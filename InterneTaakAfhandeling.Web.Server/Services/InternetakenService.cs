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
    public class InternetakenService(IOpenKlantApiClient openKlantApiClient, IZakenApiClient zakenApiClient) : IInternetakenService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IZakenApiClient _zakenApiClient = zakenApiClient;

        public async Task<Internetaken> Get(InterneTaakQueryParameters interneTaakQueryParameters)
        {
            var interneTaakQuery = new InterneTaakQuery
            {
                Klantcontact__Nummer = interneTaakQueryParameters.Klantcontact_Nummer
            };

            var internetaken = await _openKlantApiClient.QueryInterneTaakAsync(interneTaakQuery);
            var onderwerpObjectId = internetaken?.AanleidinggevendKlantcontact?.Expand?.GingOverOnderwerpobjecten?.FirstOrDefault()?.Onderwerpobjectidentificator?.ObjectId;
            if (!string.IsNullOrEmpty(onderwerpObjectId))
            {
                internetaken.Zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
            }
            return internetaken;
        }
    }
}
