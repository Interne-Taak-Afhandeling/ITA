using InterneTaakAfhandeling.Poller.Features;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public sealed class VerlopenContactverzoekHerinneringPollerJob(
    IVerlopenContactVerzoekQueryService verlopenService,
    IVerlopenContactverzoekHerinneringsEmailService emailService) : IPollerJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var ontvangers = await verlopenService.GetVerlopenContactVerzoekenAsync(cancellationToken);
        await emailService.StuurHerinneringenAsync(ontvangers, cancellationToken);
    }
}
