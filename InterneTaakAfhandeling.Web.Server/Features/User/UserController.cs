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

namespace InterneTaakAfhandeling.Web.Server.Features.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class UserController(IOpenKlantApiClient openKlantApiClient, IUserService userService, ITAUser user) : Controller
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
        
        [ProducesResponseType(typeof(Klantcontact), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("klantcontacten")]
        public async Task<IActionResult> CreateKlantcontact([FromBody] KlantcontactRequest request)
        {
            var klantcontact = await _openKlantApiClient.CreateKlantcontactAsync(request);
            return StatusCode(StatusCodes.Status201Created, klantcontact);
        }

        [ProducesResponseType(typeof(ActorKlantcontact), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("actorklantcontacten")]
        public async Task<IActionResult> CreateActorKlantcontact([FromBody] ActorKlantcontactRequest request)
        {
            var actorKlantcontact = await _openKlantApiClient.CreateActorKlantcontactAsync(request);
            return StatusCode(StatusCodes.Status201Created, actorKlantcontact);
        }
        [ProducesResponseType(typeof(Klantcontact), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("klantcontactenmetactor")]
        public async Task<IActionResult> CreateKlantcontactWithCurrentActor([FromBody] KlantcontactRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Email))
                {
                    return StatusCode(409, new ITAException
                    {
                        Message = "No email found in the current user's claims.",
                        Code = "MISSING_EMAIL_CLAIM"
                    });
                }

                var actor = await _openKlantApiClient.GetOrCreateActorByEmail(user.Email, User?.Identity?.Name);

                if (actor == null)
                {
                    return StatusCode(409, new ITAException
                    {
                        Message = "Failed to get or create actor for the current user.",
                        Code = "ACTOR_CREATION_FAILED"
                    });
                }

                var klantcontact = await _openKlantApiClient.CreateKlantcontactAsync(request);
                var actorKlantcontactRequest = new ActorKlantcontactRequest
                {
                    Actor = new ActorReference { Uuid = actor.Uuid },
                    Klantcontact = new KlantcontactReference { Uuid = klantcontact.Uuid }
                };

                var actorKlantcontact = await _openKlantApiClient.CreateActorKlantcontactAsync(actorKlantcontactRequest);
                var result = new
                {
                    Klantcontact = klantcontact,
                    ActorKlantcontact = actorKlantcontact
                };

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception ex)
            {
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = (ex as ConflictException)?.Code
                });
            }
        }
        [ProducesResponseType(typeof(Onderwerpobject), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("onderwerpobjecten")]
        public async Task<IActionResult> CreateOnderwerpobject([FromBody] Onderwerpobject request)
        {
            try
            {
                var onderwerpobject = await _openKlantApiClient.CreateOnderwerpobjectAsync(request);
                return StatusCode(StatusCodes.Status201Created, onderwerpobject);
            }
            catch (Exception ex)
            {
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = (ex as ConflictException)?.Code
                });
            }
        }
    }
}