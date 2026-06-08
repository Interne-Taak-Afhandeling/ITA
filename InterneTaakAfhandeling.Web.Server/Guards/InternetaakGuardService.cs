using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public class InternetaakGuardService(
    IOpenKlantApiClient openKlantApiClient,
    ILogger<InternetaakGuardService> logger) : IInternetaakGuardService
{
    public async Task GuardAgainstVerwerktAsync(Guid internetaakId)
    {
        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId);

        if (internetaak.Status == KnownInternetaakStatussen.Verwerkt)
        {
            logger.LogWarning(
                "Mutation blocked: interne taak {InternetaakId} has status 'verwerkt'",
                internetaakId);

            throw new ConflictException(
                "Dit contactverzoek heeft status 'verwerkt' en kan niet meer worden gewijzigd.",
                "INTERNETAAK_VERWERKT");
        }
    }

    public async Task GuardAgainstNietVerwerktAsync(Guid internetaakId)
    {
        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId);

        if (internetaak.Status != KnownInternetaakStatussen.Verwerkt)
        {
            logger.LogWarning(
                "Reopen blocked: interne taak {InternetaakId} has status '{Status}', expected 'verwerkt'",
                internetaakId,
                internetaak.Status);

            throw new ConflictException(
                "Dit contactverzoek is niet gesloten en kan niet worden heropend.",
                "INTERNETAAK_NIET_VERWERKT");
        }
    }
}
