using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.CreateKlantContact;
using InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static InterneTaakAfhandeling.Common.Services.OpenKlantApi.OpenKlantApiClient;

namespace InterneTaakAfhandeling.Web.Server.Features.CreateKlantContact
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class CreateKlantContactController(
        ITAUser user,
        ICreateKlantContactService createKlantContactService) : Controller
    {
        private readonly ICreateKlantContactService _createKlantContactService = createKlantContactService ?? throw new ArgumentNullException(nameof(createKlantContactService));

        [ProducesResponseType(typeof(RelatedKlantcontactResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost("relatedklantcontact")]
        public async Task<IActionResult> CreateRelatedKlantcontact([FromBody] CreateRelatedKlantcontactRequest request)
        {
            try
            {
                var result = await _createKlantContactService.CreateRelatedKlantcontactAsync(
                    request.KlantcontactRequest,
                    request.PreviousKlantcontactUuid,
                    user.Email,
                    user.Name
                );

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (ConflictException ex)
            {
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = ex.Code
                });
            }
            catch (Exception ex)
            {
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
        public string? PreviousKlantcontactUuid { get; set; }
    }

    public class RelatedKlantcontactResult
    {
        public required Klantcontact Klantcontact { get; set; }
        public required ActorKlantcontact ActorKlantcontact { get; set; }
        public Onderwerpobject? Onderwerpobject { get; set; }
    }
}