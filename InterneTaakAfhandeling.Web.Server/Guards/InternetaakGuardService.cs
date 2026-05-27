using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public class InternetaakGuardService(
    IOpenKlantApiClient openKlantApiClient,
    ILogger<InternetaakGuardService> logger) : IInternetaakGuardService
{
    public async Task<ObjectResult?> EnsureNotVerwerktAsync(Guid internetaakId, string actionType)
    {
        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId);

        if (internetaak.Status != KnownInternetaakStatussen.Verwerkt)
        {
            return null;
        }

        logger.LogWarning(
            "Mutation blocked: action {ActionType} on interne taak {InternetaakId} has status 'verwerkt'",
            actionType,
            internetaakId);

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            Title = "Contactverzoek is afgehandeld",
            Detail = "Dit contactverzoek heeft status 'verwerkt' en kan niet meer worden gewijzigd.",
            Status = StatusCodes.Status409Conflict
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status409Conflict
        };
    }
}
