using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht.AfdelingenGroepenOverzicht;

[Route("api/internetaken")]
[ApiController]
[Authorize(Policy = ITAPolicy.Name)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AfdelingenGroepenOverzichtController(
    IInterneTakenOverzichtService interneTakenOverzichtService,
    ILogger<AfdelingenGroepenOverzichtController> logger)
    : Controller
{
    private readonly IInterneTakenOverzichtService _interneTakenOverzichtService = interneTakenOverzichtService ??
        throw new ArgumentNullException(nameof(interneTakenOverzichtService));

    private readonly ILogger<AfdelingenGroepenOverzichtController> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    [ProducesResponseType(typeof(InterneTakenOverzichtResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpGet("afdelingen-groepen")]
    public async Task<IActionResult> GetAfdelingenGroepenOverzicht(
        [FromQuery] AfdelingenGroepenOverzichtQuery queryParameters)
    {
        try
        {
            var query = new InterneTaakQuery
            {
                Page = queryParameters.Page,
                PageSize = queryParameters.PageSize,
                Actoren__Naam = queryParameters.NaamActor,
                Status = queryParameters.Afgerond ? KnownInternetaakStatussen.Verwerkt : KnownInternetaakStatussen.TeVerwerken
            };
            var result = await _interneTakenOverzichtService.GetInterneTakenOverzichtAsync(query);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching interne taken Overzicht for afdelingen en groepen with page {Page}, pageSize {PageSize}",
                queryParameters.Page, queryParameters.PageSize);
            throw;
        }
    }
}

public class AfdelingenGroepenOverzichtQuery : InterneTakenOverzichtQueryParameters
{
    public required string NaamActor { get; set; }
    public bool Afgerond { get; set; }
}