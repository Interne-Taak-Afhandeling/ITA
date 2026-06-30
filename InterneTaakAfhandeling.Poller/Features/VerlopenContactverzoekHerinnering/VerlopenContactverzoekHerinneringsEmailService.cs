using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public sealed class VerlopenContactverzoekHerinneringsEmailService(
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

            if (ontvanger.EmailAdressen.Count == 0)
            {
                logger.LogWarning(
                    "Daily reminder: no email address found for actor {ActorUuid} — skipped",
                    ontvanger.ActorUuid);
                overgeslagen++;
                continue;
            }

            var content = templateService.GenereerMailContent(
                ontvanger.AantalVerlopenContactVerzoeken,
                ontvanger.MaxAantalWerkdagenOpenstaan,
                ontvanger.IsMedewerker,
                _baseUrl);

            foreach (var email in ontvanger.EmailAdressen)
            {
                try
                {
                    await emailService.SendEmailAsync(email, content.Onderwerp, content.HtmlBody, content.PlainTextBody);
                    logger.LogInformation(
                        "Daily reminder: email sent for actor {ActorUuid} ({AantalCvs} contact requests)",
                        ontvanger.ActorUuid,
                        ontvanger.AantalVerlopenContactVerzoeken);
                    verstuurd++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Daily reminder: SMTP error for actor {ActorUuid} — skipped",
                        ontvanger.ActorUuid);
                    fouten++;
                }
            }
        }

        logger.LogInformation(
            "Daily reminder: run completed — sent: {Verstuurd}, skipped: {Overgeslagen}, errors: {Fouten}",
            verstuurd, overgeslagen, fouten);
    }
}
