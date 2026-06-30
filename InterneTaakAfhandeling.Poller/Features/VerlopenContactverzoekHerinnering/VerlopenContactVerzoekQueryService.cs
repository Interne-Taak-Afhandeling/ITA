using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public class VerlopenContactVerzoekQueryService(
    IOpenKlantApiClient openKlantApiClient,
    IAfhandeltermijnProvider afhandeltermijnProvider,
    IInterneTaakEmailInputService emailInputService,
    ILogger<VerlopenContactVerzoekQueryService> logger) : IVerlopenContactVerzoekQueryService
{
    public async Task<IReadOnlyList<RecipientHerinneringData>> GetVerlopenContactVerzoekenAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Daily reminder: query started");

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

        logger.LogInformation("Daily reminder: {Count} te_verwerken internetaken retrieved", allInternetaken.Count);

        var verlopen = allInternetaken.Where(taak => IsVerlopen(taak, afhandeltermijn, now)).ToList();

        logger.LogInformation("Daily reminder: {Count} verlopen after date filter", verlopen.Count);

        var byActor = new Dictionary<string, (Actor Actor, List<Internetaak> Taken)>();

        foreach (var taak in verlopen)
        {
            foreach (var actor in taak.ToegewezenAanActoren ?? [])
            {
                if (string.IsNullOrEmpty(actor.Uuid))
                {
                    logger.LogWarning("Daily reminder: actor without UUID found for internetaak {Uuid} — skipped", taak.Uuid);
                    continue;
                }

                if (actor.SoortActor != SoortActor.medewerker && actor.SoortActor != SoortActor.organisatorische_eenheid)
                {
                    logger.LogWarning("Daily reminder: unrecognised actor type {SoortActor} for actor {ActorUuid} on internetaak {InternetaakUuid} — skipped",
                        actor.SoortActor, actor.Uuid, taak.Uuid);
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

        logger.LogInformation("Daily reminder: {Count} recipient groups after grouping", byActor.Count);

        var recipients = new List<RecipientHerinneringData>();

        foreach (var (_, entry) in byActor)
        {
            var resolveResult = await emailInputService.ResolveActorsEmailAsync([entry.Actor]);
            recipients.Add(new RecipientHerinneringData
            {
                ActorUuid = entry.Actor.Uuid,
                IsMedewerker = entry.Actor.SoortActor == SoortActor.medewerker,
                EmailAdressen = resolveResult.FoundEmails,
                AantalVerlopenContactVerzoeken = entry.Taken.Count,
                MaxAantalWerkdagenOpenstaan = entry.Taken.Max(taak =>
                    BerekenWerkdagen(taak.AanleidinggevendKlantcontact?.PlaatsgevondenOp ?? now, now))
            });
        }

        return recipients;
    }

    private static bool IsVerlopen(Internetaak taak, TimeSpan afhandeltermijn, DateTimeOffset now)
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
