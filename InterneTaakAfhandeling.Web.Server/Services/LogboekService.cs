using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Web.Server.Services;

public interface ILogboekService
{
    Task<LogboekData> AddContactmoment(Guid internetaakId,string type, string description);
    Task<List<Activiteit>> GetLogboek(Guid internetaakId);

 
}

public class LogboekService(IObjectApiClient objectenApiClient, IOpenKlantApiClient openKlantApiClient, IOptions<LogboekOptions> logboekOptions)
    : ILogboekService
{
    private readonly IObjectApiClient _objectenApiClient = objectenApiClient;
    private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;
    private readonly IOptions<LogboekOptions> _logboekOptions = logboekOptions;

    public async Task<LogboekData> AddContactmoment(Guid internetaakId, string type, string description)
    { 
        //1 check if a logboek for the Intenretaak already exists
        var logboekData = await _objectenApiClient.GetLogboek(internetaakId);
        if (logboekData == null)
        {
            //2 if not create it
            logboekData= await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);

        }
        var activity = BuildActivity(logboekData, internetaakId.ToString(), type, description);
        return await objectenApiClient.UpdateLogboek(activity, logboekData.Uuid);
        
    }
    
    public ObjectResult<LogboekData> BuildActivity(ObjectResult<LogboekData> logboekData, string internetaakId,
        string type, string description)
    {
        logboekData.Record.Data.Activiteiten.Add(new ActiviteitData
        {
            Datum = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Type = type,
            Omschrijving = description,
            HeeftBetrekkingOp =
            [
                new ObjectIdentificator(internetaakId, LogboekObjectIdentificators.CodeRegister,
                    LogboekObjectIdentificators.CodeObjectType,
                    LogboekObjectIdentificators.CodeSoortObjectId)
            ]
        });
        return logboekData;
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