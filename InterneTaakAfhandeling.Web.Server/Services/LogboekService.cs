using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;

namespace InterneTaakAfhandeling.Web.Server.Services;

public interface ILogboekService
{
    Task<LogboekData> AddContactmoment(Guid internetaakId, string klantcontactId, bool? isContactGelukt);
    Task<List<Activiteit>> GetLogboek(Guid internetaakId);
}

public class LogboekService(IObjectApiClient objectenApiClient, IOpenKlantApiClient openKlantApiClient)
    : ILogboekService
{
    private readonly IObjectApiClient _objectenApiClient = objectenApiClient;
    private readonly IOpenKlantApiClient _openKlantApiClient = openKlantApiClient;


    public async Task<LogboekData> AddContactmoment(Guid internetaakId, string klancontactId, bool? isContactGelukt)
    {
        //1 check if a logboek for the Intenretaak already exists
        var logboek = await _objectenApiClient.GetLogboek(internetaakId);

        //2 if not create it
        logboek ??= await _objectenApiClient.CreateLogboekForInternetaak(internetaakId);

        //3 add the contactmoment as an activity to the log

        var logBoekPatch = new ObjectPatchModel<LogboekData>
            { Record = logboek.Record, Type = logboek.Type, Uuid = logboek.Uuid };

        logBoekPatch.Record.Data.Activiteiten.Add(new ActiviteitData
        {
            Datum = DateTime.Now,
            Type = KnownLogboekActiviteitTypes.Klantcontact,
            Omschrijving = isContactGelukt.HasValue && isContactGelukt.Value
                ? "contact gehad"
                : "geen contact kunnen leggen",
            HeeftBetrekkingOp =
            [
                new ObjectIdentificator
                {
                    ObjectId = klancontactId,
                    CodeRegister = LogboekActiviteitContactmomentObjectIdentificators.CodeRegister,
                    CodeObjecttype = LogboekActiviteitContactmomentObjectIdentificators.CodeObjectType,
                    CodeSoortObjectId = LogboekActiviteitContactmomentObjectIdentificators.CodeSoortObjectId
                }
            ]
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
    public required DateTimeOffset Datum { get; set; }
    public required string Type { get; set; }

    public string? Kanaal { get; set; }
    public string? Tekst { get; set; }
    public bool? ContactGelukt { get; set; }
    public string? Id { get; set; }
    public string? Medewerker { get; set; }
}