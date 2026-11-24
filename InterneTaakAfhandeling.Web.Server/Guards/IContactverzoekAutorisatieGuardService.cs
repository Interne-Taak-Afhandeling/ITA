using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public interface IContactverzoekAutorisatieGuardService
{
    /// <summary>
    /// Throws <see cref="InterneTaakAfhandeling.Common.Exceptions.GeenToegangException"/> if the user is not
    /// a functioneel beheerder and does not belong to the afdeling/groep the interne taak is assigned to.
    /// </summary>
    Task GuardAgainstGeenLidVanAfdelingOfGroepAsync(Internetaak internetaak, ITAUser user);

    /// <summary>
    /// Same rule as the overload above, for callers that only have the id of the interne taak.
    /// Loads the interne taak and its assigned actors (skipped for a functioneel beheerder).
    /// </summary>
    Task GuardAgainstGeenLidVanAfdelingOfGroepAsync(Guid internetaakId, ITAUser user);

    /// <summary>
    /// Throws <see cref="InterneTaakAfhandeling.Common.Exceptions.GeenToegangException"/> if the user is not
    /// a functioneel beheerder and does not belong to the requested afdeling or groep (used for the worklist per afdeling/groep).
    /// </summary>
    Task GuardAgainstGeenToegangTotAfdelingOfGroepAsync(string afdelingOfGroepNaam, ITAUser user);
}
