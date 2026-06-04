using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest.MedewerkersOverzicht;

public interface IMedewerkersOverzichtService
{
    Task<IEnumerable<MedewerkerObjectData>> SearchMedewerkers(string query);
    Task<IEnumerable<MedewerkerObjectData>> FindMedewerkersByAfdelingOfGroep(string afdelingOfGroep, string type);
}

public class MedewerkersOverzichtService(IObjectApiClient objectApiClient) : IMedewerkersOverzichtService
{
    private const int MaxPages = 10;
    private readonly IObjectApiClient _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));

    public async Task<IEnumerable<MedewerkerObjectData>> SearchMedewerkers(string query)
    {
        var candidates = await GetMedewerkersRecursive(query, 1);
        var q = query.Trim();

        return candidates.Where(m =>
            (m.Voornaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
            (m.Achternaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
            (m.VolledigeNaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
            (m.VoorvoegselAchternaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true));
    }

    public async Task<IEnumerable<MedewerkerObjectData>> FindMedewerkersByAfdelingOfGroep(string afdelingOfGroep, string type)
    {
        var candidates = await GetMedewerkersRecursive(afdelingOfGroep, 1);

        return type.Equals(KnownActorType.Afdeling, StringComparison.OrdinalIgnoreCase)
            ? candidates.Where(m => m.Afdelingen?.Any(a => a.Afdelingnaam.Equals(afdelingOfGroep, StringComparison.OrdinalIgnoreCase)) == true)
            : candidates.Where(m => m.Groepen?.Any(g => g.Groepsnaam.Equals(afdelingOfGroep, StringComparison.OrdinalIgnoreCase)) == true);
    }

    private async Task<List<MedewerkerObjectData>> GetMedewerkersRecursive(string query, int page)
    {
        if (page > MaxPages) return [];

        var result = await _objectApiClient.FindMedewerkers(query, page);
        var medewerkers = result.Results.Select(x => x.Record.Data).ToList();

        if (result.Next != null)
        {
            var nextPageResult = await GetMedewerkersRecursive(query, page + 1);
            medewerkers.AddRange(nextPageResult);
        }

        return medewerkers;
    }
}
