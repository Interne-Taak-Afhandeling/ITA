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

            var actor = await _klantcontactService.GetOrCreateActorAsync(userEmail, userName);
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
                var onderwerpobject = await _klantcontactService.CreateOnderwerpobjectAsync(
                    klantcontact.Uuid,
                    laatsteKlantcontactUuid,
                    laatsteKlantcontactUuid,
                    "klantcontact",
                    "openklant",
                    "uuid");

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

                    var betrokkene = await _klantcontactService.CreateBetrokkeneAsync(klantcontactUuid, providedPartijUuid);
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