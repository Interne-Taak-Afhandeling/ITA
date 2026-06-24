namespace InterneTaakAfhandeling.Web.Server.Features.Urgentie;

public class UrgentieBerekenService(ILogger<UrgentieBerekenService> logger) : IUrgentieBerekenService
{
    private static readonly TimeSpan Afhandeltermijn = TimeSpan.FromHours(48);
    private static readonly TimeSpan BijnaVerlopenDrempel = TimeSpan.FromHours(6);

    private readonly ILogger<UrgentieBerekenService> _logger = logger;

    public UrgentieInfo? Bereken(DateTimeOffset? contactDatum)
    {
        if (contactDatum is null)
        {
            _logger.LogWarning("Urgentieberekening overgeslagen: ContactDatum is null");
            return null;
        }

        var streefdatum = contactDatum.Value + Afhandeltermijn;
        var resterend = streefdatum - DateTimeOffset.UtcNow;

        var status = resterend switch
        {
            { TotalHours: <= 0 } => UrgentieStatus.Verlopen,
            { TotalHours: <= 6 } => UrgentieStatus.BijnaVerlopen,
            _ => UrgentieStatus.BinnenTermijn
        };

        return new UrgentieInfo
        {
            Status = status switch
            {
                UrgentieStatus.BinnenTermijn => "binnen_termijn",
                UrgentieStatus.BijnaVerlopen => "bijna_verlopen",
                UrgentieStatus.Verlopen => "verlopen",
                _ => "binnen_termijn"
            },
            Streefdatum = streefdatum
        };
    }
}
