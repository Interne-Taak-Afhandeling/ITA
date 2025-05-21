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
    public class KoppelZaakController : Controller
    {
        private readonly IZakenApiClient _zakenApiClient;
        private readonly IOpenKlantApiClient _openKlantApiClient;
        private readonly IKlantcontactService _klantcontactService;
        private readonly ILogger<KoppelZaakController> _logger;

        public KoppelZaakController(
            IZakenApiClient zakenApiClient,
            IOpenKlantApiClient openKlantApiClient,
            IKlantcontactService klantcontactService,
            ILogger<KoppelZaakController> logger)
        {
            _zakenApiClient = zakenApiClient ?? throw new ArgumentNullException(nameof(zakenApiClient));
            _openKlantApiClient = openKlantApiClient ?? throw new ArgumentNullException(nameof(openKlantApiClient));
            _klantcontactService = klantcontactService ?? throw new ArgumentNullException(nameof(klantcontactService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("koppel-zaak-aan-klantcontact")]
        [ProducesResponseType(typeof(KoppelZaakResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [AllowAnonymous]
        public async Task<IActionResult> KoppelZaakAanKlantcontact([FromBody] KoppelZaakRequest request)
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

                if (string.IsNullOrWhiteSpace(request.KlantcontactUuid))
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Ongeldige aanvraag",
                        Detail = "Klantcontact UUID is vereist",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Zoek de zaak in OpenZaak
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

                // Vind het eerste (oorspronkelijke) klantcontact in de keten
                var eersteKlantcontact = await _klantcontactService.GetEersteKlantcontactInKetenAsync(request.KlantcontactUuid);

                if (eersteKlantcontact == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Klantcontact niet gevonden",
                        Detail = $"Geen klantcontact gevonden in de keten met UUID: {request.KlantcontactUuid}",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Maak of update het onderwerpobject
                var onderwerpobject = await KoppelZaakAanOnderwerpobject(eersteKlantcontact.Uuid, zaak.Uuid);

                return Ok(new KoppelZaakResult
                {
                    Zaak = zaak,
                    Klantcontact = eersteKlantcontact,
                    Onderwerpobject = onderwerpobject
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fout bij koppelen van zaak {request.ZaakIdentificatie} aan klantcontact {request.KlantcontactUuid}");

                throw new ConflictException(
                    $"Er is een fout opgetreden bij het koppelen van de zaak: {ex.Message}",
                    "ZAAK_KOPPELEN_GEFAALD");
            }
        }

        private async Task<Onderwerpobject> KoppelZaakAanOnderwerpobject(string klantcontactUuid, string zaakUuid)
        {
            // Zoek bestaande onderwerpobjecten voor dit klantcontact
            var bestaandeOnderwerpobjecten = await _openKlantApiClient.GetOnderwerpobjectenByKlantcontactAsync(klantcontactUuid);

            // Controleer of er al een onderwerpobject is voor een zaak
            var bestaandZaakOnderwerpobject = bestaandeOnderwerpobjecten
                .FirstOrDefault(o => o.Onderwerpobjectidentificator.CodeObjecttype == "zaak" &&
                                    o.Onderwerpobjectidentificator.CodeRegister == "openzaak");

            var nieuwOnderwerpobject = new Onderwerpobject
            {
                Uuid = string.Empty,  // Dit zal later ingevuld worden door de API bij aanmaken
                Url = string.Empty,   // Dit zal later ingevuld worden door de API bij aanmaken

                Klantcontact = new Klantcontact
                {
                    Uuid = klantcontactUuid,
                    Url = $"/klantinteracties/api/v1/klantcontacten/{klantcontactUuid}"
                },
                // We gebruiken hetzelfde klantcontact voor wasKlantcontact omdat we het eerste in de keten koppelen
                WasKlantcontact = new Klantcontact
                {
                    Uuid = klantcontactUuid,
                    Url = $"/klantinteracties/api/v1/klantcontacten/{klantcontactUuid}"
                },
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
                // Update het bestaande onderwerpobject
                _logger.LogInformation($"Bijwerken bestaand onderwerpobject {bestaandZaakOnderwerpobject.Uuid} met nieuwe zaak {zaakUuid}");
                nieuwOnderwerpobject.Uuid = bestaandZaakOnderwerpobject.Uuid;
                nieuwOnderwerpobject.Url = bestaandZaakOnderwerpobject.Url;
                return await _openKlantApiClient.UpdateOnderwerpobjectAsync(bestaandZaakOnderwerpobject.Uuid, nieuwOnderwerpobject);
            }
            else
            {
                // Maak een nieuw onderwerpobject
                _logger.LogInformation($"Aanmaken nieuw onderwerpobject voor zaak {zaakUuid} en klantcontact {klantcontactUuid}");
                return await _openKlantApiClient.CreateOnderwerpobjectAsync(nieuwOnderwerpobject);
            }
        }
    }

    public class KoppelZaakRequest
    {
        public required string ZaakIdentificatie { get; set; }
        public required string KlantcontactUuid { get; set; }
    }

    public class KoppelZaakResult
    {
        public InterneTaakAfhandeling.Common.Services.ZakenApi.Models.Zaak Zaak { get; set; } = null!;
        public Klantcontact Klantcontact { get; set; } = null!;
        public Onderwerpobject Onderwerpobject { get; set; } = null!;
    }
}