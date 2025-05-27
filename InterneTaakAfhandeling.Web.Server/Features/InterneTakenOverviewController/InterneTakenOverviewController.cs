using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverview
{
    [Route("api/internetaken-overview")]
    [ApiController]
    [Authorize(Policy = ITAPolicy.Name)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class InterneTakenOverviewController : Controller
    {
        private readonly IInterneTakenOverviewService _interneTakenOverviewService;
        private readonly ILogger<InterneTakenOverviewController> _logger;

        public InterneTakenOverviewController(
            IInterneTakenOverviewService interneTakenOverviewService,
            ILogger<InterneTakenOverviewController> logger)
        {
            _interneTakenOverviewService = interneTakenOverviewService ?? throw new ArgumentNullException(nameof(interneTakenOverviewService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ProducesResponseType(typeof(InterneTakenOverviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<IActionResult> GetInterneTakenOverview([FromQuery] InterneTakenOverviewQueryParameters queryParameters)
        {
            try
            {
                _logger.LogInformation("Fetching interne taken overview with page {Page}, pageSize {PageSize}",
                    queryParameters.Page, queryParameters.PageSize);

                var result = await _interneTakenOverviewService.GetInterneTakenOverviewAsync(queryParameters);

                _logger.LogInformation("Successfully fetched {Count} interne taken out of {Total} total",
                    result.Results.Count, result.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching interne taken overview with page {Page}, pageSize {PageSize}",
                    queryParameters.Page, queryParameters.PageSize);
                throw;
            }
        }
    }

    public class InterneTakenOverviewQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;

        // Validation to ensure reasonable pagination limits
        public int GetValidatedPage() => Math.Max(1, Page);
        public int GetValidatedPageSize() => Math.Min(Math.Max(1, PageSize), 100);
    }

    public class InterneTakenOverviewResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<InterneTaakOverviewItem> Results { get; set; } = new();
    }

    public class InterneTaakOverviewItem
    {
        public string Uuid { get; set; } = string.Empty;
        public string Nummer { get; set; } = string.Empty;
        public string GevraagdeHandeling { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset ToegewezenOp { get; set; }
        public DateTimeOffset? AfgehandeldOp { get; set; }

        // Contact information
        public string? Onderwerp { get; set; }
        public string? KlantNaam { get; set; }
        public DateTimeOffset? ContactDatum { get; set; }

        // Assignment information
        public string? AfdelingNaam { get; set; }
        public string? BehandelaarNaam { get; set; }
        public bool HeeftBehandelaar => !string.IsNullOrEmpty(BehandelaarNaam);
    }
}