using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Exceptions;
using InterneTaakAfhandeling.Web.Server.Services.LogboekService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CloseInterneTaakWithKlantContact;

[Route("api/internetaken")]
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class CloseInterneTaakWithKlantContactController(
    ITAUser user,
    ICreateKlantContactService createKlantContactService,
    ILogger<CloseInterneTaakWithKlantContactController> logger,
    IOpenKlantApiClient openKlantApiClient,
    ILogboekService logboekService) : Controller
{
    private readonly ICreateKlantContactService _createKlantContactService =
        createKlantContactService ?? throw new ArgumentNullException(nameof(createKlantContactService));

    private readonly ILogboekService _logboekService =
        logboekService ?? throw new ArgumentNullException(nameof(logboekService));

    private readonly ILogger<CloseInterneTaakWithKlantContactController> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly ITAUser _user = user ?? throw new ArgumentNullException(nameof(user));

    private readonly IOpenKlantApiClient openKlantApiClient =
        openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));

    [ProducesResponseType(typeof(RelatedKlantcontactResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
    [HttpPost("close-with-klantcontact")] //todo: refactor to "HttpPost("[internetaakUuid]/close-with-klantcontact)" and consider making it a PUT
    public async Task<IActionResult> CreateRelatedKlantcontact([FromBody] RequestModel request)
    {
        try
        {
            _logger.LogInformation(
                "Closing Interne taak and creating related klantcontact with aanleidinggevendKlantcontact UUID: {AanleidinggevendKlantcontactUuid}",
                request.AanleidinggevendKlantcontactUuid);

            //check if the internetaak that will be closed and the klantcontact to which the new klantcontact will be related belong to eachother
            var interneTaak = await openKlantApiClient.GetInternetaakByIdAsync(request.InterneTaakId);

            if (interneTaak?.Uuid != request.InterneTaakId.ToString())
                return BadRequest(new ProblemDetails
                {
                    Title = "Ongeldige aanvraag",
                    Detail = "Internetaak en klantcontact zijn niet gerelateerd",
                    Status = StatusCodes.Status400BadRequest
                });

            var result = await _createKlantContactService.CreateRelatedKlantcontactAsync(
                request.KlantcontactRequest,
                request.AanleidinggevendKlantcontactUuid,
                request.InterneTaakId,
                _user.Email,
                _user.Name,
                request.PartijUuid
            );

            //note: the 'afgehandeldOp' datetime is set automatically by OpenKlant
            //that behaviour is not documented in the API specs
            //the field is marked 'experimental' 
            var internetakenUpdateRequest = new InternetakenPatchRequest
            {
                Status = KnownInternetaakStatussen.Verwerkt
            };

            await openKlantApiClient.PatchInternetaakAsync(internetakenUpdateRequest, request.InterneTaakId.ToString());

            // logging klantcontact
            await _logboekService.LogContactRequestAction(KnownContactAction.Klantcontact(result, _user, request.InterneNotitie), request.InterneTaakId);

            // logging the completed action
            await _logboekService.LogContactRequestAction(KnownContactAction.Completed(_user), request.InterneTaakId);

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex,
                "Conflict error creating related klantcontact and closing internetaak {interneTaakId}, {ex.Message}",
                request.InterneTaakId, ex.Message);
            return StatusCode(409, new ITAException
            {
                Message = ex.Message,
                Code = ex.Code ?? ""
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error creating related klantcontactand and closing internetaak {interneTaakId}, {ex.Message}",
                request.InterneTaakId, ex.Message);
            return StatusCode(409, new ITAException
            {
                Message = ex.Message,
                Code = "UNEXPECTED_ERROR"
            });
        }
    }
}