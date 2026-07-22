using InterneTaakAfhandeling.Common.Helpers;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

public class UrgentieBerekenService(ILogger<UrgentieBerekenService> logger) : IUrgentieBerekenService
{
    /// <summary>
    /// Aantal resterende werkdaguren waarna het urgentielabel oranje wordt.
    /// </summary>
    private const double BijnaVerlopenDrempelUren = 6;

    private readonly ILogger<UrgentieBerekenService> _logger = logger;

    public UrgentieInfo? Bereken(DateTimeOffset? contactDatum)
    {
        if (contactDatum is null)
        {
           return null;
        }

        var afhandeltermijn = TimeSpan.FromHours(WerkdagenCalculator.AfhandeltermijnUren);
        var streefdatum = WerkdagenCalculator.AddWeekdayHours(contactDatum.Value, afhandeltermijn);
        var now = DateTimeOffset.UtcNow.ToOffset(streefdatum.Offset);
        var resterendeWerkdagUren = WerkdagenCalculator.BerekenResterendeWerkdagUren(now, streefdatum);

        var status = resterendeWerkdagUren switch
        {
            <= 0 => UrgentieStatus.Verlopen,
            var u when u <= BijnaVerlopenDrempelUren => UrgentieStatus.BijnaVerlopen,
            _ => UrgentieStatus.BinnenTermijn
        };

        return new UrgentieInfo
        {
            Status = status switch
            {
                UrgentieStatus.BinnenTermijn => "success",
                UrgentieStatus.BijnaVerlopen => "warning",
                UrgentieStatus.Verlopen => "error",
                _ => ""
            },
            Label = BuildLabel(resterendeWerkdagUren)
        };
    }

    private static string BuildLabel(double resterendeWerkdagUren)
    {
        var uren = resterendeWerkdagUren >= 0
            ? Math.Ceiling(resterendeWerkdagUren)
            : -Math.Ceiling(Math.Abs(resterendeWerkdagUren));

        return uren switch
        {
            > 48  => $"nog {(int)Math.Ceiling(uren / 24)}d",
            > 0   => $"nog {uren}u",
            < -48 => $"{(int)Math.Ceiling(Math.Abs(uren) / 24)}d verlopen",
            _     => $"{Math.Abs(uren)}u verlopen"
        };
    }

}
