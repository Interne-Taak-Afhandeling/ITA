using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Middleware;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Web.Server.Services
{
    public interface IKlantcontactService
    {
        Task<Klantcontact?> GetEersteKlantcontactInKetenAsync(string klantcontactUuid);
        Task<List<Klantcontact>> BouwKlantcontactKetenAsync(string startKlantcontactUuid);
        Task<string> GetLaatsteKlantcontactUuidAsync(string klantcontactUuid);
        Task<Actor> GetOrCreateActorAsync(string email, string? naam = null);
        Task<Onderwerpobject> CreateOnderwerpobjectKlantcontactAsync(string klantcontactUuid, string wasKlantcontactUuid);
        Task<Betrokkene> CreateBetrokkeneAsync(string klantcontactUuid, string partijUuid);
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

        public async Task<Klantcontact?> GetEersteKlantcontactInKetenAsync(string klantcontactUuid)
        {
            try
            {
                var ketenVolgorde = await BouwKlantcontactKetenAsync(klantcontactUuid);

                // Eerste klantcontact is de laatste in de lijst (oudste eerst)
                return ketenVolgorde.Count > 0 ? ketenVolgorde[ketenVolgorde.Count - 1] : null;
            }
            catch (Exception ex)
            {
                if (Guid.TryParse(klantcontactUuid, out Guid parsedKlantcontactUuid))
                {
                    _logger.LogError(ex, $"Fout bij bepalen eerste klantcontact in keten met startpunt {parsedKlantcontactUuid}");
                }
                throw new ConflictException(
                    $"Fout bij bepalen eerste klantcontact in keten: {ex.Message}",
                    "KLANTCONTACT_KETEN_BEPALEN_FOUT");
            }
        }

        public async Task<string> GetLaatsteKlantcontactUuidAsync(string klantcontactUuid)
        {
            try
            {
                var ketenVolgorde = await BouwKlantcontactKetenAsync(klantcontactUuid);
                ketenVolgorde.Reverse(); // Nieuwste bovenaan

                return ketenVolgorde.Count > 0 ? ketenVolgorde[0].Uuid : klantcontactUuid;
            }
            catch (Exception ex)
            {
                if (Guid.TryParse(klantcontactUuid, out Guid parsedKlantcontactUuid))
                {
                    _logger.LogWarning(ex, $"Could not determine latest klantcontact in chain, using original {parsedKlantcontactUuid}");
                }

                return klantcontactUuid;
            }
        }

        public async Task<List<Klantcontact>> BouwKlantcontactKetenAsync(string startKlantcontactUuid)
        {
            var aanleidinggevendKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(startKlantcontactUuid);
            if (aanleidinggevendKlantcontact == null)
            {
                throw new ConflictException(
                    $"Klantcontact with UUID {startKlantcontactUuid} not found",
                    "KLANTCONTACT_NOT_FOUND");
            }

            var keten = new List<Klantcontact>();
            var verwerkte_uuids = new HashSet<string>();

            keten.Add(aanleidinggevendKlantcontact);
            verwerkte_uuids.Add(aanleidinggevendKlantcontact.Uuid);

            await VoegKlantcontactenToeAanKeten(aanleidinggevendKlantcontact.Uuid, keten, verwerkte_uuids);

            return keten;
        }

        private async Task VoegKlantcontactenToeAanKeten(
            string klantcontactUuid,
            List<Klantcontact> keten,
            HashSet<string> verwerkte_uuids)
        {
            var klantcontacten = await _openKlantApiClient.GetKlantcontactenByOnderwerpobjectIdentificatorObjectIdAsync(klantcontactUuid);

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

        public async Task<Actor> GetOrCreateActorAsync(string email, string? naam = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ConflictException(
                    "Email must not be empty",
                    "EMPTY_EMAIL_ADDRESS");
            }

            Actor? actor = null;
            try
            {
                actor = await _openKlantApiClient.GetActorByEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving actor by email {Mask(email)}");
                throw new ConflictException(
                    $"Error retrieving actor by email: {ex.Message}",
                    "ACTOR_RETRIEVAL_ERROR");
            }

            if (actor == null)
            {
                var actorRequest = new ActorRequest
                {
                    Naam = naam ?? email,
                    SoortActor = "medewerker",
                    IndicatieActief = true,
                    Actoridentificator = new Actoridentificator
                    {
                        ObjectId = email,
                        CodeObjecttype = "mdw",
                        CodeRegister = "msei",
                        CodeSoortObjectId = "email"
                    },
                    ActorIdentificatie = new ActorIdentificatie
                    {
                        Emailadres = email
                    }
                };

                try
                {
                    actor = await _openKlantApiClient.CreateActorAsync(actorRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating actor for {Mask(actorRequest.Naam)}");
                    throw new ConflictException(
                        $"Error creating actor: {ex.Message}",
                        "ACTOR_CREATION_ERROR");
                }
            }

            if (actor == null)
            {
                throw new ConflictException(
                    "Failed to get or create actor",
                    "ACTOR_CREATION_FAILED");
            }

            return actor;
        }

        private static string Mask(string value)
        {
            return value[..(value.Length < 6 ? value.Length : 5)];
        }

        public async Task<Onderwerpobject> CreateOnderwerpobjectKlantcontactAsync(
            string klantcontactUuid,
            string wasKlantcontactUuid)
        {

            var onderwerpobject = new Onderwerpobject
            {
                Uuid = string.Empty,
                Url = string.Empty,
                Klantcontact = new Klantcontact
                {
                    Uuid = klantcontactUuid,
                    Url = $"/klantinteracties/api/v1/klantcontacten/{klantcontactUuid}",
                    HadBetrokkenActoren = new List<Actor>()
                },
                WasKlantcontact = new Klantcontact
                {
                    Uuid = wasKlantcontactUuid,
                    Url = $"/klantinteracties/api/v1/klantcontacten/{wasKlantcontactUuid}",
                    HadBetrokkenActoren = new List<Actor>()
                },
                Onderwerpobjectidentificator = new Onderwerpobjectidentificator
                {
                    ObjectId = wasKlantcontactUuid,
                    CodeObjecttype = "klantcontact",
                    CodeRegister = "openklant",
                    CodeSoortObjectId = "uuid"
                }
            };

            try
            {
                return await _openKlantApiClient.CreateOnderwerpobjectAsync(onderwerpobject);
            }
            catch (Exception ex)
            {
                throw new ConflictException(
                    $"Error creating onderwerpobject: {ex.Message}",
                    "ONDERWERPOBJECT_CREATION_ERROR");
            }
        }

        public async Task<Betrokkene> CreateBetrokkeneAsync(string klantcontactUuid, string partijUuid)
        {
            if (string.IsNullOrEmpty(klantcontactUuid) || string.IsNullOrEmpty(partijUuid))
            {
                throw new ConflictException(
                    "Both klantcontact UUID and partij UUID are required to create a betrokkene",
                    "MISSING_REQUIRED_UUIDS");
            }

            var betrokkeneRequest = new BetrokkeneRequest
            {
                WasPartij = new PartijReference
                {
                    Uuid = partijUuid
                },
                HadKlantcontact = new KlantcontactReference
                {
                    Uuid = klantcontactUuid
                },
                Rol = "klant",
                Initiator = true
            };

            try
            {
                if (Guid.TryParse(klantcontactUuid, out Guid parsedKlantcontactUuid))
                {
                    if (Guid.TryParse(partijUuid, out Guid parsedPartijUuid))
                    {
                        _logger.LogInformation($"Creating betrokkene for klantcontact {parsedKlantcontactUuid} and partij {parsedPartijUuid}");
                    }
                }

                var betrokkene = await _openKlantApiClient.CreateBetrokkeneAsync(betrokkeneRequest);
                _logger.LogInformation($"Successfully created betrokkene {betrokkene.Uuid}");
                return betrokkene;
            }
            catch (Exception ex)
            {
                if (Guid.TryParse(klantcontactUuid, out Guid parsedKlantcontactUuid))
                {
                    if (Guid.TryParse(partijUuid, out Guid parsedPartijUuid))
                    {
                        _logger.LogError(ex, $"Error creating betrokkene for klantcontact {parsedKlantcontactUuid} and partij {parsedPartijUuid}");
                    }
                }

                throw new ConflictException(
                    $"Error creating betrokkene: {ex.Message}",
                    "BETROKKENE_CREATION_ERROR");
            }
        }
    }
}