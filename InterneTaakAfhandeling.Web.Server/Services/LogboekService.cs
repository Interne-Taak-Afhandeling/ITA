using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.KnownLogboekValues;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;

namespace InterneTaakAfhandeling.Web.Server.Services;

public interface ILogboekService
{
    Task<LogboekData> AddContactmoment(Guid internetaakId, string klantcontactId, bool? isContactGelukt, string? interneNotitie = null);
    Task<LogboekData> AddInterneNotitie(Guid internetaakId, string interneNotitie);
    Task<List<Activiteit>> GetLogboek(Guid internetaakId);
}

public class LogboekService(IObjectApiClient objectenApiClient, IOpenKlantApiClient openKlantApiClient)
    : ILogboekService
{
    private readonly IObjectApiClient _objectenApiClient = objectenApiClient;
    private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;


    public async Task<LogboekData> AddContactmoment(Guid internetaakId, string klantcontactId, bool? isContactGelukt, string? interneNotitie = null)
    {
        //1 check if a logboek for the Internetaak already exists
        var logboek = await _objectenApiClient.GetLogboek(internetaakId);

        //2 if not create it
        logboek ??= await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);

        //3 add the contactmoment as an activity to the log

        var logBoekPatch = new ObjectPatchModel<LogboekData>
        { Record = logboek.Record, Type = logboek.Type, Uuid = logboek.Uuid };

        var activiteit = new ActiviteitData
        {
            Datum = DateTime.Now,
            Type = ActiviteitTypes.Klantcontact,
            Omschrijving = isContactGelukt.HasValue && isContactGelukt.Value
                ? "contact gehad"
                : "geen contact kunnen leggen",
            HeeftBetrekkingOp =
            [
                new ObjectIdentificator
                {
                    ObjectId = klantcontactId,
                    CodeRegister = ActiviteitContactmomentObjectIdentificator.CodeRegister,
                    CodeObjecttype = ActiviteitContactmomentObjectIdentificator.CodeObjectType,
                    CodeSoortObjectId = ActiviteitContactmomentObjectIdentificator.CodeSoortObjectId
                }
            ]
        };

        // Voeg interne notitie toe als deze bestaat
        if (!string.IsNullOrWhiteSpace(interneNotitie))
        {
            activiteit.Notitie = interneNotitie;
        }

        logBoekPatch.Record.Data.Activiteiten.Add(activiteit);

        return await _objectenApiClient.UpdateLogboek(logBoekPatch, logboek.Uuid);
    }

    public async Task<LogboekData> AddInterneNotitie(Guid internetaakId, string interneNotitie)
    {
        //1 check if a logboek for the Internetaak already exists
        var logboek = await _objectenApiClient.GetLogboek(internetaakId);

        //2 if not create it
        logboek ??= await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);

        //3 add the internal note as an activity to the log
        var logBoekPatch = new ObjectPatchModel<LogboekData>
        { Record = logboek.Record, Type = logboek.Type, Uuid = logboek.Uuid };

        logBoekPatch.Record.Data.Activiteiten.Add(new ActiviteitData
        {
            Datum = DateTime.Now,
            Type = ActiviteitTypes.InterneNotitie, 
            Omschrijving = "interne notitie toegevoegd",
            Notitie = interneNotitie,
            HeeftBetrekkingOp = [] 
        });

        return await _objectenApiClient.UpdateLogboek(logBoekPatch, logboek.Uuid);
    }

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
                InterneNotitie = item.Notitie
            };

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
            //todo ffnakijken hier
            else if (item.Type == ActiviteitTypes.InterneNotitie)
            {
                activiteit.Id = Guid.NewGuid().ToString(); 
                activiteit.Kanaal = "Intern";
                activiteit.Tekst = item.Notitie;
                activiteit.ContactGelukt = null; 
                activiteit.Medewerker = "Systeem";
            }

            activiteiten.Add(activiteit);
        }

        return activiteiten;
    }
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
    public string? InterneNotitie { get; set; }
}