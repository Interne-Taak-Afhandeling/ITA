using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview;

[Route("api/internetaken")]
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class MyInterneTakenOverviewController(
    IMyInterneTakenOverviewService myInterneTakenService,
    ILogger<MyInterneTakenOverviewController> logger,
    ITAUser user) : Controller
{
    private readonly IMyInterneTakenOverviewService _myInterneTakenService =
        myInterneTakenService ?? throw new ArgumentNullException(nameof(myInterneTakenService));

    [ProducesResponseType(typeof(List<Internetaak>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [HttpGet("aan-mij-toegewezen")]
    public async Task<IActionResult> GetMyInternetaken([FromQuery] bool afgerond)
    {
        var result = await _myInterneTakenService.GetMyInterneTakenAsync(user, afgerond);

        return Ok(result);
    }


    [ProducesResponseType(typeof(MyInterneTakenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [HttpGet]
    public async Task<IActionResult> GetInterneTaken([FromQuery] MyInterneTakenQueryParameters queryParameters)
    {
        try
        {
            logger.LogInformation("Fetching interne taken  with page {Page}, pageSize {PageSize}",
                queryParameters.Page, queryParameters.PageSize);


            var result = await _myInterneTakenService.GetMyInterneTakenOverviewAsync(queryParameters);

            logger.LogInformation("Successfully fetched {Count} interne taken out of {Total} total",
                result.Results.Count, result.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching interne taken  with page {Page}, pageSize {PageSize}",
                queryParameters.Page, queryParameters.PageSize);
            throw;
        }
    }
}