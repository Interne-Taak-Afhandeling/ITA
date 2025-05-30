using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.InterneTaak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    public class InternetaakController(IInternetaakService internetakenService) : Controller
    {

        private readonly IInternetaakService _internetakenService = internetakenService;

        [ProducesResponseType(typeof(Internetaak), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] InterneTaakQuery interneTaakQueryParameters)
        {
            var internetaak = await _internetakenService.Get(interneTaakQueryParameters);

            if (internetaak == null) {
                NotFound(new ProblemDetails
                {
                    Title = "Interne taak niet gevonden",
                    Detail = $"Geen interne taak gevonden met identificatie: {interneTaakQueryParameters.Nummer}",
                    Status = StatusCodes.Status404NotFound
                });
            };


            return Ok(internetaak);
        }

    }
}
