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

    public required string ActorIdentifier { get; set; } // Identifier for medewerker, afdeling, or groep

    public string? MedewerkerEmail { get; set; } // Legacy: optional medewerker email when ActorType is afdeling/groep. Ignored when ActorType is Medewerker.

    public AfdelingOfGroepSelectie? AfdelingOfGroep { get; set; } // Required when ActorType is Medewerker

    public string? InterneNotitie { get; set; } // Optional note to add to the contact request

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ActorType is not (KnownActorType.Medewerker or KnownActorType.Afdeling or KnownActorType.Groep))
        {
            yield return new ValidationResult(
                $"ActorType must be '{KnownActorType.Medewerker}', '{KnownActorType.Afdeling}', or '{KnownActorType.Groep}'.",
                [nameof(ActorType)]);
            yield break;
        }

        if (string.IsNullOrWhiteSpace(ActorIdentifier))
            yield return new ValidationResult(
                "ActorIdentifier is required.",
                [nameof(ActorIdentifier)]);

        switch (ActorType)
        {
            case KnownActorType.Medewerker:
                if (AfdelingOfGroep == null)
                {
                    yield return new ValidationResult(
                        $"AfdelingOfGroep is required when ActorType is '{KnownActorType.Medewerker}'.",
                        [nameof(AfdelingOfGroep)]);
                }
                else
                {
                    if (AfdelingOfGroep.Type is not (KnownActorType.Afdeling or KnownActorType.Groep))
                        yield return new ValidationResult(
                            $"AfdelingOfGroep.Type must be '{KnownActorType.Afdeling}' or '{KnownActorType.Groep}'.",
                            [nameof(AfdelingOfGroep)]);

                    if (string.IsNullOrWhiteSpace(AfdelingOfGroep.Identifier))
                        yield return new ValidationResult(
                            "AfdelingOfGroep.Identifier is required.",
                            [nameof(AfdelingOfGroep)]);
                }
                break;

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

public class AfdelingOfGroepSelectie
{
    public required string Type { get; set; } // KnownActorType.Afdeling or KnownActorType.Groep
    public required string Identifier { get; set; }
}

public class ForwardContactRequestResponse
{
    public Internetaak? Internetaak { get; init; }
    public required string NotificationResult { get; init; }
}

