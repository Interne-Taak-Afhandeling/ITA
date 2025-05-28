using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static InterneTaakAfhandeling.Common.Services.OpenKlantApi.OpenKlantApiClient;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CreateKlantContact
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class CreateKlantContactAndCloseInterneTaakController : Controller
    {
        private readonly ITAUser _user;
        private readonly ICreateKlantContactService _createKlantContactService;
        private readonly ILogger<CreateKlantContactAndCloseInterneTaakController> _logger;

        public CreateKlantContactAndCloseInterneTaakController(
            ITAUser user,
            ICreateKlantContactService createKlantContactService,
            ILogger<CreateKlantContactAndCloseInterneTaakController> logger)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _createKlantContactService = createKlantContactService ?? throw new ArgumentNullException(nameof(createKlantContactService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ProducesResponseType(typeof(RelatedKlantcontactResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("closeIrelatedklantcontactnterneTaakWithKlantContact")]
        public async Task<IActionResult> CreateRelatedKlantcontact([FromBody] CreateRelatedKlantcontactRequestModel request)
        {
            try
            {
                var sanatizedAanleidinggevendKlantcontactUuid = Common.Helpers.SecureLogging.SanitizeUuid(request.AanleidinggevendKlantcontactUuid);
                _logger.LogInformation("Creating related klantcontact with aanleidinggevendKlantcontact UUID: {sanatizedAanleidinggevendKlantcontactUuid}", sanatizedAanleidinggevendKlantcontactUuid);
                 
                var result = await _createKlantContactService.CreateRelatedKlantcontactAsync(
                    request.KlantcontactRequest,
                    request.AanleidinggevendKlantcontactUuid,
                    _user.Email,
                    _user.Name,
                    request.PartijUuid
                );

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (ConflictException ex)
            {
                var sanatizedAanleidinggevendKlantcontactUuid = Common.Helpers.SecureLogging.SanitizeUuid(request.AanleidinggevendKlantcontactUuid);
                _logger.LogError(ex, "Conflict error creating related klantcontact {sanatizedAanleidinggevendKlantcontactUuid}", sanatizedAanleidinggevendKlantcontactUuid);
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = ex.Code
                });
            }
            catch (Exception ex)
            {
                var sanatizedAanleidinggevendKlantcontactUuid = Common.Helpers.SecureLogging.SanitizeUuid(request.AanleidinggevendKlantcontactUuid);
                _logger.LogError(ex, "Unexpected error creating related klantcontact {sanatizedAanleidinggevendKlantcontactUuid}", sanatizedAanleidinggevendKlantcontactUuid);
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = "UNEXPECTED_ERROR"
                });
            }
        }
    }


 
}