
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services.LogboekService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTaakLogboek
{

    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class InterneTaakLogboekController(
        ILogboekService logboekService,
        ILogger<InterneTaakLogboekController> logger) : Controller
    {
        private readonly ILogboekService _logboekService = logboekService ?? throw new ArgumentNullException(nameof(logboekService));
        private readonly ILogger<InterneTaakLogboekController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [ProducesResponseType(typeof(LogboekData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("{internetaakId}/logboek")]
        public async Task<IActionResult> Get(Guid internetaakId)
        {
            try
            {
                var response = await _logboekService.GetLogboek(internetaakId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve logboek for internetaak {internetaakId}", internetaakId);
                throw;
            }
        }
     
    }
}