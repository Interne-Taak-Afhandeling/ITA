using InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;
using InterneTaakAfhandeling.Poller.Features;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public sealed class VerlopenContactverzoekHerinneringPollerJob(
    IOverdueContactVerzoekQueryService overdueService,
    IVerlopenContactverzoekHerinneringsEmailService emailService) : IPollerJob
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var ontvangers = await overdueService.GetOverdueContactVerzoekenAsync(cancellationToken);
        await emailService.StuurHerinneringenAsync(ontvangers, cancellationToken);
    }
}
