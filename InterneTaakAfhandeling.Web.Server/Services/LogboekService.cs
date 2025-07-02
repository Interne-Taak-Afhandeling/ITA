using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.KnownLogboekValues;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;
using InterneTaakAfhandeling.Web.Server.Services.Models;

namespace InterneTaakAfhandeling.Web.Server.Services;

public interface ILogboekService
{
    Task<List<Activiteit>> GetLogboek(Guid internetaakId);
    Task LogContactRequestAction(KnownContactAction knownContactAction, Guid internetaakId);
}

public class LogboekService(IObjectApiClient objectenApiClient, IOpenKlantApiClient openKlantApiClient, IZakenApiClient zakenApiClient, ILogger<LogboekService> logger)
    : ILogboekService
{
    private readonly IObjectApiClient _objectenApiClient = objectenApiClient;
    private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
    private readonly IZakenApiClient _zakenApiClient = zakenApiClient;
    private readonly ILogger<LogboekService> _logger = logger;

    public async Task<List<Activiteit>> GetLogboek(Guid internetaakId)
    {
        var logboek = await _objectenApiClient.GetLogboek(internetaakId);

        if (logboek == null) return [];

        var activiteiten = new List<Activiteit>();

        //newest on top
        var activiteitenOrderedByDate = logboek.Record.Data.Activiteiten.OrderByDescending(x => x.Datum);

        foreach (var item in activiteitenOrderedByDate)
        {
            var activiteit = new Activiteit { Datum = item.Datum, Type = item.Type };

            if (item.Type == ActiviteitTypes.Klantcontact && item.HeeftBetrekkingOp.Count == 1)
            {
                var contactmoment =
                    await _openKlantApiClient.GetKlantcontactAsync(item.HeeftBetrekkingOp.Single().ObjectId);
                if (contactmoment != null)
                {
                    activiteit.Id = contactmoment.Uuid;
                    activiteit.Kanaal = contactmoment.Kanaal ?? "Onbekend";
                    activiteit.Tekst = contactmoment.Inhoud;
                    activiteit.ContactGelukt = contactmoment.IndicatieContactGelukt;
                    activiteit.Medewerker = contactmoment.HadBetrokkenActoren?.FirstOrDefault()?.Naam ?? "Onbekend";
                }
            }
            else if (item.Type == ActiviteitTypes.Toegewezen && item.HeeftBetrekkingOp.Count == 1)
            {
                var actorId = item.HeeftBetrekkingOp.Single().ObjectId;
                var actor = await _openKlantApiClient.GetActorAsync(actorId);
                if (actor != null)
                {
                    activiteit.Id = actor.Uuid;
                    activiteit.Medewerker = actor.Naam ?? "Onbekend";
                }
            }
            else if ((item.Type == ActiviteitTypes.ZaakGekoppeld || item.Type == ActiviteitTypes.ZaakkoppelingGewijzigd) && item.HeeftBetrekkingOp.Count >= 1)
            {
                var zaakObject = item.HeeftBetrekkingOp.FirstOrDefault(x => x.CodeObjecttype == "zgw-Zaak");
                var actorObject = item.HeeftBetrekkingOp.FirstOrDefault(x => x.CodeObjecttype == "actor");

                if (zaakObject != null)
                {
                    var zaakId = zaakObject.ObjectId;

                    try
                    {
                        var zaak = await _zakenApiClient.GetZaakAsync(zaakId);
                        if (zaak != null)
                        {
                            activiteit.Id = zaak.Uuid;
                            activiteit.ZaakIdentificatie = zaak.Identificatie;
                        }
                        else
                        {
                            _logger.LogWarning("Zaak with ID {ZaakId} not found in ZakenApi", zaakId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrieving zaak {ZaakId} from ZakenApi", zaakId);
                    }
                }

                // We need actor to show colleague in logboek with zaken
                if (actorObject != null)
                {
                    try
                    {
                        var actor = await _openKlantApiClient.GetActorAsync(actorObject.ObjectId);
                        if (actor != null)
                        {
                            activiteit.Medewerker = actor.Naam ?? "Onbekend";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrieving actor {ActorId} from OpenKlant API", actorObject.ObjectId);
                        activiteit.Medewerker = "Onbekend";
                    }
                }
            }

            activiteiten.Add(activiteit);
        }

        return activiteiten;
    }

    public async Task LogContactRequestAction(KnownContactAction knownContactAction, Guid internetaakId)
    {
        var logboekData = await GetOrCreateLogboek(internetaakId);

        var logboekAction = BuildLogboekAction(knownContactAction, logboekData);

        await _objectenApiClient.UpdateLogboek(logboekAction, logboekData.Uuid);
    }

    #region Util

    private async Task<ObjectResult<LogboekData>> GetOrCreateLogboek(Guid internetaakId)
    {
        return await _objectenApiClient.GetLogboek(internetaakId)
               ?? await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);
    }

    private ObjectPatchModel<LogboekData> BuildLogboekAction(
        KnownContactAction knownContactAction,
        ObjectResult<LogboekData> logboek)
    {
        var logBoekPatch = new ObjectPatchModel<LogboekData>
        { Record = logboek.Record, Type = logboek.Type, Uuid = logboek.Uuid };

        var heeftBetrekkingOp = new List<ObjectIdentificator>();

        if (knownContactAction.HeeftBetrekkingOp != null)
        {
            heeftBetrekkingOp.Add(knownContactAction.HeeftBetrekkingOp);
        }

        heeftBetrekkingOp.AddRange(knownContactAction.AlleObjecten);

        logBoekPatch.Record.Data.Activiteiten.Add(new ActiviteitData
        {
            Datum = DateTime.Now,
            Type = knownContactAction.Type,
            Omschrijving = knownContactAction.Description,
            HeeftBetrekkingOp = heeftBetrekkingOp
        });
        return logBoekPatch;
    }

    #endregion
}

public class Activiteit
{
    public required DateTimeOffset Datum { get; set; }
    public required string Type { get; set; }

    public string? Kanaal { get; set; }
    public string? Tekst { get; set; }
    public bool? ContactGelukt { get; set; }
    public string? Id { get; set; }
    public string? Medewerker { get; set; }
    public string? ZaakIdentificatie { get; set; }
}