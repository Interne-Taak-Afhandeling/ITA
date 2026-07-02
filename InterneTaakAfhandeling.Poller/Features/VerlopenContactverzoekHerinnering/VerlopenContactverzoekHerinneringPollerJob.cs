namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public sealed class VerlopenContactverzoekHerinneringPollerJob(IVerlopenInternetakenProcessor processor) : IPollerJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
        => processor.StuurHerinneringenVoorVerlopenInternetakenAsync(cancellationToken);
}
