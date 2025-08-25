using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

public class ForwardContactRequestModel : IValidatableObject
{
    private static readonly Regex EmailRegex = ValidationRegexHelper.EmailValidator();

    public required string ActorType { get; set; } // Use KnownActorType enum values: Medewerker, Afdeling, Groep

    public required string ActorIdentifier { get; set; } // Email for medewerker, or identifier for afdeling/groep

    public string? MedewerkerEmail { get; set; } // Optional medewerker email when ActorType is afdeling/groep

    public string? InterneNotitie { get; set; } // Optional note to add to the contact request

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ActorType is not (KnownActorType.Afdeling or KnownActorType.Groep))
        {
            yield return new ValidationResult(
                "ActorType must be 'afdeling', or 'groep'.",
                [nameof(ActorType)]);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(ActorIdentifier))
            yield return new ValidationResult(
                "ActorIdentifier is required.",
                [nameof(ActorIdentifier)]);

        switch (ActorType)
        {
            case KnownActorType.Afdeling or KnownActorType.Groep when
                !string.IsNullOrWhiteSpace(MedewerkerEmail) &&
                !EmailRegex.IsMatch(MedewerkerEmail):
                yield return new ValidationResult(
                    "MedewerkerEmail must be a valid email address when provided.",
                    [nameof(MedewerkerEmail)]);
                break;
        }
    }
}

public class ForwardContactRequestResponse
{
    public Internetaak? Internetaak { get; init; }
    public required string NotificationResult { get; init; }
}

public class ActorEmailResolutionResult
{
    public List<string> FoundEmails { get; } = [];
    public List<Actor> NotFoundActors { get; } = [];
}