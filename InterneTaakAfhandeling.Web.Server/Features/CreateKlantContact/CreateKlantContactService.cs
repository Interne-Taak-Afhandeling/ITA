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
            string? previousKlantcontactUuid,
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

            // Get or create actor based on value
            var actor = await GetOrCreateActorAsync(userEmail, userName);

            // Create the klantcontact
            var klantcontact = await _openKlantApiClient.CreateKlantcontactAsync(klantcontactRequest);

            // Associate actor with klantcontact
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

            // If we have a previous klantcontact, establish relationships
            if (!string.IsNullOrEmpty(previousKlantcontactUuid))
            {
                // Create relationship with previous klantcontact
                var onderwerpobject = await CreateOnderwerpobjectAsync(klantcontact.Uuid, previousKlantcontactUuid);
                result.Onderwerpobject = onderwerpobject;

                // Create betrokkene relationship
                await CreateBetrokkeneRelationshipAsync(result, klantcontact.Uuid, previousKlantcontactUuid, partijUuid);
            }

            return result;
        }

        private async Task CreateBetrokkeneRelationshipAsync(
            RelatedKlantcontactResult result,
            string klantcontactUuid,
            string previousKlantcontactUuid,
            string? providedPartijUuid)
        {
            try
            {
                // If partijUuid is directly provided, use it
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

                // Try to get partijUuid from previous klantcontact

                if (Guid.TryParse(previousKlantcontactUuid, out Guid parsedpreviousKlantcontactUuid))
                {
                    _logger.LogInformation($"Fetching previous klantcontact {parsedpreviousKlantcontactUuid} to extract partijUuid");
                }

                
                var previousKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(previousKlantcontactUuid);

                // Check if we have betrokkenen in the expand data
                if (previousKlantcontact?.Expand?.HadBetrokkenen != null &&
                    previousKlantcontact.Expand.HadBetrokkenen.Count > 0)
                {
                    var firstBetrokkene = previousKlantcontact.Expand.HadBetrokkenen[0];

                    // Try direct property access first (most common case)
                    if (firstBetrokkene.WasPartij != null)
                    {
                        string? extractedPartijUuid = null;

                        // Try to get uuid property through reflection
                        try
                        {
                            var partijType = firstBetrokkene.WasPartij.GetType();
                            var uuidProperty = partijType.GetProperty("Uuid");
                            if (uuidProperty != null)
                            {
                                extractedPartijUuid = uuidProperty.GetValue(firstBetrokkene.WasPartij)?.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error accessing Uuid property via reflection");
                        }

                        if (!string.IsNullOrEmpty(extractedPartijUuid))
                        {
                            _logger.LogInformation($"Found partijUuid {extractedPartijUuid} from betrokkene {firstBetrokkene.Uuid}");
                            var betrokkene = await CreateBetrokkeneAsync(klantcontactUuid, extractedPartijUuid);
                            result.Betrokkene = betrokkene;
                        }
                        else
                        {
                            _logger.LogWarning($"Could not extract partijUuid from betrokkene {firstBetrokkene.Uuid}");
                        }
                    }
                }
                else
                {
                    if (Guid.TryParse(previousKlantcontactUuid, out Guid parsedPreviousKlantcontactUuid))
                    {
                        _logger.LogWarning($"No hadBetrokkenen found in previous klantcontact {parsedPreviousKlantcontactUuid}");
                    }

                    
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail if we can't create the betrokkene
                _logger.LogError(ex, $"Could not create betrokkene for klantcontact {klantcontactUuid}");
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
                    HadBetrokkenActoren = new List<Actor>() // Initialize to empty list to prevent null reference
                },
                WasKlantcontact = new Klantcontact
                {
                    Uuid = wasKlantcontactUuid,
                    Url = $"/klantinteracties/api/v1/klantcontacten/{wasKlantcontactUuid}",
                    HadBetrokkenActoren = new List<Actor>() // Initialize to empty list to prevent null reference
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
                return await _openKlantApiClient.GetActorByEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving actor by email {Mask(email)}");
                throw new ConflictException(
                    $"Error retrieving actor by email: {ex.Message}",
                    "ACTOR_RETRIEVAL_ERROR");
            }
        }

        //show only the first part of a string. up to 6 characters 
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
    }

    public class RelatedKlantcontactResult
    {
        public Klantcontact Klantcontact { get; set; }
        public ActorKlantcontact ActorKlantcontact { get; set; }
        public Onderwerpobject? Onderwerpobject { get; set; }
        public Betrokkene? Betrokkene { get; set; }
    }
}