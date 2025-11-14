using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IKlantcontactService
    {
        Task<Klantcontact?> GetEersteKlantcontactInKetenAsync(Guid klantcontactUuid);
        Task<List<Klantcontact>> BouwKlantcontactKetenAsync(Guid startKlantcontactUuid);
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
            var aanleidinggevendKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(startKlantcontactUuid) ?? throw new ConflictException(
                    $"Klantcontact with UUID {startKlantcontactUuid} not found",
                    "KLANTCONTACT_NOT_FOUND");
            var keten = new List<Klantcontact>();
            var verwerkte_uuids = new HashSet<Guid>() { aanleidinggevendKlantcontact.Uuid };

            await VoegKlantcontactenToeAanKeten(aanleidinggevendKlantcontact.Uuid, keten, verwerkte_uuids);

            return keten;
        }

        private async Task VoegKlantcontactenToeAanKeten(
            Guid klantcontactUuid,
            List<Klantcontact> keten,
            HashSet<Guid> verwerkte_uuids)
        {
            var klantcontacten = await _openKlantApiClient.GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(klantcontactUuid.ToString());

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
    }
}
