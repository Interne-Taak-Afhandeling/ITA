using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.CreateKlantContact
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class CreateKlantContactController : Controller
    {
        private readonly ITAUser _user;
        private readonly ICreateKlantContactService _createKlantContactService;
        private readonly ILogger<CreateKlantContactController> _logger;

        public CreateKlantContactController(
            ITAUser user,
            ICreateKlantContactService createKlantContactService,
            ILogger<CreateKlantContactController> logger)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _createKlantContactService = createKlantContactService ?? throw new ArgumentNullException(nameof(createKlantContactService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ProducesResponseType(typeof(RelatedKlantcontactResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("relatedklantcontact")]
        public async Task<IActionResult> CreateRelatedKlantcontact([FromBody] CreateRelatedKlantcontactRequest request)
        {
            try
            {

                if (Guid.TryParse(request.AanleidinggevendKlantcontactUuid, out Guid parsedKlantcontact))
                {
                    if (Guid.TryParse(request.PartijUuid, out Guid parsedPartijUuid))
                    {
                        _logger.LogInformation($"Creating related klantcontact with aanleidinggevendKlantcontact UUID: {parsedKlantcontact}, partij UUID: {parsedPartijUuid}");
                    }
                }                

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
                _logger.LogWarning(ex, $"Conflict error creating related klantcontact: {ex.Message}");
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = ex.Code
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error creating related klantcontact: {ex.Message}");
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = "UNEXPECTED_ERROR"
                });
            }
        }
    }

    public class CreateRelatedKlantcontactRequest
    {
        public required KlantcontactRequest KlantcontactRequest { get; set; }
        public string? AanleidinggevendKlantcontactUuid { get; set; }
        public string? PartijUuid { get; set; }
    }

    public class ITAException
    {
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}