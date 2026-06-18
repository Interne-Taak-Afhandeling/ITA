using InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;
using InterneTaakAfhandeling.Common.Services.Emailservices.Content;
using InterneTaakAfhandeling.Common.Services.Emailservices.SmtpMailService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InterneTaakAfhandeling.Poller.Features.DagelijkseHerinnering;

public sealed class DagelijkseHerinneringsEmailService(
    IInterneTaakEmailInputService emailInputService,
    IDagelijkseHerinneringsTemplateService templateService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<DagelijkseHerinneringsEmailService> logger) : IDagelijkseHerinneringsEmailService
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
                    "Dagelijkse herinnering: geen e-mailadres gevonden voor actor {ActorUuid} — overgeslagen",
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
                        "Dagelijkse herinnering: mail verstuurd naar {EmailPrefix}... voor actor {ActorUuid} ({AantalCvs} CVs)",
                        email[..Math.Min(email.Length, 4)],
                        actorUuid,
                        ontvanger.VerlopenContactVerzoeken.Count);
                    verstuurd++;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Dagelijkse herinnering: SMTP-fout voor {EmailPrefix}... (actor {ActorUuid}) — overgeslagen",
                        email[..Math.Min(email.Length, 4)],
                        actorUuid);
                    fouten++;
                }
            }
        }

        logger.LogInformation(
            "Dagelijkse herinnering: run voltooid — verstuurd: {Verstuurd}, overgeslagen: {Overgeslagen}, fouten: {Fouten}",
            verstuurd, overgeslagen, fouten);
    }
}
