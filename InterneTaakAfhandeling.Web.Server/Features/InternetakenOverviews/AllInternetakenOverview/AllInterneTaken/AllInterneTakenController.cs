using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.AllInternetakenOverview.AlleOverzicht
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize(Policy = FunctioneelBeheerderPolicy.Name)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class AlleOverzichtController(
        IInterneTakenOverzichtService interneTakenOverzichtService,
        ILogger<AlleOverzichtController> logger)
        : Controller
    {
        private readonly IInterneTakenOverzichtService _interneTakenOverzichtService = interneTakenOverzichtService ?? throw new ArgumentNullException(nameof(interneTakenOverzichtService));
        private readonly ILogger<AlleOverzichtController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [ProducesResponseType(typeof(InterneTakenOverviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetAlleOverzicht([FromQuery] AlleOverzichtQuery queryParameters)
        {
            try
            {
                var query = new InterneTaakQuery
                {
                    Page = queryParameters.Page,
                    PageSize = queryParameters.PageSize,
                    Status = queryParameters.Afgerond ? KnownInternetaakStatussen.Verwerkt : KnownInternetaakStatussen.TeVerwerken,
                    Actoren__Naam = string.IsNullOrWhiteSpace(queryParameters.AfdelingOfGroep) ? null : queryParameters.AfdelingOfGroep
                };

                var result = await _interneTakenOverzichtService.GetInterneTakenOverzichtAsync(query, queryParameters.Medewerker);

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
                _logger.LogError(ex, "Error fetching interne taken Overzicht with page {Page}, pageSize {PageSize}",
                    queryParameters.Page, queryParameters.PageSize);
                throw;
            }
        }
    }

    public class AlleOverzichtQuery : InterneTakenOverviewQueryParameters
    {
        /// <summary>
        /// Naam of an afdeling or groep: only show contactverzoeken assigned to it.
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? AfdelingOfGroep { get; set; }

        /// <summary>
        /// false (default): open contactverzoeken; true: afgehandelde contactverzoeken.
        /// </summary>
        public bool Afgerond { get; set; }
    }
}
