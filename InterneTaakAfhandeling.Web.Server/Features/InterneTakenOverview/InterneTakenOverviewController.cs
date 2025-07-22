using System.ComponentModel.DataAnnotations;
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
    public class InterneTakenOverviewController(
        IInterneTakenOverviewService interneTakenOverviewService,
        ILogger<InterneTakenOverviewController> logger)
        : Controller
    {
        private readonly IInterneTakenOverviewService _interneTakenOverviewService = interneTakenOverviewService ?? throw new ArgumentNullException(nameof(interneTakenOverviewService));
        private readonly ILogger<InterneTakenOverviewController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
    public enum IntertaakStatus
    { 
        TeVerwerken,
        Verwerkt
    }

    public class InterneTakenOverviewQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        public string? NaamActeur { get; set; }
        [EnumDataType(typeof(IntertaakStatus))]
        public IntertaakStatus? Status { get; set; }
        public int GetValidatedPage() => Math.Max(1, Page);
        public int GetValidatedPageSize() => Math.Min(Math.Max(1, PageSize), 50);
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
        public string Uuid { get; set; } = string.Empty;                    // Internetaak.Uuid
        public string Nummer { get; set; } = string.Empty;                  // Internetaak.Nummer
        public string GevraagdeHandeling { get; set; } = string.Empty;      // Internetaak.GevraagdeHandeling
        public string Status { get; set; } = string.Empty;                  // Internetaak.Status
        public DateTimeOffset ToegewezenOp { get; set; }                    // Internetaak.ToegewezenOp
        public DateTimeOffset? AfgehandeldOp { get; set; }                  // Internetaak.AfgehandeldOp

        public string? Onderwerp { get; set; }                             // Klantcontact.Onderwerp
        public DateTimeOffset? ContactDatum { get; set; }                  // Klantcontact.PlaatsgevondenOp

        public string? KlantNaam { get; set; }                             // Klantcontact.Expand.HadBetrokkenen[].VolledigeNaam ?? Organisatienaam

        public string? AfdelingNaam { get; set; }                          // Actor.Naam (waar SoortActor != medewerker)
        public string? BehandelaarNaam { get; set; }                       // Actor.Naam (waar SoortActor == medewerker)
    }
}