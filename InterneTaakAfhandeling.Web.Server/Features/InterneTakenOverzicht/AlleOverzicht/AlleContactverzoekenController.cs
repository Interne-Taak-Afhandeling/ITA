using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht.Model;
using InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht.AlleOverzicht
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize(Policy = ITAPolicy.Name)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AlleOverzichtController(
        IInterneTakenOverzichtService interneTakenOverzichtService,
        ILogger<AlleOverzichtController> logger)
        : Controller
    {
        private readonly IInterneTakenOverzichtService _interneTakenOverzichtService = interneTakenOverzichtService ?? throw new ArgumentNullException(nameof(interneTakenOverzichtService));
        private readonly ILogger<AlleOverzichtController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [ProducesResponseType(typeof(InterneTakenOverzichtResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpGet("alle-overzicht")]
        public async Task<IActionResult> GetAlleOverzicht([FromQuery] AlleOverzichtQuery queryParameters)
        {
            try
            {
                _logger.LogInformation("Fetching interne taken Overzicht with page {Page}, pageSize {PageSize}",
                    queryParameters.Page, queryParameters.PageSize);

                var query = new InterneTaakQuery
                {
                    Page = queryParameters.GetValidatedPage(),
                    PageSize = queryParameters.GetValidatedPageSize(),
                    Status = KnownInternetaakStatussen.TeVerwerken
                };


                var result = await _interneTakenOverzichtService.GetInterneTakenOverzichtAsync(query);

                _logger.LogInformation("Successfully fetched {Count} interne taken out of {Total} total",
                    result.Results.Count, result.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interne taken Overzicht with page {Page}, pageSize {PageSize}",
                    queryParameters.Page, queryParameters.PageSize);
                throw;
            }
        }
    }


    public class AlleOverzichtQuery : InterneTakenOverzichtQueryParameters
    {

    }
}