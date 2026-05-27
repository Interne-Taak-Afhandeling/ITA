using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public interface IInternetaakGuardService
{
    /// <summary>
    /// Checks whether the interne taak has status 'verwerkt'.
    /// Returns a 409 Conflict ObjectResult if blocked, or null if the mutation is allowed.
    /// </summary>
    Task<ObjectResult?> EnsureNotVerwerktAsync(Guid internetaakId, string actionType);
}
