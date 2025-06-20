using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;

namespace InterneTaakAfhandeling.Web.Server.Services;

public interface ILogboekService
{
    Task<ObjectResult<LogboekData>> AddContactmoment(Guid internetaakId);
    Task<List<Activiteit>> GetLogboek(Guid internetaakId);

    Task<LogboekData> LogActivity(ObjectResult<LogboekData> logboekData, string interneTaakId,
        string type,
        string description);
}

public class LogboekService(IObjectApiClient objectenApiClient, IOpenKlantApiClient openKlantApiClient)
    : ILogboekService
{
    private readonly IObjectApiClient _objectenApiClient = objectenApiClient;
    private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;

    public async Task<ObjectResult<LogboekData>> AddContactmoment(Guid internetaakId)
    {
        //1 check if a logboek for the Intenretaak already exists
        var exisistingLogboek = await _objectenApiClient.GetLogboek(internetaakId);


        if (exisistingLogboek != null) return exisistingLogboek;

        //2 if not create it
        return await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);


        //3 add an antry to the logboek with information about this contactmoment
    }


    public async Task<List<Activiteit>> GetLogboek(Guid internetaakId)
    {
        var logboek = await _objectenApiClient.GetLogboek(internetaakId);

        if (logboek == null) return [];

        var activiteiten = new List<Activiteit>();

        foreach (var item in logboek.Record.Data.Activiteiten)
        {
            var activiteit = new Activiteit { Datum = item.Datum, Type = item.Type };

            if (item.Type == KnownLogboekActiviteitTypes.Klantcontact && item.HeeftBetrekkingOp != null)
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

            activiteiten.Add(activiteit);
        }

        return activiteiten;
    }

    public Task<LogboekData> LogActivity(ObjectResult<LogboekData> logboekData, string interneTaakId,
        string type,
        string description)
    {
        var activity = objectenApiClient.BuildActivity(logboekData, interneTaakId, type, description);
        return objectenApiClient.UpdateLogboek(activity, logboekData.Uuid);
    }
}

public class Activiteit
{
    public required string Datum { get; set; }
    public required string Type { get; set; }

    public string? Kanaal { get; set; }
    public string? Tekst { get; set; }
    public bool? ContactGelukt { get; set; }
    public string? Id { get; set; }
    public string? Medewerker { get; set; }
}