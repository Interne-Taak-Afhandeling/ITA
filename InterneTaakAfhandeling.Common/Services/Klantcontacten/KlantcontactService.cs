using System.Net.Http.Json;
using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Common.Services.Klantcontacten
{
    public interface IKlantcontactService
    {
        Task<Klantcontact?> GetEersteKlantcontactInKetenAsync(Guid klantcontactUuid);
        Task<List<Klantcontact>> BouwKlantcontactKetenAsync(Guid startKlantcontactUuid);
        Task<Klantcontact> GetKlantcontactAsync(Guid uuid);
        Task<List<Klantcontact>> QueryKlantcontactAsync(KlantcontactQuery query);
        Task<Onderwerpobject?> GetOnderwerpobjectAsync(Guid uuid);
    }

    public class KlantcontactService : IKlantcontactService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly ILogger<KlantcontactService> _logger;

        public KlantcontactService(
            IOpenKlantApiClient openKlantApiClient,
            ILogger<KlantcontactService> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Klantcontact?> GetEersteKlantcontactInKetenAsync(Guid klantcontactUuid)
        {
            try
            {
                var ketenVolgorde = await BouwKlantcontactKetenAsync(klantcontactUuid);

                // Eerste klantcontact is de laatste in de lijst (oudste eerst)
                return ketenVolgorde.Count > 0 ? ketenVolgorde[ketenVolgorde.Count - 1] : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij bepalen eerste klantcontact in keten met startpunt {ParsedKlantcontactUuid}",
                    klantcontactUuid);
                throw new ConflictException(
                    $"Fout bij bepalen eerste klantcontact in keten: {ex.Message}",
                    "KLANTCONTACT_KETEN_BEPALEN_FOUT");
            }
        }

        public async Task<List<Klantcontact>> BouwKlantcontactKetenAsync(Guid startKlantcontactUuid)
        {
            var aanleidinggevendKlantcontact = await GetKlantcontactAsync(startKlantcontactUuid) ?? throw new ConflictException(
                    $"Klantcontact with UUID {startKlantcontactUuid} not found",
                    "KLANTCONTACT_NOT_FOUND");
            var keten = new List<Klantcontact>();
            var verwerkte_uuids = new HashSet<Guid>() { aanleidinggevendKlantcontact.Uuid };

            await VoegKlantcontactenToeAanKeten(aanleidinggevendKlantcontact.Uuid, keten, verwerkte_uuids);

            return keten;
        }
        
        public async Task<Klantcontact> GetKlantcontactAsync(Guid uuid)
        {
            _logger.LogInformation("Fetching klantcontact {Uuid}", uuid);

            var klantcontact = await _openKlantApiClient.GetAsync<Klantcontact>(
                $"klantcontacten/{uuid}?expand=leiddeTotInterneTaken,gingOverOnderwerpobjecten,hadBetrokkenen,hadBetrokkenen.digitaleAdressen,hadBetrokkenen.wasPartij");
        
            _logger.LogInformation("Onderwerpobjecten count: {Count}", klantcontact?.GingOverOnderwerpobjecten?.Count ?? 0);
            foreach (var obj in klantcontact?.GingOverOnderwerpobjecten ?? [])
            {
                _logger.LogInformation("Onderwerpobject: {Uuid}, Identificator: {Id}",
                    obj.Uuid,
                    SecureLogging.SanitizeAndTruncate(obj.Onderwerpobjectidentificator?.ObjectId));
            }

            return klantcontact;
        }
        
        public async Task<List<Klantcontact>> QueryKlantcontactAsync(KlantcontactQuery query)
        {
            var klantcontacten = await _openKlantApiClient.GetAsync<KlantcontactResponse>($"klantcontacten?{query.BuildQueryString()}");
            return klantcontacten?.Results ?? [];
        }
        
        public async Task<Onderwerpobject?> GetOnderwerpobjectAsync(Guid uuid)
        {
            var response = await _openKlantApiClient.GetAsyncHttpResponse($"onderwerpobjecten/{uuid}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Onderwerpobject>();
        }

        private async Task VoegKlantcontactenToeAanKeten(
            Guid klantcontactUuid,
            List<Klantcontact> keten,
            HashSet<Guid> verwerkte_uuids)
        {
            var klantcontacten = await GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(klantcontactUuid.ToString());

            foreach (var klantcontact in klantcontacten)
            {
                if (!verwerkte_uuids.Contains(klantcontact.Uuid))
                {
                    keten.Add(klantcontact);
                    verwerkte_uuids.Add(klantcontact.Uuid);

                    await VoegKlantcontactenToeAanKeten(klantcontact.Uuid, keten, verwerkte_uuids);
                }
            }
        }
        
        private async Task<List<Klantcontact>> GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(string objectId)
        {
            try
            {
                _logger.LogInformation("Fetching klantcontacten with onderwerpobject__onderwerpobjectidentificatorObjectId={ObjectId}", objectId);

                // Query om klantcontacten te vinden die verwijzen naar dit objectId via onderwerpobjectidentificator

                //  var response = await _httpClient.GetAsync($"partijen?partijIdentificator__objectId={bsn}");
                // response.EnsureSuccessStatusCode();
            
                var klantcontactResponse =
                    await _openKlantApiClient.GetAsync<KlantcontactResponse>(
                        $"klantcontacten?onderwerpobject__onderwerpobjectidentificatorObjectId={objectId}");
            
                var klantcontacten = klantcontactResponse?.Results ?? [];

                _logger.LogInformation("Found {Count} klantcontacten referring to objectId {ObjectId}",
                    klantcontacten.Count, objectId);

                return klantcontacten;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving klantcontacten by onderwerpobject ObjectId: {Message}", ex.Message);
                return [];
            }
        }
        
        private class KlantcontactResponse
        {
            public int Count { get; set; }
            public string? Next { get; set; }
            public string? Previous { get; set; }
            public List<Klantcontact> Results { get; set; } = [];
        }
    }
}
