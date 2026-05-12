using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.InterneTaak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    public class InternetaakDetailsController(IInternetaakService internetakenService) : Controller
    {

        private readonly IInternetaakService _internetakenService = internetakenService;

        [ProducesResponseType(typeof(Internetaak), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("{internetaakNummer}")]
        public async Task<IActionResult> Get([FromRoute] string internetaakNummer)
        {
            var interneTaakQueryParameters = new InterneTaakQuery { Nummer = internetaakNummer };
            var internetaak = await _internetakenService.Get(interneTaakQueryParameters);

            if (internetaak == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Interne taak niet gevonden",
                    Detail = $"Geen interne taak gevonden met identificatie: {interneTaakQueryParameters.Nummer}",
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (internetaak.AanleidinggevendKlantcontact?.Nummer == null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Contactmoment-nummer ontbreekt",
                    Detail = "Het aanleidinggevend klantcontact heeft geen nummer. Dit is een foutsituatie.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            return Ok(internetaak);
        }

        [ProducesResponseType(typeof(Internetaak), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [HttpGet("by-klantcontact/{nummer}")]
        public async Task<IActionResult> GetByKlantcontactNummer([FromRoute] string nummer)
        {
            var internetaak = await _internetakenService.GetByKlantcontactNummer(nummer);

            if (internetaak == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Interne taak niet gevonden",
                    Detail = $"Geen interne taak gevonden voor klantcontact met nummer: {nummer}",
                    Status = StatusCodes.Status404NotFound
                });
            }

            if (internetaak.AanleidinggevendKlantcontact?.Nummer == null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Contactmoment-nummer ontbreekt",
                    Detail = "Het aanleidinggevend klantcontact heeft geen nummer. Dit is een foutsituatie.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            return Ok(internetaak);
        }

    }
}
