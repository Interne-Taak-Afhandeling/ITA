namespace InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

public interface IVerlopenContactverzoekHerinneringsEmailService
{
    Task StuurHerinneringenAsync(
        IReadOnlyList<RecipientHerinneringData> ontvangers,
        CancellationToken cancellationToken = default);
}
