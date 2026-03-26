using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Config;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Web.Server.Features.Werklijst;

public class WerklijstService(
    IOpenKlantApiClient openKlantApiClient,
    IObjectApiClient objectApiClient,
    IOptions<AfhandeltermijnOptions> afhandeltermijnOptions,
    ILogger<WerklijstService> logger) : IWerklijstService
{
    private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
    private readonly IObjectApiClient _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));
    private readonly AfhandeltermijnOptions _afhandeltermijnOptions = afhandeltermijnOptions.Value;
    private readonly ILogger<WerklijstService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<WerklijstResponse> GetWerklijstAsync(WerklijstQuery query, ITAUser user)
    {
        _logger.LogInformation(
            "Fetching werklijst for user {UserId}, role: {Role}, page: {Page}",
            SecureLogging.SanitizeAndTruncate(user.ObjectregisterMedewerkerId),
            user.HasOrganisatieCoordinatorAccess ? "organisatie-coordinator" : "team-coordinator",
            query.Page);

        // Dual-role: broadest scope (organisatie) takes precedence per spec edge case
        if (user.HasOrganisatieCoordinatorAccess)
        {
            return await GetWerklijstForOrganisatieCoordinatorAsync(query);
        }

        return await GetWerklijstForTeamCoordinatorAsync(query, user);
    }

    private async Task<WerklijstResponse> GetWerklijstForOrganisatieCoordinatorAsync(WerklijstQuery query)
    {
        var interneTaakQuery = BuildBaseQuery(query);
        var response = await _openKlantApiClient.GetAllInternetakenAsync(interneTaakQuery);

        var items = (await Task.WhenAll(
            response.Results.Select(MapToWerklijstItemAsync))).ToList();

        return new WerklijstResponse
        {
            Count = response.Count,
            Next = response.Next,
            Previous = response.Previous,
            Results = items
        };
    }

    private async Task<WerklijstResponse> GetWerklijstForTeamCoordinatorAsync(WerklijstQuery query, ITAUser user)
    {
        var scopeNames = await ResolveUserScopeNamesAsync(user.ObjectregisterMedewerkerId);

        if (scopeNames.Count == 0)
        {
            _logger.LogWarning(
                "Team-coordinator {UserId} has no afdelingen/groepen membership",
                SecureLogging.SanitizeAndTruncate(user.ObjectregisterMedewerkerId));
            return new WerklijstResponse();
        }

        // Fetch items per scope name, then merge and apply in-memory pagination.
        // Municipal scale (~500 items) makes this acceptable for MVP.
        var allItems = new List<WerklijstOverzichtItem>();
        foreach (var scopeName in scopeNames)
        {
            var interneTaakQuery = BuildBaseQuery(query);
            interneTaakQuery.Actoren__Naam = scopeName;
            interneTaakQuery.Page = null;
            interneTaakQuery.PageSize = null;

            try
            {
                var response = await _openKlantApiClient.GetAllInternetakenAsync(interneTaakQuery);
                var items = await Task.WhenAll(
                    response.Results.Select(MapToWerklijstItemAsync));
                allItems.AddRange(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching werklijst for scope {ScopeName}",
                    SecureLogging.SanitizeAndTruncate(scopeName));
            }
        }

        // Deduplicate by UUID (an item could appear under multiple scopes)
        var deduplicated = allItems
            .DistinctBy(i => i.Uuid)
            .ToList();

        var page = query.Page;
        var pageSize = query.PageSize;
        var total = deduplicated.Count;
        var paged = deduplicated
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new WerklijstResponse
        {
            Count = total,
            Next = page * pageSize < total ? $"/api/werklijst?page={page + 1}&pageSize={pageSize}" : null,
            Previous = page > 1 ? $"/api/werklijst?page={page - 1}&pageSize={pageSize}" : null,
            Results = paged
        };
    }

    private async Task<List<string>> ResolveUserScopeNamesAsync(string objectregisterMedewerkerId)
    {
        try
        {
            var medewerkers = await _objectApiClient.GetMedewerkersByIdentificatie(objectregisterMedewerkerId);
            if (medewerkers.Count == 0) return [];

            var medewerker = medewerkers.First();
            return (medewerker.Afdelingen ?? [])
                .Select(a => a.Afdelingnaam)
                .Concat((medewerker.Groepen ?? []).Select(g => g.Groepsnaam))
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving scope for medewerker {MedewerkerId}",
                SecureLogging.SanitizeAndTruncate(objectregisterMedewerkerId));
            return [];
        }
    }

    private static InterneTaakQuery BuildBaseQuery(WerklijstQuery query) => new()
    {
        Page = query.Page,
        PageSize = query.PageSize,
        Status = KnownInternetaakStatussen.TeVerwerken
    };

    private async Task<WerklijstOverzichtItem> MapToWerklijstItemAsync(Internetaak internetaak)
    {
        var item = new WerklijstOverzichtItem
        {
            Url = internetaak.Url ?? string.Empty,
            Uuid = internetaak.Uuid,
            Nummer = internetaak.Nummer,
            Status = internetaak.Status
        };

        await LoadKlantcontactInfoAsync(internetaak, item);
        await LoadActorInfoAsync(internetaak, item);

        return item;
    }

    private async Task LoadKlantcontactInfoAsync(Internetaak internetaak, WerklijstOverzichtItem item)
    {
        if (internetaak.AanleidinggevendKlantcontact?.Uuid is not { } klantcontactUuid || klantcontactUuid == default)
            return;

        try
        {
            var klantcontact = await _openKlantApiClient.GetKlantcontactAsync(klantcontactUuid);

            item.Onderwerp = klantcontact.Onderwerp;
            item.Kanaal = klantcontact.Kanaal;
            item.PlaatsgevondenOp = klantcontact.PlaatsgevondenOp;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch klantcontact for internetaak {Uuid}",
                SecureLogging.SanitizeUuid(internetaak.Uuid));
        }
    }

    private async Task LoadActorInfoAsync(Internetaak internetaak, WerklijstOverzichtItem item)
    {
        if (internetaak.ToegewezenAanActoren?.Any() != true)
            return;

        var actorTasks = internetaak.ToegewezenAanActoren
            .Where(actorRef => !string.IsNullOrEmpty(actorRef.Uuid))
            .Select(actorRef => GetActorSafeAsync(actorRef.Uuid));

        var actors = await Task.WhenAll(actorTasks);

        var medewerkerActor = actors.FirstOrDefault(a => a?.SoortActor == SoortActor.medewerker);
        if (medewerkerActor != null)
        {
            item.Medewerker = medewerkerActor.Naam;
        }

        // Distinguish afdeling vs groep by CodeObjecttype
        foreach (var actor in actors.Where(a => a?.SoortActor == SoortActor.organisatorische_eenheid))
        {
            if (actor?.Actoridentificator?.CodeObjecttype == KnownAfdelingIdentificators.CodeObjecttypeAfdeling)
            {
                item.Afdeling = actor.Naam;
            }
            else if (actor?.Actoridentificator?.CodeObjecttype == KnownGroepIdentificators.CodeObjecttypeGroep)
            {
                item.Groep = actor.Naam;
            }
            else if (!string.IsNullOrEmpty(actor?.Naam))
            {
                // Fallback: assign to afdeling if CodeObjecttype is unknown
                item.Afdeling ??= actor!.Naam;
            }
        }
    }

    private async Task<Actor?> GetActorSafeAsync(string uuid)
    {
        try
        {
            return await _openKlantApiClient.GetActorAsync(uuid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch actor {Uuid}", SecureLogging.SanitizeUuid(uuid));
            return null;
        }
    }
}
