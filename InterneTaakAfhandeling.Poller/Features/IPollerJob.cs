namespace InterneTaakAfhandeling.Poller.Features;

public interface IPollerJob
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
