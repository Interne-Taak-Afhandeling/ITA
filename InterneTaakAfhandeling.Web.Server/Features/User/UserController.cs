using Duende.IdentityModel;
using InterneTaakAfhandeling.Web.Server.Features.Base;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 
namespace InterneTaakAfhandeling.Web.Server.Features.User
{ 
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class UserController(IOpenKlantApiClient openKlantApiClient) : BaseController
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
 
        [ProducesResponseType(typeof(List<Internetaken>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpGet("internetaken")]
        public async Task<IActionResult> GetInternetaken()
        {
            return Ok(await _openKlantApiClient.GetInterneTakenByAssignedUser(UserEmail));
        }
        
        [ProducesResponseType(typeof(Klantcontact), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("klantcontacten")]
        public async Task<IActionResult> CreateKlantcontact([FromBody] KlantcontactRequest request)
        {
            var klantcontact = await _openKlantApiClient.CreateKlantcontactAsync(request);
            return StatusCode(StatusCodes.Status201Created, klantcontact);
        }
    }
}