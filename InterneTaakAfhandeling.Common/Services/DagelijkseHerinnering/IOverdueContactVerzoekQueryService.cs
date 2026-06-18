namespace InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

public interface IOverdueContactVerzoekQueryService
{
    Task<IReadOnlyList<RecipientHerinneringData>> GetOverdueContactVerzoekenAsync(CancellationToken cancellationToken = default);
}
