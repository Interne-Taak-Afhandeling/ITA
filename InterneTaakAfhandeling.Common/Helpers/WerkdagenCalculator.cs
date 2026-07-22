namespace InterneTaakAfhandeling.Common.Helpers;

public static class WerkdagenCalculator
{
    /// <summary>
    /// Aantal werkdaguren waarbinnen een contactverzoek afgehandeld dient te zijn (weekenden uitgezonderd).
    /// Gedeeld tussen UI (Urgentie-badge) en de e-mailherinnering zodat beide dezelfde streefdatum hanteren.
    /// </summary>
    public const double AfhandeltermijnUren = 48;


    /// <summary>
    /// Adds the given number of hours to a start time, skipping weekends entirely (24/7 on weekdays).
    /// </summary>
    public static DateTimeOffset AddWeekdayHours(DateTimeOffset start, TimeSpan hoursToAdd)
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

            var endOfDay = new DateTimeOffset(current.Date.AddDays(1), current.Offset);
            var hoursLeftInDay = (endOfDay - current).TotalHours;

            if (remainingHours <= hoursLeftInDay)
            {
                current = current.AddHours(remainingHours);
                remainingHours = 0;
            }
            else
            {
                remainingHours -= hoursLeftInDay;
                current = endOfDay;
            }
        }

        return current;
    }

    /// <summary>
    /// Calculates the number of weekday hours (excluding Saturday/Sunday) between now and the streefdatum.
    /// Returns negative values when the streefdatum has passed.
    /// </summary>
    public static double BerekenResterendeWerkdagUren(DateTimeOffset now, DateTimeOffset streefdatum)
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

            var endOfDay = new DateTimeOffset(current.Date.AddDays(1), current.Offset);
            var dayEnd = endOfDay < end ? endOfDay : end;

            totalHours += (dayEnd - current).TotalHours;
            current = endOfDay;
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
