using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.ReopenContactRequest;

public interface IReopenContactRequestService
{
    Task<ReopenResult> ReopenAsync(Guid internetaakId);
}

public class ReopenResult
{
    public required Internetaak Internetaak { get; set; }
    public string? Waarschuwing { get; set; }
}
