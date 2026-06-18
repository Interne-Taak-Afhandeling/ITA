namespace InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

public interface IDagelijkseHerinneringsEmailService
{
    Task StuurHerinneringenAsync(
        IReadOnlyList<RecipientHerinneringData> ontvangers,
        CancellationToken cancellationToken = default);
}
