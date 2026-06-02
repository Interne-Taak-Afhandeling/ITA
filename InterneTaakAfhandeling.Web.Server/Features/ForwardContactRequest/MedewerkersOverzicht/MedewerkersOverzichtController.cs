using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest.MedewerkersOverzicht;

[Route("api")]
[ApiController]
[Authorize(Policy = ITAPolicy.Name)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class MedewerkersOverzichtController(ILogger<MedewerkersOverzichtController> logger, IObjectApiClient objectApiClient)
    : Controller
{
    private readonly ILogger<MedewerkersOverzichtController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IObjectApiClient _objectApiClient = objectApiClient ?? throw new ArgumentNullException(nameof(objectApiClient));

    /// <summary>
    /// Search medewerkers by free text (data_icontains) or filter by afdeling/groep name.
    /// Usage:
    ///   GET /api/medewerkers?search=Jan                           → free text search
    ///   GET /api/medewerkers?afdelingOfGroep=Burgerzaken&amp;type=Afdeling → filtered by afdeling
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpGet("medewerkers")]
    public async Task<IActionResult> GetMedewerkersOverzicht(
        [FromQuery] string? search = null,
        [FromQuery] string? afdelingOfGroep = null,
        [FromQuery] string? type = null)
    {
        try
        {
            IEnumerable<MedewerkerObjectData> medewerkers;

            if (!string.IsNullOrWhiteSpace(search))
            {
                medewerkers = await SearchMedewerkers(search);
            }
            else if (!string.IsNullOrWhiteSpace(afdelingOfGroep) && !string.IsNullOrWhiteSpace(type))
            {
                medewerkers = await FindMedewerkersByAfdelingOfGroep(afdelingOfGroep, type);
            }
            else
            {
                return BadRequest("Either 'search' or both 'afdelingOfGroep' and 'type' query parameters are required.");
            }

            var result = medewerkers.Select(x => new
            {
                Naam = x.VolledigeNaam ?? string.Join(" ", new[] { x.Voornaam, x.VoorvoegselAchternaam, x.Achternaam }.Where(s => !string.IsNullOrWhiteSpace(s))),
                x.Identificatie,
                Afdelingen = x.Afdelingen?.Select(a => new { a.Afdelingnaam }).ToList() ?? [],
                Groepen = x.Groepen?.Select(g => new { g.Groepsnaam }).ToList() ?? []
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching medewerkers");
            return Problem("Er is een fout opgetreden bij het ophalen van de lijst met medewerkers");
        }
    }

    private async Task<IEnumerable<MedewerkerObjectData>> SearchMedewerkers(string query)
    {
        var candidates = await GetMedewerkersRecursive(query, 1);
        var q = query.Trim();

        return candidates.Where(m =>
            (m.Voornaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
            (m.Achternaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
            (m.VolledigeNaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true) ||
            (m.VoorvoegselAchternaam?.Contains(q, StringComparison.OrdinalIgnoreCase) == true));
    }

    private async Task<IEnumerable<MedewerkerObjectData>> FindMedewerkersByAfdelingOfGroep(string afdelingOfGroep, string type)
    {
        var candidates = await GetMedewerkersRecursive(afdelingOfGroep, 1);

        return type.Equals(KnownActorType.Afdeling, StringComparison.OrdinalIgnoreCase)
            ? candidates.Where(m => m.Afdelingen?.Any(a => a.Afdelingnaam.Equals(afdelingOfGroep, StringComparison.OrdinalIgnoreCase)) == true)
            : candidates.Where(m => m.Groepen?.Any(g => g.Groepsnaam.Equals(afdelingOfGroep, StringComparison.OrdinalIgnoreCase)) == true);
    }

    private async Task<List<MedewerkerObjectData>> GetMedewerkersRecursive(string query, int page)
    {
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
