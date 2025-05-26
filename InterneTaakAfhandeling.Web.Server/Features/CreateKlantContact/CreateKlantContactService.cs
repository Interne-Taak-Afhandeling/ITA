using InterneTaakAfhandeling.Common.Services.OpenklantApi;
using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Middleware;

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
        private readonly ILogger<CreateKlantContactService> _logger;

        public CreateKlantContactService(IOpenKlantApiClient openKlantApiClient, ILogger<CreateKlantContactService> logger)
        {
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
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
                    laatsteKlantcontactUuid = await GetLaatsteKlantcontactUuid(aanleidinggevendKlantcontactUuid);

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
                var onderwerpobject = await CreateOnderwerpobjectAsync(klantcontact.Uuid, laatsteKlantcontactUuid);
                result.Onderwerpobject = onderwerpobject;
            }

            await CreateBetrokkeneRelationshipAsync(result, klantcontact.Uuid, partijUuid);

            return result;
        }

        private async Task<string> GetLaatsteKlantcontactUuid(string klantcontactUuid)
        {
            try
            {
                var ketenVolgorde = await BouwKlantcontactKeten(klantcontactUuid);
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

        private async Task<List<Klantcontact>> BouwKlantcontactKeten(string startKlantcontactUuid)
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

        private async Task<Actor> GetOrCreateActorAsync(string email, string? naam = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ConflictException(
                    "Email must not be empty",
                    "EMPTY_EMAIL_ADDRESS");
            }

            var actor = await GetActorByEmailAsync(email);
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

                actor = await CreateActorAsync(actorRequest);
            }

            if (actor == null)
            {
                throw new ConflictException(
                    "Failed to get or create actor",
                    "ACTOR_CREATION_FAILED");
            }

            return actor;
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

        private async Task<Actor?> GetActorByEmailAsync(string email)
        {
            try
            {
                return await _openKlantApiClient.QueryActorAsync(new()
                {
                    ActoridentificatorObjectId = email,
                    ActoridentificatorCodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                    ActoridentificatorCodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                    ActoridentificatorCodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                    SoortActor = SoortActor.medewerker
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving actor by email {Mask(email)}");
                throw new ConflictException(
                    $"Error retrieving actor by email: {ex.Message}",
                    "ACTOR_RETRIEVAL_ERROR");
            }
        }

        private static string Mask(string value)
        {
            return value[..(value.Length < 6 ? value.Length : 5)];
        }

        private async Task<Actor?> CreateActorAsync(ActorRequest request)
        {
            try
            {
                return await _openKlantApiClient.CreateActorAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating actor for {Mask(request.Naam)}");
                throw new ConflictException(
                    $"Error creating actor: {ex.Message}",
                    "ACTOR_CREATION_ERROR");
            }
        }

        private async Task<Onderwerpobject> CreateOnderwerpobjectAsync(string klantcontactUuid, string wasKlantcontactUuid)
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
    }

    public class RelatedKlantcontactResult
    {
        public Klantcontact Klantcontact { get; set; }
        public ActorKlantcontact ActorKlantcontact { get; set; }
        public Onderwerpobject? Onderwerpobject { get; set; }
        public Betrokkene? Betrokkene { get; set; }
    }
}