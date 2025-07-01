using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Web.Server.Exceptions;
using InterneTaakAfhandeling.Web.Server.Middleware;
using InterneTaakAfhandeling.Web.Server.Services;
using InterneTaakAfhandeling.Web.Server.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.KoppelZaak
{
    [Route("api/internetaken")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class KoppelZaakAanKlantcontactController : Controller
    {
        private readonly IZakenApiClient _zakenApiClient;
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly ILogger<KoppelZaakAanKlantcontactController> _logger;
        private readonly ILogboekService  _logboekService;

        public KoppelZaakAanKlantcontactController(
            IZakenApiClient zakenApiClient,
            IOpenKlantApiClient openKlantApiClient,
            ILogger<KoppelZaakAanKlantcontactController> logger,
            ILogboekService logboekService)
        {
            _zakenApiClient = zakenApiClient ?? throw new ArgumentNullException(nameof(zakenApiClient));
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logboekService = logboekService ?? throw new ArgumentNullException(nameof(logboekService));
        }

        [HttpPost("koppel-zaak")]
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

                var safeZaakId = SecureLogging.SanitizeAndTruncate(request.ZaakIdentificatie, 50);
                var safeKlantcontactUuid = SecureLogging.SanitizeUuid(request.AanleidinggevendKlantcontactUuid);

                _logger.LogInformation("Zoeken naar zaak met identificatie: {SafeZaakId}", safeZaakId);
                var zaak = await _zakenApiClient.GetZaakByIdentificatieAsync(request.ZaakIdentificatie);

                if (zaak == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Zaak niet gevonden",
                        Detail = $"Geen zaak gevonden met identificatie: {safeZaakId}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                _logger.LogInformation("Ophalen aanleidinggevend klantcontact met UUID: {SafeKlantcontactUuid}", safeKlantcontactUuid);
                var aanleidinggevendKlantcontact = await _openKlantApiClient.GetKlantcontactAsync(request.AanleidinggevendKlantcontactUuid);

                if (aanleidinggevendKlantcontact == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Aanleidinggevend klantcontact niet gevonden",
                        Detail = $"Geen klantcontact gevonden met UUID: {safeKlantcontactUuid}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                var onderwerpobject = await KoppelZaakAanOnderwerpobject(aanleidinggevendKlantcontact, zaak.Uuid,request.InternetaakId);

                return Ok(new KoppelZaakAanKlantcontactResult
                {
                    Zaak = zaak,
                    Klantcontact = aanleidinggevendKlantcontact,
                    Onderwerpobject = onderwerpobject
                });
            }
            catch (Exception ex)
            {
                var safeZaakId = SecureLogging.SanitizeAndTruncate(request.ZaakIdentificatie, 50);
                var safeKlantcontactUuid = SecureLogging.SanitizeUuid(request.AanleidinggevendKlantcontactUuid);

                _logger.LogError(ex, "Fout bij koppelen van zaak {SafeZaakId} aan aanleidinggevend klantcontact {SafeKlantcontactUuid}",
                    safeZaakId, safeKlantcontactUuid);

                throw new ConflictException(
                    $"Er is een fout opgetreden bij het koppelen van de zaak: {ex.Message}",
                    "ZAAK_KOPPELEN_GEFAALD");
            }
        }

        private async Task<Onderwerpobject> KoppelZaakAanOnderwerpobject(Klantcontact klantcontact, string zaakUuid,
            string requestInternetaakId)
        {
            var safeKlantcontactUuid = SecureLogging.SanitizeUuid(klantcontact.Uuid);
            _logger.LogInformation("Koppelen zaak aan klantcontact met UUID: {SafeKlantcontactUuid}", safeKlantcontactUuid);

            Onderwerpobject? bestaandZaakOnderwerpobject = null;
            int zaakOnderwerpobjectCount = 0;

            if (klantcontact.GingOverOnderwerpobjecten?.Count > 0)
            {
                foreach (var onderwerpobjectRef in klantcontact.GingOverOnderwerpobjecten)
                {
                    if (string.IsNullOrEmpty(onderwerpobjectRef?.Uuid)) continue;

                    var onderwerpobject = await _openKlantApiClient.GetOnderwerpobjectAsync(onderwerpobjectRef.Uuid);

                    if (onderwerpobject?.Onderwerpobjectidentificator != null &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeObjecttype == "zgw-Zaak" &&
                        onderwerpobject.Onderwerpobjectidentificator.CodeRegister == "openzaak")
                    {
                        var safeOnderwerpUuid = SecureLogging.SanitizeUuid(onderwerpobject.Uuid);
                        _logger.LogInformation("Zaak-onderwerpobject gevonden: {SafeOnderwerpUuid}", safeOnderwerpUuid);
                        zaakOnderwerpobjectCount++;

                        bestaandZaakOnderwerpobject ??= onderwerpobject;
                    }
                }
            }

            // Check of er meerdere zaak-gerelateerde onderwerpobjecten zijn
            if (zaakOnderwerpobjectCount > 1)
            {
                _logger.LogWarning("Klantcontact {SafeKlantcontactUuid} heeft {ZaakOnderwerpobjectCount} zaak-gerelateerde onderwerpobjecten. Het koppelen van een nieuwe zaak wordt niet ondersteund in deze situatie.",
                    safeKlantcontactUuid, zaakOnderwerpobjectCount);
                throw new ConflictException(
                    "Het koppelen van een nieuwe zaak wordt niet ondersteund omdat er al meerdere zaken gekoppeld zijn aan dit contactverzoek.",
                    "MEERDERE_ZAKEN_GEKOPPELD");
            }

            var request = new KlantcontactOnderwerpobjectRequest
            {
                Klantcontact = new KlantcontactReference { Uuid = klantcontact.Uuid },
                WasKlantcontact = null, // opzettelijk null voor zaak-koppeling
                Onderwerpobjectidentificator = new Onderwerpobjectidentificator
                {
                    ObjectId = zaakUuid,
                    CodeObjecttype = "zgw-Zaak",
                    CodeRegister = "openzaak",
                    CodeSoortObjectId = "uuid"
                }
            };
            var internetaakId = klantcontact?.LeiddeTotInterneTaken?.First(x=> x.Uuid == requestInternetaakId)?.Uuid;

            if (bestaandZaakOnderwerpobject != null && !string.IsNullOrEmpty(bestaandZaakOnderwerpobject.Uuid))
            {
                var safeOnderwerpUuid = SecureLogging.SanitizeUuid(bestaandZaakOnderwerpobject.Uuid);
                var safeZaakUuid = SecureLogging.SanitizeUuid(zaakUuid);
                _logger.LogInformation("Bijwerken bestaand zaak-onderwerpobject {SafeOnderwerpUuid} met nieuwe zaak {SafeZaakUuid}",
                    safeOnderwerpUuid, safeZaakUuid);

                return await _openKlantApiClient.UpdateOnderwerpobjectAsync(bestaandZaakOnderwerpobject.Uuid, request);
            }
            else
            {
                var safeZaakUuid = SecureLogging.SanitizeUuid(zaakUuid);
                var safeKlantUuid = SecureLogging.SanitizeUuid(klantcontact.Uuid);
                _logger.LogInformation("Aanmaken nieuw onderwerpobject voor zaak {SafeZaakUuid} en klantcontact {SafeKlantUuid}",
                    safeZaakUuid, safeKlantUuid);
               
                  var linkedKlantContact = await _openKlantApiClient.CreateOnderwerpobjectAsync(request);

                if (internetaakId != null)
                      await _logboekService.LogContactRequestAction(KnownContactAction.CaseLinked(Guid.Parse(zaakUuid)),
                          Guid.Parse(internetaakId));

                  return linkedKlantContact;
            }
        }
    }

    public class KoppelZaakAanKlantcontactRequest
    {
        public required string ZaakIdentificatie { get; set; }
        public required string AanleidinggevendKlantcontactUuid { get; set; }
        
        public required string InternetaakId  { get; set; }
    }

    public class KoppelZaakAanKlantcontactResult
    {
        public required Common.Services.ZakenApi.Models.Zaak Zaak { get; set; }
        public required Klantcontact Klantcontact { get; set; }
        public required Onderwerpobject Onderwerpobject { get; set; }
    }
}