using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class UserController(IUserService userService, ITAUser user) : Controller
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [ProducesResponseType(typeof(List<Common.Services.OpenKlantApi.Models.Internetaken>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("internetaken")]
        public async Task<IActionResult> GetInternetaken()
        {
            return Ok(await _userService.GetInterneTakenByAssignedUser(user));
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpPost("internetaken/{internetakenId}/assign-to-self")]
        public async Task<IActionResult> AssignInternetakenToSelfAsync([FromRoute] string internetakenId)
        {  
            return Ok(await _userService.AssignInternetakenToSelfAsync(internetakenId, user));
        }
    }
}




