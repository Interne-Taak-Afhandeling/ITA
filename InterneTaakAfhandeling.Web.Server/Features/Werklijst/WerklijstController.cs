using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Werklijst;

[Route("api/werklijst")]
[ApiController]
[Authorize(Policy = CoordinatorPolicy.Name)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class WerklijstController(
    IWerklijstService werklijstService,
    ITAUser user,
    ILogger<WerklijstController> logger) : Controller
{
    private readonly IWerklijstService _werklijstService = werklijstService ?? throw new ArgumentNullException(nameof(werklijstService));
    private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));
    private readonly ILogger<WerklijstController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [ProducesResponseType(typeof(WerklijstResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<IActionResult> GetWerklijst([FromQuery] WerklijstQuery query)
    {
        try
        {
            var result = await _werklijstService.GetWerklijstAsync(query, _user);
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
            _logger.LogError(ex, "Error fetching werklijst with page {Page}, pageSize {PageSize}",
                query.Page, query.PageSize);
            throw;
        }
    }
}
