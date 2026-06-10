using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Features.ReopenContactRequest;

public interface IReopenContactRequestService
{
    Task<ReopenResult> ReopenAsync(Guid internetaakId, string reden, ITAUser user);
}

public class ReopenResult
{
    public required Internetaak Internetaak { get; set; }
    public string? Waarschuwing { get; set; }
}
