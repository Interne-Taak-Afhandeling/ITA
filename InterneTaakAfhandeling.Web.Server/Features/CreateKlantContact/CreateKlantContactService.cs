using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Web.Server.Services;
using System;

namespace InterneTaakAfhandeling.Web.Server.Features.CreateKlantContact
{
    public interface ICreateKlantContactService
    {
        Task<RelatedKlantcontactResult> CreateRelatedKlantcontactAsync(
            KlantcontactRequest klantcontactRequest,
            string? previousKlantcontactUuid,
            string userEmail,
            string? userName,
            string? partijUuid = null);
    }

    public class CreateKlantContactService : ICreateKlantContactService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly IKlantcontactService _klantcontactService;
        private readonly ILogger<CreateKlantContactService> _logger;

        public CreateKlantContactService(
            IOpenKlantApiClient openKlantApiClient,
            IKlantcontactService klantcontactService,
            ILogger<CreateKlantContactService> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RelatedKlantcontactResult> CreateRelatedKlantcontactAsync(
            KlantcontactRequest klantcontactRequest,
            string? aanleidinggevendKlantcontactUuid,
            string userEmail,
            string? userName,
            string? partijUuid = null)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ConflictException(
                    "No email found for the current user.",
                    "MISSING_EMAIL_CLAIM");
            }

            var laatsteKlantcontactUuid = string.Empty;
            if (!string.IsNullOrEmpty(aanleidinggevendKlantcontactUuid))
            {
                try
                {
                    //because there is no proper way to sort klantcontacten by date (date is optional),
                    //we don't add all new klantcontacten that take place during the handling of an internetaak to the original klantcontact
                    //instead, we add each klantcontact to the previous klantcontact. this creates a chain of klantcontacten
                    //here we have to find the last one in the change and we will link the new klantcontact that we are creating here to that one 
                    laatsteKlantcontactUuid = await _klantcontactService.GetLaatsteKlantcontactUuidAsync(aanleidinggevendKlantcontactUuid);

                    if (!string.IsNullOrEmpty(laatsteKlantcontactUuid) && laatsteKlantcontactUuid != aanleidinggevendKlantcontactUuid)
                    {
                        if (Guid.TryParse(laatsteKlantcontactUuid, out Guid parsedLaatsteKlantcontactUuid))
                        {
                            if (Guid.TryParse(aanleidinggevendKlantcontactUuid, out Guid parsedAanleidinggevendKlantcontactUuid))
                            {
                                _logger.LogInformation($"Using most recent klantcontact in chain: {parsedLaatsteKlantcontactUuid} " + $"instead of original: {parsedAanleidinggevendKlantcontactUuid}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Guid.TryParse(aanleidinggevendKlantcontactUuid, out Guid parsedAanleidinggevendKlantcontactUuid))
                    {
                        _logger.LogWarning(ex, $"Could not determine latest contact in chain. Using original: {parsedAanleidinggevendKlantcontactUuid}");
                    }
                }
            }

            var actor = await GetOrCreateActorAsync(userEmail, userName);
            var klantcontact = await _openKlantApiClient.CreateKlantcontactAsync(klantcontactRequest);
            var actorKlantcontactRequest = new ActorKlantcontactRequest
            {
                Actor = new ActorReference { Uuid = actor.Uuid },
                Klantcontact = new KlantcontactReference { Uuid = klantcontact.Uuid }
            };
            var actorKlantcontact = await _openKlantApiClient.CreateActorKlantcontactAsync(actorKlantcontactRequest);
            var result = new RelatedKlantcontactResult
            {
                Klantcontact = klantcontact,
                ActorKlantcontact = actorKlantcontact
            };

            if (!string.IsNullOrEmpty(laatsteKlantcontactUuid))
            {
                var onderwerpobject = await CreateOnderwerpobjectKlantcontactAsync(
                    klantcontact.Uuid,
                    laatsteKlantcontactUuid);

                result.Onderwerpobject = onderwerpobject;
            }

            await CreateBetrokkeneRelationshipAsync(result, klantcontact.Uuid, partijUuid);

            return result;
        }

        private async Task CreateBetrokkeneRelationshipAsync(
            RelatedKlantcontactResult result,
            string klantcontactUuid,
            string? providedPartijUuid)
        {
            try
            {
                if (!string.IsNullOrEmpty(providedPartijUuid))
                {
                    if (Guid.TryParse(providedPartijUuid, out Guid parsedprovidedPartijUuid))
                    {
                        _logger.LogInformation($"Creating betrokkene using provided partijUuid: {parsedprovidedPartijUuid}");
                    }

                    var betrokkene = await CreateBetrokkeneAsync(klantcontactUuid, providedPartijUuid);
                    result.Betrokkene = betrokkene;
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not create betrokkene for klantcontact {klantcontactUuid}");
            }
        }

        private async Task<Actor> GetOrCreateActorAsync(string email, string? naam = null)
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
                var safeEmailId = SecureLogging.CreateSafeIdentifier(email);
                _logger.LogError(ex, $"Error retrieving actor by email identifier {safeEmailId}");
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
                    var safeActorId = SecureLogging.CreateSafeIdentifier(actorRequest.Naam);
                    _logger.LogError(ex, $"Error creating actor with identifier {safeActorId}");
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

        private async Task<Onderwerpobject> CreateOnderwerpobjectKlantcontactAsync(
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

        private async Task<Betrokkene> CreateBetrokkeneAsync(string klantcontactUuid, string partijUuid)
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
                var safeKlantcontactUuid = SecureLogging.SanitizeUuid(klantcontactUuid);
                var safePartijUuid = SecureLogging.SanitizeUuid(partijUuid);
                _logger.LogInformation($"Creating betrokkene for klantcontact {safeKlantcontactUuid} and partij {safePartijUuid}");

                var betrokkene = await _openKlantApiClient.CreateBetrokkeneAsync(betrokkeneRequest);

                var safeBetrokkeneUuid = SecureLogging.SanitizeUuid(betrokkene.Uuid);
                _logger.LogInformation($"Successfully created betrokkene {safeBetrokkeneUuid}");

                return betrokkene;
            }
            catch (Exception ex)
            {
                var safeKlantcontactUuid = SecureLogging.SanitizeUuid(klantcontactUuid);
                var safePartijUuid = SecureLogging.SanitizeUuid(partijUuid);
                _logger.LogError(ex, $"Error creating betrokkene for klantcontact {safeKlantcontactUuid} and partij {safePartijUuid}");

                throw new ConflictException(
                    $"Error creating betrokkene: {ex.Message}",
                    "BETROKKENE_CREATION_ERROR");
            }
        }
    }

    public class RelatedKlantcontactResult
    {
        public Klantcontact Klantcontact { get; set; }
        public ActorKlantcontact ActorKlantcontact { get; set; }
        public Onderwerpobject? Onderwerpobject { get; set; }
        public Betrokkene? Betrokkene { get; set; }
    }
}