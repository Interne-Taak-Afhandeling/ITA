namespace InterneTaakAfhandeling.Web.Server.Features.ReopenContactRequest;

public class ReopenContactRequestModel
{
    public required string Reden { get; set; }
}

public class ReopenContactRequestResponse
{
    public required object Internetaak { get; set; }
    public string? Waarschuwing { get; set; }
}
