using InterneTaakAfhandeling.Common.ApiClients;
using InterneTaakAfhandeling.Common.ApiClients.ObjectApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.Actors;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.AllInternetakenOverview;

public interface IMedewerkerActorenService
{
    /// <summary>
    /// Finds every OpenKlant actor that represents the medewerker with the given objectregister identificatie.
    /// A medewerker can be known under several identificaties: the objectregister id (doorsturen),
    /// the e-mail address from Entra ID (login) and a manually entered e-mail address.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetActorUuidsAsync(string medewerkerIdentificatie);
}

public class MedewerkerActorenService(
    IActorService actorService,
    IObjectApiClient objectApiClient,
    ILogger<MedewerkerActorenService> logger) : IMedewerkerActorenService
{
    public async Task<IReadOnlyList<Guid>> GetActorUuidsAsync(string medewerkerIdentificatie)
    {
        var queries = new List<ActorQuery>
        {
            CreateQuery(KnownMedewerkerIdentificators.ObjectRegisterId, medewerkerIdentificatie)
        };

        var emails = await GetEmailAddressesAsync(medewerkerIdentificatie);
        foreach (var email in emails)
        {
            queries.Add(CreateQuery(KnownMedewerkerIdentificators.EmailFromEntraId, email));
            queries.Add(CreateQuery(KnownMedewerkerIdentificators.EmailHandmatig, email));
        }

        var actors = await Task.WhenAll(queries.Select(actorService.QueryActorAsync));

        return actors
            .Select(actor => Guid.TryParse(actor?.Uuid, out var uuid) ? uuid : (Guid?)null)
            .OfType<Guid>()
            .Distinct()
            .ToList();
    }

    private async Task<IEnumerable<string>> GetEmailAddressesAsync(string medewerkerIdentificatie)
    {
        try
        {
            var medewerkers = await objectApiClient.GetMedewerkersByIdentificatie(medewerkerIdentificatie);
            return medewerkers
                .SelectMany(m => m.EmailAddresses)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex)
        {
            // Without e-mail addresses we can still filter on the objectregister actor.
            logger.LogWarning(ex, "Kon e-mailadressen van medewerker niet ophalen uit het objectenregister");
            return [];
        }
    }

    // IndicatieActief is deliberately not set: afgehandelde contactverzoeken can be assigned to actors that are no longer active.
    private static ActorQuery CreateQuery(KnownMedewerkerIdentificators identificator, string objectId) => new()
    {
        ActoridentificatorCodeObjecttype = identificator.CodeObjecttype,
        ActoridentificatorCodeRegister = identificator.CodeRegister,
        ActoridentificatorCodeSoortObjectId = identificator.CodeSoortObjectId,
        ActoridentificatorObjectId = objectId,
        SoortActor = SoortActor.medewerker
    };
}
