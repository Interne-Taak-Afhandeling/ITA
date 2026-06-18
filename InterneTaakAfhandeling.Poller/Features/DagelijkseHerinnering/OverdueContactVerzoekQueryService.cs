using InterneTaakAfhandeling.Common.Services.Afhandeltermijn;
using InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Features.DagelijkseHerinnering;

public class OverdueContactVerzoekQueryService(
    IOpenKlantApiClient openKlantApiClient,
    IAfhandeltermijnProvider afhandeltermijnProvider,
    ILogger<OverdueContactVerzoekQueryService> logger) : IOverdueContactVerzoekQueryService
{
    public async Task<IReadOnlyList<RecipientHerinneringData>> GetOverdueContactVerzoekenAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Dagelijkse herinnering: query gestart");

        var afhandeltermijn = afhandeltermijnProvider.GetAfhandeltermijn();
        var now = DateTimeOffset.UtcNow;
        var allInternetaken = new List<Internetaak>();

        var page = 1;
        while (true)
        {
            var results = await openKlantApiClient.QueryInterneTakenAsync(
                new InterneTaakQuery { Status = KnownInternetaakStatussen.TeVerwerken, Page = page });

            if (results.Count == 0) break;

            allInternetaken.AddRange(results);
            page++;
        }

        logger.LogInformation("Dagelijkse herinnering: {Count} te_verwerken internetaken opgehaald", allInternetaken.Count);

        var overdue = allInternetaken.Where(taak => IsOverdue(taak, afhandeltermijn, now)).ToList();

        logger.LogInformation("Dagelijkse herinnering: {Count} verlopen na datumfilter", overdue.Count);

        var byActor = new Dictionary<string, (Actor Actor, List<Internetaak> Taken)>();

        foreach (var taak in overdue)
        {
            foreach (var actor in taak.ToegewezenAanActoren ?? [])
            {
                if (string.IsNullOrEmpty(actor.Uuid))
                {
                    logger.LogWarning("Actor zonder UUID gevonden voor internetaak {Uuid} — overgeslagen", taak.Uuid);
                    continue;
                }

                if (!byActor.TryGetValue(actor.Uuid, out var entry))
                {
                    entry = (actor, []);
                    byActor[actor.Uuid] = entry;
                }
                entry.Taken.Add(taak);
            }
        }

        logger.LogInformation("Dagelijkse herinnering: {Count} ontvangergroepen na groepering", byActor.Count);

        return byActor.Values.Select(entry => new RecipientHerinneringData
        {
            Ontvanger = entry.Actor,
            VerlopenContactVerzoeken = entry.Taken,
            MaxAantalWerkdagenOpenstaan = entry.Taken.Max(taak =>
                BerekenWerkdagen(taak.AanleidinggevendKlantcontact?.PlaatsgevondenOp ?? now, now))
        }).ToList();
    }

    private static bool IsOverdue(Internetaak taak, TimeSpan afhandeltermijn, DateTimeOffset now)
    {
        var plaatsgevondenOp = taak.AanleidinggevendKlantcontact?.PlaatsgevondenOp;
        return plaatsgevondenOp != null && VoegWerkurenToe(plaatsgevondenOp.Value, afhandeltermijn) <= now;
    }

    // Adds business hours to a starting timestamp, skipping Saturday and Sunday.
    private static DateTimeOffset VoegWerkurenToe(DateTimeOffset start, TimeSpan werkuren)
    {
        var remaining = (int)werkuren.TotalHours;
        var current = start;

        while (remaining > 0)
        {
            current = current.AddHours(1);
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                remaining--;
        }

        return current;
    }

    // Counts business days (Mon–Fri) between two timestamps, exclusive of the start date.
    internal static int BerekenWerkdagen(DateTimeOffset start, DateTimeOffset end)
    {
        var days = 0;
        var current = start.Date.AddDays(1);

        while (current <= end.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                days++;
            current = current.AddDays(1);
        }

        return days;
    }
}
