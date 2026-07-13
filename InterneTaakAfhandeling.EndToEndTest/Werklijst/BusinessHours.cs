namespace InterneTaakAfhandeling.EndToEndTest.Werklijst
{
    /// <summary>
    /// Mirrors the Mon-Fri, 24h/day business-hour arithmetic used by UrgentieBerekenService
    /// (afhandeltermijn 48h, bijna-verlopen drempel 6h). Used to:
    /// <list type="bullet">
    /// <item>construct a ContactDatum a target number of business hours in the past, immune to
    /// which weekday the suite happens to run on (<see cref="SubtractBusinessHours"/>)</item>
    /// <item>compute the expected badge status/label at assertion time as a "known answer"
    /// oracle, since real elapsed time drifts slightly between fixture creation and page load
    /// (<see cref="ElapsedBusinessHours"/>, <see cref="ExpectedStatusClass"/>, <see cref="ExpectedLabel"/>)</item>
    /// </list>
    /// </summary>
    internal static class BusinessHours
    {
        internal const int AfhandeltermijnUren = 48;
        internal const int BijnaVerlopenDrempelUren = 6;

        internal static DateTime SubtractBusinessHours(DateTime from, double hours)
        {
            var current = from;
            var remaining = hours;
            while (remaining > 0)
            {
                // A `current` sitting exactly at midnight represents the end of the previous
                // day, not the start of its own day - treat the day ending at `current` as the
                // one being consumed, so weekday/weekend checks and hour math stay correct.
                var dayStart = current.TimeOfDay == TimeSpan.Zero ? current.AddDays(-1).Date : current.Date;
                if (IsWeekend(dayStart))
                {
                    current = dayStart;
                    continue;
                }

                var hoursAvailable = (current - dayStart).TotalHours;
                if (remaining <= hoursAvailable)
                {
                    current = current.AddHours(-remaining);
                    remaining = 0;
                }
                else
                {
                    remaining -= hoursAvailable;
                    current = dayStart;
                }
            }
            return current;
        }

        internal static double ElapsedBusinessHours(DateTime from, DateTime to)
        {
            var current = from;
            var total = 0.0;
            while (current < to)
            {
                if (IsWeekend(current))
                {
                    current = SkipForwardToWeekday(current);
                    continue;
                }

                var endOfDay = current.Date.AddDays(1);
                var dayEnd = endOfDay < to ? endOfDay : to;
                total += (dayEnd - current).TotalHours;
                current = endOfDay;
            }
            return total;
        }

        internal static string ExpectedStatusClass(double resterendeWerkdagUren) => resterendeWerkdagUren switch
        {
            <= 0 => "utrecht-badge-status--error",
            var u when u <= BijnaVerlopenDrempelUren => "utrecht-badge-status--warning",
            _ => "utrecht-badge-status--success"
        };

        internal static string ExpectedLabel(double resterendeWerkdagUren)
        {
            var uren = resterendeWerkdagUren >= 0
                ? Math.Ceiling(resterendeWerkdagUren)
                : -Math.Ceiling(Math.Abs(resterendeWerkdagUren));

            return uren switch
            {
                > 48 => $"nog {(int)Math.Ceiling(uren / 24)}d",
                > 0 => $"nog {uren}u",
                < -48 => $"{(int)Math.Ceiling(Math.Abs(uren) / 24)}d verlopen",
                _ => $"{Math.Abs(uren)}u verlopen"
            };
        }

        private static bool IsWeekend(DateTime date) =>
            date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        private static DateTime SkipForwardToWeekday(DateTime date)
        {
            var next = date.Date.AddDays(1);
            while (IsWeekend(next))
            {
                next = next.AddDays(1);
            }
            return next;
        }
    }
}
