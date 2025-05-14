using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetaakContactmomentenController
{
    [Route("api/klantcontacten")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class InternetaakContactmomentenController(IOpenKlantApiClient openKlantApiClient, IUserService userService, ITAUser user) : Controller
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));


        [ProducesResponseType(typeof(List<Internetaken>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("{aanleidinggevendKlantcontactId}")]
        public IActionResult Get(string aanleidinggevendKlantcontactId)
        {
            //todo: implement in story pc-1245
            //haal recursief de onderwerpobjecten op van het contactmoment. haal de contactmomenten op war die naar verwijzien. repeat    
            return Ok(new List<ContactmomentResponse>());

        }


        record ContactmomentResponse(
          bool ContactGelukt,
          string Tekst,
          string Datum,
          string Medewerker,
          string Kanaal
     );
    }

}