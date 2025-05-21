using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.KoppelZaak
{
    [Route("api/koppelzaak")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class KoppelZaakAanKlantcontactController : Controller
    {
        private readonly IZakenApiClient _zakenApiClient;
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly IKlantcontactService _klantcontactService;
        private readonly ILogger<KoppelZaakAanKlantcontactController> _logger;

        public KoppelZaakAanKlantcontactController(
            IZakenApiClient zakenApiClient,
            IOpenKlantApiClient openKlantApiClient,
            IKlantcontactService klantcontactService,
            ILogger<KoppelZaakAanKlantcontactController> logger)
        {
            _zakenApiClient = zakenApiClient ?? throw new ArgumentNullException(nameof(zakenApiClient));
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("koppel-zaak-aan-klantcontact")]
        [ProducesResponseType(typeof(KoppelZaakAanKlantcontactResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [AllowAnonymous]
        public async Task<IActionResult> KoppelZaakAanKlantcontact([FromBody] KoppelZaakAanKlantcontactRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ZaakIdentificatie))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Ongeldige aanvraag",
                        Detail = "Zaaknummer (identificatie) is vereist",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                if (string.IsNullOrWhiteSpace(request.AanleidinggevendKlantcontactUuid))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Ongeldige aanvraag",
                        Detail = "Aanleidinggevend klantcontact UUID is vereist",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation($"Zoeken naar zaak met identificatie: {request.ZaakIdentificatie}");
                var zaak = await _zakenApiClient.GetZaakByIdentificatieAsync(request.ZaakIdentificatie);

                if (zaak == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Zaak niet gevonden",
                        Detail = $"Geen zaak gevonden met identificatie: {request.ZaakIdentificatie}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation($"Ophalen aanleidinggevend klantcontact met UUID: {request.AanleidinggevendKlantcontactUuid}");
                var aanleidinggevendKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(request.AanleidinggevendKlantcontactUuid);

                if (aanleidinggevendKlantcontact == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Aanleidinggevend klantcontact niet gevonden",
                        Detail = $"Geen klantcontact gevonden met UUID: {request.AanleidinggevendKlantcontactUuid}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var onderwerpobject = await KoppelZaakAanOnderwerpobject(aanleidinggevendKlantcontact.Uuid, zaak.Uuid);

                return Ok(new KoppelZaakAanKlantcontactResult
                {
                    Zaak = zaak,
                    Klantcontact = aanleidinggevendKlantcontact,
                    Onderwerpobject = onderwerpobject
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fout bij koppelen van zaak {request.ZaakIdentificatie} aan aanleidinggevend klantcontact {request.AanleidinggevendKlantcontactUuid}");

                throw new ConflictException(
                    $"Er is een fout opgetreden bij het koppelen van de zaak: {ex.Message}",
                    "ZAAK_KOPPELEN_GEFAALD");
            }
        }
        private async Task<Onderwerpobject> KoppelZaakAanOnderwerpobject(string aanleidinggevendKlantcontactUuid, string zaakUuid)
        {
            _logger.LogInformation($"Ophalen klantcontact met UUID: {aanleidinggevendKlantcontactUuid}");
            var klantcontact = await _openKlantApiClient.GetKlantcontactAsync(aanleidinggevendKlantcontactUuid);

            if (klantcontact == null)
            {
                _logger.LogWarning($"Klantcontact met UUID {aanleidinggevendKlantcontactUuid} niet gevonden");
                throw new ConflictException(
                    $"Klantcontact met UUID {aanleidinggevendKlantcontactUuid} niet gevonden",
                    "KLANTCONTACT_NIET_GEVONDEN");
            }

            Onderwerpobject? bestaandZaakOnderwerpobject = null;
            int zaakOnderwerpobjectCount = 0;

            if (klantcontact.GingOverOnderwerpobjecten != null && klantcontact.GingOverOnderwerpobjecten.Any())
            {
                _logger.LogInformation($"Klantcontact heeft {klantcontact.GingOverOnderwerpobjecten.Count} onderwerpobjecten");

                foreach (var onderwerpobjectRef in klantcontact.GingOverOnderwerpobjecten)
                {
                    var onderwerpobject = await _openKlantApiClient.GetOnderwerpobjectAsync(onderwerpobjectRef.Uuid);

                    if (onderwerpobject != null &&
                        onderwerpobject.Onderwerpobjectidentificator != null &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeObjecttype == "zgw-Zaak" &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeRegister == "openzaak")
                    {
                        _logger.LogInformation($"Zaak-onderwerpobject gevonden: {onderwerpobject.Uuid}");
                        zaakOnderwerpobjectCount++;

                        if (bestaandZaakOnderwerpobject == null)
                        {
                            bestaandZaakOnderwerpobject = onderwerpobject;
                        }
                    }
                }
            }

            // Check of er meerdere zaak-gerelateerde onderwerpobjecten zijn
            if (zaakOnderwerpobjectCount > 1)
            {
                _logger.LogWarning($"Klantcontact {aanleidinggevendKlantcontactUuid} heeft {zaakOnderwerpobjectCount} zaak-gerelateerde onderwerpobjecten. Het koppelen van een nieuwe zaak wordt niet ondersteund in deze situatie.");
                throw new ConflictException(
                    "Het koppelen van een nieuwe zaak wordt niet ondersteund omdat er al meerdere zaken gekoppeld zijn aan dit contact.",
                    "MEERDERE_ZAKEN_GEKOPPELD");
            }

            var onderwerpobjectRequest = new Onderwerpobject
            {
                Klantcontact = new Klantcontact
                {
                    Uuid = aanleidinggevendKlantcontactUuid,
                    Url = klantcontact.Url
                },
                // WasKlantcontact opzettelijk null laten
                WasKlantcontact = null,
                Onderwerpobjectidentificator = new Onderwerpobjectidentificator
                {
                    ObjectId = zaakUuid,
                    CodeObjecttype = "zgw-Zaak",
                    CodeRegister = "openzaak",
                    CodeSoortObjectId = "uuid"
                }
            };

            if (bestaandZaakOnderwerpobject != null)
            {
                _logger.LogInformation($"Bijwerken bestaand zaak-onderwerpobject {bestaandZaakOnderwerpobject.Uuid} met nieuwe zaak {zaakUuid}");

                return await _openKlantApiClient.UpdateOnderwerpobjectAsync(bestaandZaakOnderwerpobject.Uuid, onderwerpobjectRequest);
            }
            else
            {
                _logger.LogInformation($"Aanmaken nieuw onderwerpobject voor zaak {zaakUuid} en klantcontact {aanleidinggevendKlantcontactUuid}");
                var result = await _openKlantApiClient.CreateOnderwerpobjectAsync(onderwerpobjectRequest);

                return result;
            }
        }
    }

    public class KoppelZaakAanKlantcontactRequest
    {
        public required string ZaakIdentificatie { get; set; }
        public required string AanleidinggevendKlantcontactUuid { get; set; }
    }

    public class KoppelZaakAanKlantcontactResult
    {
        public InterneTaakAfhandeling.Common.Services.ZakenApi.Models.Zaak Zaak { get; set; } = null!;
        public Klantcontact Klantcontact { get; set; } = null!;
        public Onderwerpobject Onderwerpobject { get; set; } = null!;
    }
}