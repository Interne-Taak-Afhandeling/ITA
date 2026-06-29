using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Web.Server.Features.Urgentie;

public class UrgentieBerekenService(
    IOptions<UrgentieOptions> options,
    ILogger<UrgentieBerekenService> logger) : IUrgentieBerekenService
{
    private readonly UrgentieOptions _options = options.Value;
    private readonly ILogger<UrgentieBerekenService> _logger = logger;

    public UrgentieInfo? Bereken(DateTimeOffset? contactDatum)
    {
        if (contactDatum is null)
        {
            _logger.LogWarning("Urgentieberekening overgeslagen: ContactDatum is null");
            return null;
        }

        var afhandeltermijn = TimeSpan.FromHours(_options.AfhandeltermijnUren);
        var streefdatum = AddWeekdayHours(contactDatum.Value, afhandeltermijn);
        var resterendeWerkdagUren = BerekenResterendeWerkdagUren(DateTimeOffset.UtcNow, streefdatum);

        var status = resterendeWerkdagUren switch
        {
            <= 0 => UrgentieStatus.Verlopen,
            var u when u <= _options.BijnaVerlopenDrempelUren => UrgentieStatus.BijnaVerlopen,
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
            Streefdatum = streefdatum,
            ResterendeUren = Math.Round(resterendeWerkdagUren)
        };
    }

    /// <summary>
    /// Adds the given number of hours to a start time, skipping weekends entirely (24/7 on weekdays).
    /// </summary>
    internal static DateTimeOffset AddWeekdayHours(DateTimeOffset start, TimeSpan hoursToAdd)
    {
        var current = start;
        var remainingHours = hoursToAdd.TotalHours;

        while (remainingHours > 0)
        {
            if (IsWeekend(current))
            {
                current = SkipToNextWeekday(current);
                continue;
            }

            var endOfDay = current.Date.AddDays(1);
            var hoursLeftInDay = (endOfDay - current).TotalHours;

            if (remainingHours <= hoursLeftInDay)
            {
                current = current.AddHours(remainingHours);
                remainingHours = 0;
            }
            else
            {
                remainingHours -= hoursLeftInDay;
                current = new DateTimeOffset(endOfDay, current.Offset);
            }
        }

        return current;
    }

    /// <summary>
    /// Calculates the number of weekday hours (excluding Saturday/Sunday) between now and the streefdatum.
    /// Returns negative values when the streefdatum has passed.
    /// </summary>
    internal static double BerekenResterendeWerkdagUren(DateTimeOffset now, DateTimeOffset streefdatum)
    {
        if (now >= streefdatum)
        {
            return -BerekenWerkdagUrenTussen(streefdatum, now);
        }

        return BerekenWerkdagUrenTussen(now, streefdatum);
    }

    private static double BerekenWerkdagUrenTussen(DateTimeOffset start, DateTimeOffset end)
    {
        var current = start;
        var totalHours = 0.0;

        while (current < end)
        {
            if (IsWeekend(current))
            {
                current = SkipToNextWeekday(current);
                continue;
            }

            var endOfDay = current.Date.AddDays(1);
            var dayEnd = endOfDay < end
                ? new DateTimeOffset(endOfDay, current.Offset)
                : end;

            totalHours += (dayEnd - current).TotalHours;
            current = new DateTimeOffset(endOfDay, current.Offset);
        }

        return totalHours;
    }

    private static bool IsWeekend(DateTimeOffset date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static DateTimeOffset SkipToNextWeekday(DateTimeOffset date)
    {
        var next = date.Date.AddDays(1);
        while (next.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            next = next.AddDays(1);
        }

        return new DateTimeOffset(next, date.Offset);
    }
}
