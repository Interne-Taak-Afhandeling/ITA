using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public interface IContactverzoekAutorisatieGuardService
{
    /// <summary>
    /// Throws <see cref="InterneTaakAfhandeling.Common.Exceptions.GeenToegangException"/> if the user is not
    /// a functioneel beheerder and does not belong to the afdeling/groep the interne taak is assigned to.
    /// </summary>
    Task GuardAgainstGeenToegangAsync(Internetaak internetaak, ITAUser user);
}
