using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Web.Server.Services;
using static InterneTaakAfhandeling.Common.Services.OpenKlantApi.OpenKlantApiClient;
using InterneTaakAfhandeling.Common.Services;



namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact
{
    public interface ICreateKlantContactService
    {
        Task<RelatedKlantcontactResult> CreateRelatedKlantcontactAsync(
            KlantcontactRequest klantcontactRequest,
            Guid previousKlantcontactUuid,
            Guid internetaakUuid,
            string userEmail,
            string? userName,
            Guid? partijUuid = null);
    }

    public class CreateKlantContactService : ICreateKlantContactService
    {
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly IKlantcontactService _klantcontactService;
        private readonly ILogger<CreateKlantContactService> _logger;   

        public CreateKlantContactService(
            IOpenKlantApiClient openKlantApiClient,
            IKlantcontactService klantcontactService,
            ILogger<CreateKlantContactService> logger         
            )
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));     
        }

        public async Task<RelatedKlantcontactResult> CreateRelatedKlantcontactAsync(
            KlantcontactRequest klantcontactRequest,
            Guid aanleidinggevendKlantcontactUuid,
            Guid internetaakUuid,
            string userEmail,
            string? userName,
            Guid? partijUuid = null)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ConflictException(
                    "No email found for the current user.",
                    "MISSING_EMAIL_CLAIM");
            }

            Guid? laatsteKlantcontactUuid = null;

            try
            {
                //because there is no proper way to sort klantcontacten by date (date is optional),
                //we don't add all new klantcontacten that take place during the handling of an internetaak to the original newKlantcontact
                //instead, we add each newKlantcontact to the previous newKlantcontact. this creates a chain of klantcontacten
                //here we have to find the last one in the change and we will link the new newKlantcontact that we are creating here to that one 
                laatsteKlantcontactUuid = await GetLaatsteKlantcontactUuidAsync(aanleidinggevendKlantcontactUuid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine latest contact in chain. Using original: {aanleidinggevendKlantcontactUuid}", aanleidinggevendKlantcontactUuid);
            }


            var actor = await GetOrCreateActorAsync(userEmail, userName);
            var newKlantcontact = await _openKlantApiClient.CreateKlantcontactAsync(klantcontactRequest);
            var actorKlantcontactRequest = new ActorKlantcontactRequest
            {
                Actor = new ActorReference { Uuid = actor.Uuid },
                Klantcontact = new KlantcontactReference { Uuid = newKlantcontact.Uuid }
            };
            var actorKlantcontact = await _openKlantApiClient.CreateActorKlantcontactAsync(actorKlantcontactRequest);
            var result = new RelatedKlantcontactResult
            {
                Klantcontact = newKlantcontact,
                ActorKlantcontact = actorKlantcontact
            };

            var onderwerpobject = await CreateOnderwerpobjectKlantcontactAsync(
                newKlantcontact.Uuid,
                laatsteKlantcontactUuid ?? aanleidinggevendKlantcontactUuid);

            result.Onderwerpobject = onderwerpobject;

            if (partijUuid.HasValue)
            {
                var betrokkene = await CreateBetrokkeneAsync(newKlantcontact.Uuid, partijUuid.Value);
                result.Betrokkene = betrokkene;
            }


            return result;
        }

        private async Task<Guid> GetLaatsteKlantcontactUuidAsync(Guid klantcontactUuid)
        {
            try
            {
                var ketenVolgorde = await _klantcontactService.BouwKlantcontactKetenAsync(klantcontactUuid);
                ketenVolgorde.Reverse(); // Nieuwste bovenaan

                return ketenVolgorde.Count > 0 ? ketenVolgorde[0].Uuid : klantcontactUuid;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine latest klantcontact in chain, using original {klantcontactUuid}", klantcontactUuid);

                return klantcontactUuid;
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

            Actor? actor;
            try
            {
                actor = await GetEntraActorByEmailAsync(email);
            }
            catch (Exception ex)
            {
                var safeEmailId = SecureLogging.SanitizeAndTruncate(email, 5);
                _logger.LogError(ex, "Error retrieving actor by email {SafeEmailId}", safeEmailId);

                throw new ConflictException(
                    $"Error retrieving actor by email: {ex.Message}",
                    "ACTOR_RETRIEVAL_ERROR");
            }

            if (actor == null)
            {
                var actorRequest = new ActorRequest
                {
                    Naam = naam ?? email,
                    SoortActor = SoortActor.medewerker,
                    IndicatieActief = true,
                    Actoridentificator = new Actoridentificator
                    {
                        ObjectId = email,
                        CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                        CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                        CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId
                    }
                };

                try
                {
                    actor = await _openKlantApiClient.CreateActorAsync(actorRequest);
                }
                catch (Exception ex)
                {
                    var safeActorId = SecureLogging.SanitizeAndTruncate(actorRequest.Naam, 5);
                    _logger.LogError(ex, "Error creating actor with identifier {SafeActorId}", safeActorId);
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
            Guid klantcontactUuid,
            Guid wasKlantcontactUuid)
        {
            var request = new KlantcontactOnderwerpobjectRequest
            {
                Klantcontact = new KlantcontactReference { Uuid = klantcontactUuid },
                WasKlantcontact = new KlantcontactReference { Uuid = wasKlantcontactUuid },
                Onderwerpobjectidentificator = new Onderwerpobjectidentificator
                {
                    ObjectId = wasKlantcontactUuid.ToString(),
                    CodeObjecttype = "klantcontact",
                    CodeRegister = "openklant",
                    CodeSoortObjectId = "uuid"
                }
            };

            try
            {
                return await _openKlantApiClient.CreateOnderwerpobjectAsync(request);
            }
            catch (Exception ex)
            {
                throw new ConflictException(
                    $"Error creating onderwerpobject: {ex.Message}",
                    "ONDERWERPOBJECT_CREATION_ERROR");
            }
        }

        private async Task<Betrokkene> CreateBetrokkeneAsync(Guid klantcontactUuid, Guid partijUuid)
        {
            if (klantcontactUuid == Guid.Empty || partijUuid == Guid.Empty)
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
                _logger.LogInformation("Creating betrokkene for klantcontact {klantcontactUuid} and partij {partijUuid}",
                    klantcontactUuid, partijUuid);

                var betrokkene = await _openKlantApiClient.CreateBetrokkeneAsync(betrokkeneRequest);

                _logger.LogInformation("Successfully created betrokkene {SafeBetrokkeneUuid}", betrokkene.Uuid);

                return betrokkene;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating betrokkene for klantcontact {klantcontactUuid} and partij {partijUuid}",
                    klantcontactUuid, partijUuid);

                throw new ConflictException(
                    $"Error creating betrokkene: {ex.Message}",
                    "BETROKKENE_CREATION_ERROR");
            }
        }
        private async Task<Actor?> GetEntraActorByEmailAsync(string userEmail)
        {

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var fromEntra = await _openKlantApiClient.QueryActorAsync(new ActorQuery
                {
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                    IndicatieActief = true,
                    SoortActor = SoortActor.medewerker,
                    ActoridentificatorObjectId = userEmail
                });

                return fromEntra;

            }

            return null;
        }
    }

    public class RelatedKlantcontactResult
    {
        public required Klantcontact Klantcontact { get; set; }
        public required ActorKlantcontact ActorKlantcontact { get; set; }
        public Onderwerpobject? Onderwerpobject { get; set; }
        public Betrokkene? Betrokkene { get; set; }
    }

}
