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
        public async Task<IActionResult> AssignInternetakenAsync([FromRoute] string internetakenId)
        {
            try
            {
                var safeInternetaakId = SecureLogging.SanitizeUuid(internetakenId);
                var safeUserEmail = SecureLogging.SanitizeAndTruncate(user.Email, 5);

                _logger.LogInformation("Assigning internetaak {SafeInternetaakId} to user {SafeUserEmail}",
                    safeInternetaakId, safeUserEmail);

                var (result, currentUserActor) = await _AssignInternetakenService.ToSelfAsync(internetakenId, user);

                var assignedAction = KnownContactAction.AssignedToSelf(Guid.Parse(currentUserActor.Uuid));
                await _logboekService.LogContactRequestAction(assignedAction, Guid.Parse(internetakenId));

                _logger.LogInformation("Successfully assigned internetaak {SafeInternetaakId} to user {SafeUserEmail} and logged action",
                    safeInternetaakId, safeUserEmail);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var safeInternetaakId = SecureLogging.SanitizeUuid(internetakenId);
                var safeUserEmail = SecureLogging.SanitizeAndTruncate(user.Email, 5);

                _logger.LogError(ex, "Error assigning internetaak {SafeInternetaakId} to user {SafeUserEmail}",
                    safeInternetaakId, safeUserEmail);
                throw;
            }
        }
    }
}