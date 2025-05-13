using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetaakContactmomentenController
{
    [Route("api/internetaak")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class InternetaakContactmomentenController(IOpenKlantApiClient openKlantApiClient, IUserService userService, ITAUser user) : Controller
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));


        [ProducesResponseType(typeof(List<Internetaken>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("{contactverzoekId}/contactmomenten")]
        public async Task<IActionResult> Get(string contactverzoekId)
        {

            //get the internetaak
            var internetaak = await _openKlantApiClient.GetInternetaak(contactverzoekId);


            if(internetaak == null )
            {
                return NotFound();
            }

            var klantcontactId = internetaak.AanleidinggevendKlantcontact?.Uuid;

            if (string.IsNullOrWhiteSpace(klantcontactId))
            {
                return NotFound();
            }

            //get the klantcontact the initiated this internetaak
            var contactmoment = await _openKlantApiClient.GetKlantcontact(klantcontactId);


            if (contactmoment == null)
            {
                return NotFound();
            }

            //get the onderwerpobjecten (of type klantcontact) related to that klantcontact

            var ondewerpobjecten = contactmoment.GingOverOnderwerpobjecten;

            if (ondewerpobjecten == null)
            {
                return Ok(new List<ContactmomentResponse>());
               
            }


            var result = new List<ContactmomentResponse>();

            foreach (var ondewerpobject in ondewerpobjecten)
            {
                //get the klantcontacten that those onderwerpobjecten refer to

                // var onderwerpobject = _openKlantApiClient.GetOnderwerpObject();
                // var contactmoment = await _openKlantApiClient.GetKlantcontact(onderwerpobject.Klantcontact.Uuid);
                // result.Add(contactmoment);

            }

            // return result;

            return Ok(new List<ContactmomentResponse>() {
               new ContactmomentResponse( true, "test " + contactverzoekId , "15-04-2025", "Piet van Gelre", "Telefoon" ),
                 new ContactmomentResponse( true, "teeeeest", "15-04-2025", "Piet van Gelre", "Telefoon" )
            });


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