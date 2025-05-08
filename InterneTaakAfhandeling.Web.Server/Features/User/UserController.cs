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
                if (string.IsNullOrEmpty(UserEmail))
                {
                    return StatusCode(409, new ITAException
                    {
                        Message = "No email found in the current user's claims.",
                        Code = "MISSING_EMAIL_CLAIM"
                    });
                }

                // Verkrijg of maak een actor voor de huidige gebruiker
                var actor = await _openKlantApiClient.GetOrCreateActorByEmail(UserEmail, User?.Identity?.Name);

                if (actor == null)
                {
                    return StatusCode(409, new ITAException
                    {
                        Message = "Failed to get or create actor for the current user.",
                        Code = "ACTOR_CREATION_FAILED"
                    });
                }

                // Maak het klantcontact aan
                var klantcontact = await _openKlantApiClient.CreateKlantcontactAsync(request);

                // Koppel de actor aan het klantcontact
                var actorKlantcontactRequest = new ActorKlantcontactRequest
                {
                    Actor = new ActorReference { Uuid = actor.Uuid },
                    Klantcontact = new KlantcontactReference { Uuid = klantcontact.Uuid }
                };

                var actorKlantcontact = await _openKlantApiClient.CreateActorKlantcontactAsync(actorKlantcontactRequest);

                // Combineer de resultaten in een anoniem object
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
    }
}