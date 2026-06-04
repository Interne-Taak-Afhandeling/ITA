namespace InterneTaakAfhandeling.Web.Server.Guards;

public interface IInternetaakGuardService
{
    /// <summary>
    /// Throws <see cref="InterneTaakAfhandeling.Common.Exceptions.ConflictException"/> if the interne taak has status 'verwerkt'.
    /// </summary>
    Task GuardAgainstVerwerktAsync(Guid internetaakId);
}
