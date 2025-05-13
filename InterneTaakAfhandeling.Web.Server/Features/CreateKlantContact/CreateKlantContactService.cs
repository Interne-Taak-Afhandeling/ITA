// In CreateKlantContactService.cs

using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Features.CreateKlantContact;
using static InterneTaakAfhandeling.Common.Services.OpenKlantApi.OpenKlantApiClient;

public interface ICreateKlantContactService
{
    Task<RelatedKlantcontactResult> CreateRelatedKlantcontactAsync(
        KlantcontactRequest klantcontactRequest,
        string? previousKlantcontactUuid,
        string userEmail,
        string? userName);
}

public class CreateKlantContactService : ICreateKlantContactService
{
    private readonly IOpenKlantApiClient _openKlantApiClient;

    public CreateKlantContactService(IOpenKlantApiClient openKlantApiClient)
    {
        _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
    }

    public async Task<RelatedKlantcontactResult> CreateRelatedKlantcontactAsync(
        KlantcontactRequest klantcontactRequest,
        string? previousKlantcontactUuid,
        string userEmail,
        string? userName)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            throw new ConflictException(
                "No email found for the current user.",
                "MISSING_EMAIL_CLAIM");
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
            ActorKlantcontact = actorKlantcontact,
            Onderwerpobject = null
        };

        if (!string.IsNullOrEmpty(previousKlantcontactUuid))
        {
            var onderwerpobject = new Onderwerpobject
            {
                Uuid = string.Empty, 
                Url = string.Empty, 
                Klantcontact = new Klantcontact
                {
                    Uuid = klantcontact.Uuid,
                    Url = klantcontact.Url
                },
                WasKlantcontact = new Klantcontact
                {
                    Uuid = previousKlantcontactUuid,
                    Url = string.Empty
                }
            };

            var createdOnderwerpobject = await _openKlantApiClient.CreateOnderwerpobjectAsync(onderwerpobject);
            result.Onderwerpobject = createdOnderwerpobject;
        }

        return result;
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
                Actoridentificator = new ActorIdentificator
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

    private async Task<Actor?> GetActorByEmailAsync(string email)
    {
        try
        {
            var response = await _openKlantApiClient.GetActorByEmail(email);
            return response;
        }
        catch (Exception ex)
        {
            throw new ConflictException(
                $"Error retrieving actor by email: {ex.Message}",
                "ACTOR_RETRIEVAL_ERROR");
        }
    }

    private async Task<Actor?> CreateActorAsync(ActorRequest request)
    {
        try
        {
            var actor = await _openKlantApiClient.CreateActorAsync(request);
            return actor;
        }
        catch (Exception ex)
        {
            throw new ConflictException(
                $"Error creating actor: {ex.Message}",
                "ACTOR_CREATION_ERROR");
        }
    }
}