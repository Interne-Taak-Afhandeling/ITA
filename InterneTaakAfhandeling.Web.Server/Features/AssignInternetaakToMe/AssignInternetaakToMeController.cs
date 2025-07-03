using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using InterneTaakAfhandeling.Web.Server.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.AssignInternetaakToMe
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class AssignInternetaakToMeController(
        IAssignInternetaakToMeService AssignInternetakenService,
        ITAUser user,
        ILogboekService logboekService,
        ILogger<AssignInternetaakToMeController> logger) : Controller
    {
        private readonly IAssignInternetaakToMeService _AssignInternetakenService = AssignInternetakenService;
        private readonly ILogboekService _logboekService = logboekService ?? throw new ArgumentNullException(nameof(logboekService));
        private readonly ILogger<AssignInternetaakToMeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("{internetaakId}/assign-to-me")]
        public async Task<IActionResult> AssignInternetakenAsync([FromRoute] Guid internetaakId)
        {
            try
            {
                var (updatedInterneTaak, currentUserActor) = await _AssignInternetakenService.ToSelfAsync(internetaakId, user);

                var assignedAction = KnownContactAction.AssignedToSelf(Guid.Parse(currentUserActor.Uuid));
                await _logboekService.LogContactRequestAction(assignedAction, internetaakId);

                return Ok(updatedInterneTaak);
            }
            catch (Exception ex)
            {
                var safeUserEmail = SecureLogging.SanitizeAndTruncate(user.Email, 5);

                _logger.LogError(ex, "Error assigning internetaak {InternetaakId} to user {SafeUserEmail}",
          internetaakId, safeUserEmail);
                throw;
            }
        }
    }
}



