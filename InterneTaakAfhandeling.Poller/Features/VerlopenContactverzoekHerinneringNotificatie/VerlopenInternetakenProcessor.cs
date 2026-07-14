using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinneringNotificatie;


public sealed class VerlopenInternetakenProcessor(
    IOpenKlantApiClient openKlantApiClient,

    IInterneTaakEmailInputService emailInputService,
    VerlopenContactverzoekHerinneringNotificatieTemplateService templateService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<VerlopenInternetakenProcessor> logger) : IPollerJob
{
    private readonly string _baseUrl = configuration.GetValue<string>("Ita:BaseUrl")
        ?? throw new InvalidOperationException("Ita:BaseUrl configuratie ontbreekt.");
    private readonly VerlopenContactverzoekHerinneringNotificatieTemplateService _templateService = templateService;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Daily reminder: run started");

        var now = DateTimeOffset.UtcNow;

        var verlopenContactVerzoeken = await GetVerlopenContactVerzoekenAsync(now);

        var herrinneringsModels = await BuildHerrinneringsModels(now, verlopenContactVerzoeken);

        var (verstuurd, overgeslagen, fouten) = await StuurHerinneringenAsync(herrinneringsModels, cancellationToken);

        logger.LogInformation(
           "Daily reminder: run completed — sent: {Verstuurd}, skipped: {Overgeslagen}, errors: {Fouten}",
           verstuurd, overgeslagen, fouten);
    }

    private async Task<Dictionary<string, (Actor Actor, List<Internetaak> Taken)>> GetVerlopenContactVerzoekenAsync(DateTimeOffset now)
    {
        var verlopenInterneTaken = await GetVerlopenInternetaken(now);

        logger.LogInformation("Daily reminder: {Count} verlopen contactverzoeken", verlopenInterneTaken.Count);

        return GroupVerlopenTakenByActor(verlopenInterneTaken);

    }

    private async Task<List<RecipientHerinneringData>> BuildHerrinneringsModels(DateTimeOffset now, Dictionary<string, (Actor Actor, List<Internetaak> Taken)> verlopenTakenByActor)
    {
        var recipients = new List<RecipientHerinneringData>();

        foreach (var (_, entry) in verlopenTakenByActor)
        {
            var resolveResult = await emailInputService.ResolveActorsEmailAsync([entry.Actor]);

            foreach (var error in resolveResult.Errors)
            {
                logger.LogWarning("Error while resolving actors for interne taken {Numbers}: {Error}", entry.Taken.Select(x => x.Nummer).ToList(), error);
            }

            if (resolveResult.FoundEmails.Count == 0)
            {
                logger.LogWarning("Daily reminder: no email address found for actor {ActorUuid}, for interne taken {Numbers}", entry.Actor.Uuid, entry.Taken.Select(x => x.Nummer).ToList());
                continue;
            }

            recipients.Add(new RecipientHerinneringData
            {
                ActorUuid = entry.Actor.Uuid,
                Link = entry.Actor.SoortActor == SoortActor.medewerker ? "/afdelings-contacten" : "/",
                EmailAdressen = resolveResult.FoundEmails,
                AantalVerlopenContactVerzoeken = entry.Taken.Count,
                MaxAantalWerkdagenOpenstaan = entry.Taken.Max(taak =>
                    BerekenWerkdagen(taak.AanleidinggevendKlantcontact?.PlaatsgevondenOp ?? now, now))
            });

        }

        return recipients;
    }

    private Dictionary<string, (Actor Actor, List<Internetaak> Taken)> GroupVerlopenTakenByActor(List<Internetaak> verlopenInterneTaken)
    {
        var byActor = new Dictionary<string, (Actor Actor, List<Internetaak> Taken)>();

        foreach (var taak in verlopenInterneTaken)
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
                    logger.LogInformation("Daily reminder: unrecognised actor type {SoortActor} for actor {ActorUuid} on internetaak {InternetaakUuid} — skipped",
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

        return byActor;
    }

    private async Task<List<Internetaak>> GetVerlopenInternetaken(DateTimeOffset now)
    {
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

        return [.. allInternetaken.Where(taak => IsVerlopen(taak, now))]; ;
    }

    private async Task<(int verstuurd, int overgeslagen, int fouten)> StuurHerinneringenAsync(
        IReadOnlyList<RecipientHerinneringData> herinneringen,
        CancellationToken cancellationToken)
    {
        var verstuurd = 0;
        var overgeslagen = 0;
        var fouten = 0;

        foreach (var herinnering in herinneringen)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var werkvoorraadUrl = $"{_baseUrl.TrimEnd('/')}{herinnering.Link}";

            var content = VerlopenContactverzoekHerinneringNotificatieTemplateService.GenereerMailBody(
                herinnering.AantalVerlopenContactVerzoeken,
                herinnering.MaxAantalWerkdagenOpenstaan,
               werkvoorraadUrl);

            var plainTextContent = VerlopenContactverzoekHerinneringNotificatieTemplateService.GenereerMailBodyPlainText(
                herinnering.AantalVerlopenContactVerzoeken,
                herinnering.MaxAantalWerkdagenOpenstaan,
                werkvoorraadUrl);

            var onderwerp = VerlopenContactverzoekHerinneringNotificatieTemplateService.GenereerMailSubject(herinnering.AantalVerlopenContactVerzoeken);

            foreach (var email in herinnering.EmailAdressen)
            {
                try
                {
                    await emailService.SendEmailAsync(email, onderwerp, content, plainTextContent);
                    logger.LogInformation(
                        "Daily reminder: email sent for actor {ActorUuid} ({AantalCvs} contact requests)",
                        herinnering.ActorUuid,
                        herinnering.AantalVerlopenContactVerzoeken);
                    verstuurd++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Daily reminder: SMTP error for actor {ActorUuid} — skipped",
                        herinnering.ActorUuid);
                    fouten++;
                }
            }
        }

        return (verstuurd, overgeslagen, fouten);
    }

    private static bool IsVerlopen(Internetaak taak, DateTimeOffset now)
    {
        var plaatsgevondenOp = taak.AanleidinggevendKlantcontact?.PlaatsgevondenOp;
        return plaatsgevondenOp != null && CalculateExpirationDateTime(plaatsgevondenOp.Value) <= now;
    }

    private static DateTimeOffset CalculateExpirationDateTime(DateTimeOffset start)
    {
        // Afhandeltermijn hardcoded at 48 hours; swap this registration for a configurable implementation in #329.
        var remaining = 48;
        var current = start;

        while (remaining > 0)
        {
            current = current.AddHours(1);
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                remaining--;
        }

        return current;
    }

    private static int BerekenWerkdagen(DateTimeOffset start, DateTimeOffset end)
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
