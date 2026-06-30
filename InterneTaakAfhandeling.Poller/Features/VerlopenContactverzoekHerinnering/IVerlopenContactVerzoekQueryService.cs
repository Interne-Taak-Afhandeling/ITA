namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public interface IVerlopenContactVerzoekQueryService
{
    Task<IReadOnlyList<RecipientHerinneringData>> GetVerlopenContactVerzoekenAsync(CancellationToken cancellationToken = default);
}
