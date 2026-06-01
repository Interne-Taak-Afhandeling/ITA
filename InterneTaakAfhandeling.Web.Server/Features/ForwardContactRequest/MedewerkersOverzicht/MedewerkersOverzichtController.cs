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

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpGet("medewerkers")]
    public async Task<IActionResult> GetMedewerkersOverzicht(
        [FromQuery] string afdelingOfGroep,
        [FromQuery] string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(afdelingOfGroep) || string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("Both 'afdelingOfGroep' and 'type' query parameters are required.");
            }

            var medewerkers = await FindMedewerkersByAfdelingOfGroep(afdelingOfGroep, type);

            var result = medewerkers.Select(x => new
            {
                Naam = x.VolledigeNaam ?? $"{x.Voornaam} {x.VoorvoegselAchternaam} {x.Achternaam}".Trim(),
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

    private async Task<IEnumerable<MedewerkerObjectData>> FindMedewerkersByAfdelingOfGroep(string afdelingOfGroep, string type)
    {
        var searchResult = await _objectApiClient.FindMedewerkers(afdelingOfGroep);
        var candidates = searchResult.Results.Select(x => x.Record.Data).ToList();

        return type.Equals(KnownActorType.Afdeling, StringComparison.OrdinalIgnoreCase)
            ? candidates.Where(m => m.Afdelingen?.Any(a => a.Afdelingnaam.Equals(afdelingOfGroep, StringComparison.OrdinalIgnoreCase)) == true)
            : candidates.Where(m => m.Groepen?.Any(g => g.Groepsnaam.Equals(afdelingOfGroep, StringComparison.OrdinalIgnoreCase)) == true);
    }
}
