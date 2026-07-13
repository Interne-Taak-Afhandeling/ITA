namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinneringNotificatie;

public sealed class VerlopenContactverzoekHerinneringNotificatiePollerJob(IVerlopenInternetakenProcessor processor) : IPollerJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
        => processor.StuurHerinneringenVoorVerlopenInternetakenAsync(cancellationToken);
}
