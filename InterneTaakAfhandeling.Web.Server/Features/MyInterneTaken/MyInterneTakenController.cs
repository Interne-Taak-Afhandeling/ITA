using System.Security.Claims;
using Duende.IdentityModel;
using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static InterneTaakAfhandeling.Common.Services.OpenKlantApi.OpenKlantApiClient;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTaken
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class MyInterneTakenController(IOpenKlantApiClient openKlantApiClient, IUserService userService, ITAUser user) : Controller
    {
        private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));


        [ProducesResponseType(typeof(List<Internetaken>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("internetaken")]
        public async Task<IActionResult> GetInternetaken()
        {
          return Ok(await _userService.GetInterneTakenByAssignedUser(user));
        }      
    }
}