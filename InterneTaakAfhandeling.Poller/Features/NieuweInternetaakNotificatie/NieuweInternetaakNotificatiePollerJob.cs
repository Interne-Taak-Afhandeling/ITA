namespace InterneTaakAfhandeling.Poller.Features.NieuweInternetaakNotificatie;

public sealed class NieuweInternetaakNotificatiePollerJob(INieuweInternetakenProcessor processor) : IPollerJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
        => processor.NotifyAboutNewInternetakenAsync();
}
