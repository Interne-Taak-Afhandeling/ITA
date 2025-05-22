using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InternetakenController(IInternetakenService internetakenService, IUserService userService, ITAUser user) : Controller
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        private readonly IInternetakenService _internetakenService = internetakenService;

        [ProducesResponseType(typeof(Common.Services.OpenKlantApi.Models.Internetaken), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] InterneTaakQueryParameters interneTaakQueryParameters)
        {
            var intertakens = await _internetakenService.Get(interneTaakQueryParameters);
            return Ok(intertakens);
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("{internetakenId}/assign-to-self")]
        public async Task<IActionResult> AssignInternetakenToSelfAsync([FromRoute] string internetakenId)
        {
            return Ok(await _userService.AssignInternetakenToSelfAsync(internetakenId, user));
        }

    }
}
