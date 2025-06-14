﻿using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Exceptions;
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
        private readonly IOpenKlantApiClient openKlantApiClient = openKlantApiClient;

        [ProducesResponseType(typeof(RelatedKlantcontactResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ITAException), StatusCodes.Status409Conflict)]
        [HttpPost()]
        public async Task<IActionResult> CreateRelatedKlantcontact([FromBody] RequestModel request)
        {
            try
            {
                _logger.LogInformation("Closing Interne taak and creating related klantcontact with aanleidinggevendKlantcontact UUID: {AanleidinggevendKlantcontactUuid}", request.AanleidinggevendKlantcontactUuid);

                //check if the internetaak that will be closed and the klantcontact to which the new klantcontact will be related belong to eachother
                var interneTaak = await openKlantApiClient.GetInternetaakByIdAsync(request.InterneTaakId.ToString());

                if(interneTaak?.Uuid != request.InterneTaakId.ToString())
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Ongeldige aanvraag",
                        Detail = "Internetaak en klantcontact zijn niet gerelateerd",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var result = await _createKlantContactService.CreateRelatedKlantcontactAsync(
                    request.KlantcontactRequest,
                    request.AanleidinggevendKlantcontactUuid,
                    _user.Email,
                    _user.Name,
                    request.PartijUuid
                );

                //note: the 'afgehandeldOp' datetime is set automatically by OpenKlant
                //that behaviour is not documented in the API specs
                //the field is marked 'experimental' 
                var internetakenUpdateRequest = new InternetakenPatchRequest
                {
                    Status = "verwerkt",
                };

                await openKlantApiClient.PatchInternetaakAsync(internetakenUpdateRequest, request.InterneTaakId.ToString());

                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (ConflictException ex)
            {
                _logger.LogError(ex, "Conflict error creating related klantcontact and closing internetaak {interneTaakId}, {ex.Message}", request.InterneTaakId, ex.Message);
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = ex.Code ?? ""
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating related klantcontactand and closing internetaak {interneTaakId}, {ex.Message}", request.InterneTaakId, ex.Message);
                return StatusCode(409, new ITAException
                {
                    Message = ex.Message,
                    Code = "UNEXPECTED_ERROR"
                });
            }
        }
    }


}