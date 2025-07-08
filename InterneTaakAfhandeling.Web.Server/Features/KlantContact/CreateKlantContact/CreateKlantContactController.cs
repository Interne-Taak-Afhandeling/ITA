using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Exceptions;
using InterneTaakAfhandeling.Web.Server.Services;
using InterneTaakAfhandeling.Web.Server.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CreateKlantContact;

[Route("api/klantcontacten")]
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class CreateKlantContactController : Controller
{
    private readonly ICreateKlantContactService _createKlantContactService;
    private readonly ILogboekService _logboekService;
    private readonly ILogger<CreateKlantContactController> _logger;
    private readonly ITAUser _user;


    public CreateKlantContactController(
        ITAUser user,
        ICreateKlantContactService createKlantContactService,
        ILogger<CreateKlantContactController> logger,
        ILogboekService logboekService)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _createKlantContactService = createKlantContactService ??
                                     throw new ArgumentNullException(nameof(createKlantContactService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logboekService = logboekService ?? throw new ArgumentNullException(nameof(logboekService));
    }

    [ProducesResponseType(typeof(RelatedKlantcontactResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
    [HttpPost("add-klantcontact")] //todo refactor, route should be api/klantcontacten/[klantcontactUuid]/klantcontacten
    public async Task<IActionResult> CreateRelatedKlantcontact([FromBody] CreateRelatedKlantcontactRequestModel request)
    {
        try
        {
            _logger.LogInformation(
                "Creating related klantcontact with aanleidinggevendKlantcontact UUID: {aanleidinggevendKlantcontactUuid}",
                request.AanleidinggevendKlantcontactUuid);

            var result = await _createKlantContactService.CreateRelatedKlantcontactAsync(
                request.KlantcontactRequest,
                request.AanleidinggevendKlantcontactUuid,
                request.InterneTaakId,
                _user.Email,
                _user.Name,
                request.PartijUuid
            );

            // logging klantcontact
            await _logboekService.LogContactRequestAction(KnownContactAction.Klantcontact(result, _user), request.InterneTaakId);

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ConflictException ex)
        {
            _logger.LogError(ex, "Conflict error creating related klantcontact {aanleidinggevendKlantcontactUuid}",
                request.AanleidinggevendKlantcontactUuid);
            return StatusCode(409, new ITAException
            {
                Message = ex.Message,
                Code = ex.Code
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating related klantcontact {aanleidinggevendKlantcontactUuid}",
                request.AanleidinggevendKlantcontactUuid);
            return StatusCode(409, new ITAException
            {
                Message = ex.Message,
                Code = "UNEXPECTED_ERROR"
            });
        }
    }
}