namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest;

public class ForwardContactRequestModel
{
    public required string ActorType { get; set; } // possible values medewerker, afdeling and groep

    public required string ActorIdentifier { get; set; }   // Email for medewerker, or identifier for afdeling/groep

    public string? InterneNotitie { get; set; } // Optional note to add to the contact request
}