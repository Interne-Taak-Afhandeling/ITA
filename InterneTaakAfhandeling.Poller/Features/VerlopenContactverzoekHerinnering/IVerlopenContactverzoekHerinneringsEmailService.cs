namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public interface IVerlopenContactverzoekHerinneringsEmailService
{
    Task StuurHerinneringenAsync(
        IReadOnlyList<RecipientHerinneringData> ontvangers,
        CancellationToken cancellationToken = default);
}
