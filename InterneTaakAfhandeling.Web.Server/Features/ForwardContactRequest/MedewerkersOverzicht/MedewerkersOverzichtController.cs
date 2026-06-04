using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.ForwardContactRequest.MedewerkersOverzicht;

[Route("api")]
[ApiController]
[Authorize(Policy = ITAPolicy.Name)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class MedewerkersOverzichtController(ILogger<MedewerkersOverzichtController> logger, IMedewerkersOverzichtService medewerkersOverzichtService)
    : Controller
{
    private readonly ILogger<MedewerkersOverzichtController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMedewerkersOverzichtService _medewerkersOverzichtService = medewerkersOverzichtService ?? throw new ArgumentNullException(nameof(medewerkersOverzichtService));

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
                medewerkers = await _medewerkersOverzichtService.SearchMedewerkers(search);
            }
            else if (!string.IsNullOrWhiteSpace(afdelingOfGroep) && !string.IsNullOrWhiteSpace(type))
            {
                medewerkers = await _medewerkersOverzichtService.FindMedewerkersByAfdelingOfGroep(afdelingOfGroep, type);
            }
            else
            {
                return BadRequest("Either 'search' or both 'afdelingOfGroep' and 'type' query parameters are required.");
            }

            var result = medewerkers.Select(x => new
            {
                Naam = string.IsNullOrWhiteSpace(x.VolledigeNaam)
                    ? string.Join(" ", new[] { x.Voornaam, x.VoorvoegselAchternaam, x.Achternaam }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    : x.VolledigeNaam,
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
}
