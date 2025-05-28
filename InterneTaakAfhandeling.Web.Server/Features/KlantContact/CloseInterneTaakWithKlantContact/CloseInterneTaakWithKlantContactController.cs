using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static InterneTaakAfhandeling.Common.Services.OpenKlantApi.OpenKlantApiClient;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CloseInterneTaakWithKlantContact
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class CloseInterneTaakWithKlantContactController(
        ITAUser user,
        ICreateKlantContactService createKlantContactService,
        ILogger<CloseInterneTaakWithKlantContactController> logger,
        IOpenKlantApiClient openKlantApiClient) : Controller
    {
        private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));
        private readonly ICreateKlantContactService _createKlantContactService = createKlantContactService ?? throw new ArgumentNullException(nameof(createKlantContactService));
        private readonly ILogger<CloseInterneTaakWithKlantContactController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [ProducesResponseType(typeof(RelatedKlantcontactResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("closeInterneTaakWithKlantContact")]
        public async Task<IActionResult> CreateRelatedKlantcontact([FromBody] RequestModel request)
        {
            try
            {
                var sanatizedAanleidinggevendKlantcontactUuid = Common.Helpers.SecureLogging.SanitizeUuid(request.AanleidinggevendKlantcontactUuid);
                _logger.LogInformation("Closing Interne taak and creating related klantcontact with aanleidinggevendKlantcontact UUID: {sanatizedAanleidinggevendKlantcontactUuid}", sanatizedAanleidinggevendKlantcontactUuid);


                var result = await _createKlantContactService.CreateRelatedKlantcontactAsync(
                    request.KlantcontactRequest,
                    request.AanleidinggevendKlantcontactUuid,
                    _user.Email,
                    _user.Name,
                    request.PartijUuid
                );


                var internetakenUpdateRequest = new InternetakenPatchRequest
                {
                    Status = "verwerkt"
                };

                await openKlantApiClient.PatchInternetaakAsync(internetakenUpdateRequest, request.InterneTaakId);

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (ConflictException ex)
            {
                var sanatizedInterneTaakId = Common.Helpers.SecureLogging.SanitizeUuid(request.InterneTaakId);
                _logger.LogError(ex, "Conflict error creating related klantcontact and closing internetaak {sanatizedInterneTaakId},  {ex.Message}", sanatizedInterneTaakId, ex.Message);
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = ex.Code ?? ""
                });
            }
            catch (Exception ex)
            {
                var sanatizedInterneTaakId = Common.Helpers.SecureLogging.SanitizeUuid(request.InterneTaakId);
                _logger.LogError(ex, "Unexpected error creating related klantcontactand and closing internetaak {sanatizedInterneTaakId},: {ex.Message}", sanatizedInterneTaakId, ex.Message);
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = "UNEXPECTED_ERROR"
                });
            }
        }
    }


}