using InterneTaakAfhandeling.Common.Services;
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

                case ActiviteitTypes.Doorsturen:
                    {
                        activiteit.Tekst = await CreateDoorsturenText(item);

                        activiteit.UitgevoerdDoor = GetName(item);

                        if (!string.IsNullOrEmpty(item.Notitie))
                        {
                            activiteit.Notitie = item.Notitie;
                        }
                        break;
                    }
            }

            activiteiten.Add(activiteit);
        }

        return activiteiten;
    }

    private async Task<string> CreateDoorsturenText(ActiviteitData item)
    {
        if (item.HeeftBetrekkingOp == null || item.HeeftBetrekkingOp.Count == 0)
        {
            throw new ArgumentException("Geen objectreferenties gevonden in activiteit.");
        }

        var descriptions = await Task.WhenAll(item.HeeftBetrekkingOp.Select(GetActorDescription));
        return $"Contactverzoek doorgestuurd aan {string.Join(" en ", descriptions)}";
    }

    private async Task<string> GetActorDescription(ObjectIdentificator objectIdentificator)
    {
        return objectIdentificator.CodeObjecttype switch
        {
            KnownAfdelingIdentificators.CodeObjecttypeAfdeling => await GetAfdelingDescription(objectIdentificator.ObjectId),
            KnownGroepIdentificators.CodeObjecttypeGroep => await GetGroepDescription(objectIdentificator.ObjectId),
            KnownMedewerkerIdentificators.CodeObjecttypeMedewerker => $"medewerker \"{objectIdentificator.ObjectId}\"",
            _ => throw new InvalidOperationException($"Onbekend objecttype: {objectIdentificator.CodeObjecttype}")
        };
    }

    private async Task<string> GetAfdelingDescription(string objectId)
    {
        var afdelingen = await _objectenApiClient.GetAfdelingenByIdentificatie(objectId);
        var afdeling = afdelingen.FirstOrDefault();
        return $"{KnownActorType.Afdeling.ToLower()} \"{afdeling?.Naam ?? "onbekend"}\"";
    }

    private async Task<string> GetGroepDescription(string objectId)
    {
        var groepen = await _objectenApiClient.GetGroepenByIdentificatie(objectId);
        var groep = groepen.FirstOrDefault();
        return $"{KnownActorType.Groep.ToLower()} \"{groep?.Naam ?? "onbekend"}\"";
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
        ActiviteitTypes.Doorsturen => "Doorgestuurd",

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
            HeeftBetrekkingOp = knownContactAction.HeeftBetrekkingOp ?? []
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