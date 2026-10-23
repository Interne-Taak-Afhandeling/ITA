using InterneTaakAfhandeling.Common.Services.Internetaken;
using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;

namespace InterneTaakAfhandeling.Web.Server.Guards;

public class InternetaakGuardService(
    IInternetaakService internetaakService,
    ILogger<InternetaakGuardService> logger) : IInternetaakGuardService
{
    public async Task GuardAgainstVerwerktAsync(Guid internetaakId)
    {
        var internetaak = await internetaakService.GetInternetaakByIdAsync(internetaakId);

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
        var internetaak = await internetaakService.GetInternetaakByIdAsync(internetaakId);

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
