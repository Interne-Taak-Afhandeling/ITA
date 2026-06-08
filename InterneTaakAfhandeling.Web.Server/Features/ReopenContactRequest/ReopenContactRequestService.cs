using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services.LogboekService;

namespace InterneTaakAfhandeling.Web.Server.Features.ReopenContactRequest;

public class ReopenContactRequestService(
    IOpenKlantApiClient openKlantApiClient,
    ILogboekService logboekService,
    ILogger<ReopenContactRequestService> logger) : IReopenContactRequestService
{
    public async Task<ReopenResult> ReopenAsync(Guid internetaakId, string reden, ITAUser user)
    {
        var internetaak = await openKlantApiClient.GetInternetaakByIdAsync(internetaakId);

        var actors = await ResolveActorsAsync(internetaak);
        var medewerkerActors = actors.Where(a => a.SoortActor == SoortActor.medewerker).ToList();
        var orgEenheidActors = actors.Where(a => a.SoortActor == SoortActor.organisatorische_eenheid).ToList();

        string? waarschuwing = null;
        var hasInactiveMedewerker = medewerkerActors.Any(a => a.IndicatieActief == false);

        if (hasInactiveMedewerker)
        {
            if (orgEenheidActors.Count == 0)
            {
                logger.LogWarning(
                    "Reopen blocked: interne taak {InternetaakId} has inactive medewerker and no organisatorische eenheid",
                    internetaakId);

                throw new UnprocessableEntityException(
                    "De oorspronkelijke behandelaar is niet meer actief en er is geen organisatorische eenheid gekoppeld. Heropening is niet mogelijk.",
                    "GEEN_ORGANISATORISCHE_EENHEID");
            }

            // Reassign to the organisatorische eenheid actors only
            var reassignRequest = new InternetakenPatchActorsRequest
            {
                ToegewezenAanActoren = orgEenheidActors
                    .Select(a => new UuidObject { Uuid = Guid.Parse(a.Uuid) })
                    .ToList()
            };

            await openKlantApiClient.PatchInternetaakActorAsync(reassignRequest, internetaak.Uuid);

            waarschuwing = "De oorspronkelijke behandelaar is niet meer actief. Het contactverzoek is toegewezen aan de organisatorische eenheid.";

            logger.LogInformation(
                "Interne taak {InternetaakId} reassigned to organisatorische eenheid due to inactive medewerker",
                internetaakId);
        }

        // Patch status to te_verwerken
        var statusRequest = new InternetakenPatchStatusRequest
        {
            Status = KnownInternetaakStatussen.TeVerwerken
        };

        var updatedInternetaak = await openKlantApiClient.PatchInternetaakStatusAsync(statusRequest, internetaak.Uuid);

        await logboekService.LogContactRequestAction(
            KnownContactAction.Reopened(reden, user), internetaakId);

        return new ReopenResult
        {
            Internetaak = updatedInternetaak,
            Waarschuwing = waarschuwing
        };
    }

    private async Task<List<Actor>> ResolveActorsAsync(Internetaak internetaak)
    {
        if (internetaak.ToegewezenAanActoren == null || internetaak.ToegewezenAanActoren.Count == 0)
        {
            return [];
        }

        var resolvedActors = await Task.WhenAll(
            internetaak.ToegewezenAanActoren
                .Where(a => !string.IsNullOrEmpty(a.Uuid))
                .Select(a => openKlantApiClient.GetActorAsync(a.Uuid))
        );

        return resolvedActors.ToList();
    }
}
