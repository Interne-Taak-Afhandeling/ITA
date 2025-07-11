﻿using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("{internetaakNummer}")]

        //todo: refactor to use Id instead of nummer
        public async Task<IActionResult> Get([FromRoute] string internetaakNummer)
        {
            var interneTaakQueryParameters = new InterneTaakQuery { Nummer = internetaakNummer };
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
