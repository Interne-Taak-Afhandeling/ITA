// In CreateKlantContactService.cs

using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Features.CreateKlantContact;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models;
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

        // Get or create actor for the current user (nu roepen we onze eigen methode aan)
        var actor = await GetOrCreateActorAsync(userEmail, userName);

        // Create the new klantcontact
        var klantcontact = await _openKlantApiClient.CreateKlantcontactAsync(klantcontactRequest);

        // Link the klantcontact to the current actor
        var actorKlantcontactRequest = new ActorKlantcontactRequest
        {
            Actor = new ActorReference { Uuid = actor.Uuid },
            Klantcontact = new KlantcontactReference { Uuid = klantcontact.Uuid }
        };

        var actorKlantcontact = await _openKlantApiClient.CreateActorKlantcontactAsync(actorKlantcontactRequest);

        // Initialize the result
        var result = new RelatedKlantcontactResult
        {
            Klantcontact = klantcontact,
            ActorKlantcontact = actorKlantcontact,
            Onderwerpobject = null
        };

        // If there's a previous klantcontact, create the onderwerpobject link
        if (!string.IsNullOrEmpty(previousKlantcontactUuid))
        {
            var onderwerpobject = new Onderwerpobject
            {
                Uuid = string.Empty, // This will be filled by the API
                Url = string.Empty, // This will be filled by the API
                Klantcontact = new Klantcontact
                {
                    Uuid = klantcontact.Uuid,
                    Url = klantcontact.Url,
                    HadBetrokkenActoren = new List<Actor>() // Minimale instantie
                },
                WasKlantcontact = new Klantcontact
                {
                    Uuid = previousKlantcontactUuid,
                    Url = string.Empty, // The URL is not needed for the request
                    HadBetrokkenActoren = new List<Actor>() // Minimale instantie
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

        // Probeer eerst de actor op te halen
        var actor = await GetActorByEmailAsync(email);

        // Als actor niet bestaat, maak een nieuwe aan
        if (actor == null)
        {
            // Maak een nieuwe actor request
            var actorRequest = new ActorRequest
            {
                Naam = naam ?? email, // Als naam niet opgegeven is, gebruik email als naam
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

            // Maak de actor aan
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