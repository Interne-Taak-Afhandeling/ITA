using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.KnownLogboekValues;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Services.Models;

namespace InterneTaakAfhandeling.Web.Server.Services;

public interface ILogboekService
{
    Task<List<Activiteit>> GetLogboek(Guid internetaakId);
    Task LogContactRequestAction(KnownContactAction knownContactAction, Guid internetaakId, Guid objectId);
}

public class LogboekService(IObjectApiClient objectenApiClient, IOpenKlantApiClient openKlantApiClient)
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

            activiteiten.Add(activiteit);
        }

        return activiteiten;
    }

    public async Task LogContactRequestAction(KnownContactAction knownContactAction, Guid internetaakId, Guid objectId)
    {
        var logboekData = await GetOrCreateLogboek(internetaakId);

        var logboekAction = BuildLogboekAction(knownContactAction, logboekData, objectId);

        await objectenApiClient.UpdateLogboek(logboekAction, logboekData.Uuid);
    }

    #region Util

    private async Task<ObjectResult<LogboekData>> GetOrCreateLogboek(Guid internetaakId)
    {
        return await _objectenApiClient.GetLogboek(internetaakId)
               ?? await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);
    }

    private ObjectPatchModel<LogboekData> BuildLogboekAction(KnownContactAction knownContactAction,
        ObjectResult<LogboekData> logboek, Guid objectId)
    {
        var logBoekPatch = new ObjectPatchModel<LogboekData>
            { Record = logboek.Record, Type = logboek.Type, Uuid = logboek.Uuid };

        logBoekPatch.Record.Data.Activiteiten.Add(new ActiviteitData
        {
            Datum = DateTime.Now,
            Type = knownContactAction.Type,
            Omschrijving = knownContactAction.Description,
            HeeftBetrekkingOp =
            [
                new ObjectIdentificator
                {
                    CodeRegister = knownContactAction.CodeRegister,
                    CodeObjecttype = knownContactAction.CodeObjectType,
                    CodeSoortObjectId = knownContactAction.CodeSoortObjectId,
                    ObjectId = objectId.ToString()
                }
            ]
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
}