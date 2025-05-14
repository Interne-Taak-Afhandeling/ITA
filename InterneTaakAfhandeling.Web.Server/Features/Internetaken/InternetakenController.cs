using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InternetakenController(IInternetakenService internetakenService) : Controller
    {

        private readonly IInternetakenService _internetakenService = internetakenService;

        [ProducesResponseType(typeof(Common.Services.OpenKlantApi.Models.Internetaken), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("nummer/{nummer}")]
        public async Task<IActionResult> GetByNummer(string nummer)
        {
            var intertakens = await _internetakenService.GetByNummerAsync(nummer);
            return Ok(intertakens);
        }

    }
}
