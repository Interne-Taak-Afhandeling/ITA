using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Features.Werklijst;

public interface IWerklijstService
{
    Task<WerklijstResponse> GetWerklijstAsync(WerklijstQuery query, ITAUser user);
}
