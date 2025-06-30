using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using InterneTaakAfhandeling.Web.Server.Services.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Helpers;
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
        [HttpPost("{internetakenId}/assign-to-me")]
        public async Task<IActionResult> AssignInternetakenAsync([FromRoute] Guid internetakenId)
        {
            var safeUserEmail = SecureLogging.SanitizeAndTruncate(user.Email, 5);

            try
            {
                _logger.LogInformation("Assigning internetaak {InternetaakId} to user {SafeUserEmail}",
                    internetakenId, safeUserEmail);

                var (updatedInterneTaak, currentUserActor) = await _AssignInternetakenService.ToSelfAsync(internetakenId, user);

                var assignedAction = KnownContactAction.AssignedToSelf(Guid.Parse(currentUserActor.Uuid));
                await _logboekService.LogContactRequestAction(assignedAction, internetakenId);

                _logger.LogInformation("Successfully assigned internetaak {SafeInternetaakId} to user {SafeUserEmail} and logged action",
                    internetakenId, safeUserEmail);

                return Ok(updatedInterneTaak);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning internetaak {SafeInternetaakId} to user {SafeUserEmail}",
          internetakenId, safeUserEmail);
                throw;
            }
        }
    }
}



