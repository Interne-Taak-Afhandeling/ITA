using System.ComponentModel.DataAnnotations;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

public class ForwardContactRequestModel : IValidatableObject
{
    public string? Medewerker { get; set; }
    public string? Afdeling { get; set; }
    public string? Groep { get; set; }

    public string? InterneNotitie { get; set; } // Optional note to add to the contact request

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var afdelingSpecified = !string.IsNullOrWhiteSpace(Afdeling);
        var groepSpecified = !string.IsNullOrWhiteSpace(Groep);
        if ((!afdelingSpecified && !groepSpecified) || (afdelingSpecified && groepSpecified))
        {
            yield return new ValidationResult(
                "Specificeer een afdeling of groep.",
                [nameof(Afdeling), nameof(Groep)]);
            yield break;
        }
    }
}

public class ForwardContactRequestResponse
{
    public Internetaak? Internetaak { get; init; }
    public required string NotificationResult { get; init; }
}

