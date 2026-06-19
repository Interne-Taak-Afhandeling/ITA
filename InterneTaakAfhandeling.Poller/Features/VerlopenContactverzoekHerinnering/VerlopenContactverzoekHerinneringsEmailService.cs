using InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;
using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public sealed class VerlopenContactverzoekHerinneringsEmailService(
    IInterneTaakEmailInputService emailInputService,
    IVerlopenContactverzoekHerinneringsTemplateService templateService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<VerlopenContactverzoekHerinneringsEmailService> logger) : IVerlopenContactverzoekHerinneringsEmailService
{
    private readonly string _baseUrl = configuration.GetValue<string>("Ita:BaseUrl")
        ?? throw new InvalidOperationException("Ita:BaseUrl configuratie ontbreekt.");

    public async Task StuurHerinneringenAsync(
        IReadOnlyList<RecipientHerinneringData> ontvangers,
        CancellationToken cancellationToken = default)
    {
        var verstuurd = 0;
        var overgeslagen = 0;
        var fouten = 0;

        foreach (var ontvanger in ontvangers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var actorUuid = ontvanger.Ontvanger.Uuid;
            var resolveResult = await emailInputService.ResolveActorsEmailAsync([ontvanger.Ontvanger]);

            if (resolveResult.FoundEmails.Count == 0)
            {
                logger.LogWarning(
                    "Daily reminder: no email address found for actor {ActorUuid} — skipped",
                    actorUuid);
                overgeslagen++;
                continue;
            }

            var content = templateService.GenereerMailContent(ontvanger, _baseUrl);

            foreach (var email in resolveResult.FoundEmails)
            {
                try
                {
                    await emailService.SendEmailAsync(email, content.Onderwerp, content.HtmlBody);
                    logger.LogInformation(
                        "Daily reminder: email sent to {EmailPrefix}... for actor {ActorUuid} ({AantalCvs} contact requests)",
                        email[..Math.Min(email.Length, 4)],
                        actorUuid,
                        ontvanger.VerlopenContactVerzoeken.Count);
                    verstuurd++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Daily reminder: SMTP error for {EmailPrefix}... (actor {ActorUuid}) — skipped",
                        email[..Math.Min(email.Length, 4)],
                        actorUuid);
                    fouten++;
                }
            }
        }

        logger.LogInformation(
            "Daily reminder: run completed — sent: {Verstuurd}, skipped: {Overgeslagen}, errors: {Fouten}",
            verstuurd, overgeslagen, fouten);
    }
}
