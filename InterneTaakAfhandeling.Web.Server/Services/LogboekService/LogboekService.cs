using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.KnownLogboekValues;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.ZakenApi;

namespace InterneTaakAfhandeling.Web.Server.Services.LogboekService;

public interface ILogboekService
{
    Task<List<Activiteit>> GetLogboek(Guid internetaakId);
    Task LogContactRequestAction(KnownContactAction knownContactAction, Guid internetaakId);
}

public class LogboekService(IObjectApiClient objectenApiClient, IOpenKlantApiClient openKlantApiClient, IZakenApiClient zakenApiClient)
    : ILogboekService
{
    private readonly IObjectApiClient _objectenApiClient = objectenApiClient;
    private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;

    public async Task<List<Activiteit>> GetLogboek(Guid internetaakId)
    {
        var logboek = await _objectenApiClient.GetLogboek(internetaakId);

        if (logboek == null) return [];

        var activiteiten = new List<Activiteit>();

        //newest on top
        var activiteitenOrderedByDate = logboek.Record.Data.Activiteiten.OrderByDescending(x => x.Datum);

        foreach (var item in activiteitenOrderedByDate)
        {
            var activiteit = new Activiteit
            {
                Datum = item.Datum,
                Type = item.Type,
                Titel = GetActionTitle(item.Type)
            };

            switch (item.Type)
            {
                case ActiviteitTypes.Klantcontact when item.HeeftBetrekkingOp.Count == 1:
                    {
                        var contactmoment =
                            await _openKlantApiClient.GetKlantcontactAsync(item.HeeftBetrekkingOp.Single().ObjectId);
                        if (contactmoment != null)
                        {
                            activiteit.Id = contactmoment.Uuid;
                            activiteit.Kanaal = contactmoment.Kanaal ?? "Onbekend";
                            activiteit.Tekst = contactmoment.Inhoud;
                            activiteit.ContactGelukt = contactmoment.IndicatieContactGelukt;
                            activiteit.UitgevoerdDoor = GetName( item);
                            activiteit.Notitie = item.Notitie;
 
                            activiteit.Titel = contactmoment.IndicatieContactGelukt.HasValue && contactmoment.IndicatieContactGelukt.Value
                                ? "Contact gelukt"
                                : "Contact niet gelukt";
                        }

                        break;
                    }
                case ActiviteitTypes.Toegewezen when item.HeeftBetrekkingOp.Count == 1:
                    {
                        var actorId = item.HeeftBetrekkingOp.Single().ObjectId;
                        var actor = await _openKlantApiClient.GetActorAsync(actorId);
                        if (actor != null)
                        {
                            activiteit.UitgevoerdDoor = GetName(item);
                            activiteit.Tekst = $"Contactverzoek opgepakt door {actor.Naam ?? "Onbekend"}";
                        }

                        break;
                    }
                case ActiviteitTypes.ZaakGekoppeld:
                    {
                        var zaak = await zakenApiClient.GetZaakAsync(item.HeeftBetrekkingOp.Single().ObjectId);
                        if (zaak != null)
                        {
                            activiteit.UitgevoerdDoor = GetName(item);
                            activiteit.Tekst = $"Zaak {zaak.Identificatie} gekoppeld aan het contactverzoek";
                        }

                        break;
                    }
                case ActiviteitTypes.ZaakkoppelingGewijzigd:
                    {
                        var zaak = await zakenApiClient.GetZaakAsync(item.HeeftBetrekkingOp.Single().ObjectId);
                        if (zaak != null)
                        {
                            activiteit.UitgevoerdDoor = GetName(item);
                            activiteit.Tekst = $"Zaak {zaak.Identificatie} gekoppeld aan het contactverzoek";
                        }

                        break;
                    }
                case ActiviteitTypes.Verwerkt:
                    {

                        activiteit.UitgevoerdDoor = GetName(item);
                        activiteit.Tekst = $"Contactverzoek afgerond";

                        break;
                    }
                case ActiviteitTypes.InterneNotitie:
                    {
                        activiteit.UitgevoerdDoor = GetName(item);
                        activiteit.Notitie = item.Notitie;
                        break;
                    }
                        // TODO -> medewerker email + afdeling or group
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

    private static string GetActionTitle(string type) => type switch
    {
        ActiviteitTypes.Klantcontact => "Klantcontact",
        ActiviteitTypes.Verwerkt => "Afgerond",
        ActiviteitTypes.ZaakGekoppeld => "Zaak gekoppeld",
        ActiviteitTypes.ZaakkoppelingGewijzigd => "Zaak gewijzigd",
        ActiviteitTypes.Toegewezen => "Opgepakt",
        ActiviteitTypes.InterneNotitie => "Interne notitie",
        _ => type ?? "Onbekende actie"
    };

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

        logBoekPatch.Record.Data.Activiteiten.Add(new ActiviteitData
        {
            Datum = DateTime.Now,
            Type = knownContactAction.Type,
            Notitie = knownContactAction.Notitie ?? string.Empty,
            Omschrijving = knownContactAction.Description,
            Actor = knownContactAction.Actor,
            HeeftBetrekkingOp = knownContactAction.HeeftBetrekkingOp != null
                ?
                [
                    knownContactAction.HeeftBetrekkingOp
                ]
                : []
        });
        return logBoekPatch;
    }


    private static string GetName(ActiviteitData item) => item.Actor?.Naam ?? "Onbekend";


    #endregion
}

public class Activiteit
{
    public required DateTimeOffset Datum { get; set; }
    public required string Type { get; set; }
    public required string Titel { get; set; }

    public string? Kanaal { get; set; }
    public string? Tekst { get; set; }
    public bool? ContactGelukt { get; set; }
    public string? Id { get; set; }
    public string? UitgevoerdDoor { get; set; }
    public string? Notitie { get; set; }
}