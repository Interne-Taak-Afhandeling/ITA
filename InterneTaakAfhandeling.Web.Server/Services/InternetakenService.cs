using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IInternetakenService
    {
        Task<Internetaken> GetByNummerAsync(string nummer);
    }
    public class InternetakenService(IOpenKlantApiClient openKlantApiClient, IZakenApiClient zakenApiClient) : IInternetakenService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
        private readonly IZakenApiClient _zakenApiClient = zakenApiClient;

        public async Task<Internetaken> GetByNummerAsync(string nummer)
        {
            var internetaken = await _openKlantApiClient.GetInternetakenByNummerAsync(nummer);
            var onderwerpObjectId = internetaken.AanleidinggevendKlantcontact?.Expand?.GingOverOnderwerpobjecten?.FirstOrDefault()?.Onderwerpobjectidentificator?.ObjectId;
            if (!string.IsNullOrEmpty(onderwerpObjectId))
            {
                internetaken.Zaak = await _zakenApiClient.GetZaakAsync(onderwerpObjectId);
            }
            return internetaken;
        }
    }
}
