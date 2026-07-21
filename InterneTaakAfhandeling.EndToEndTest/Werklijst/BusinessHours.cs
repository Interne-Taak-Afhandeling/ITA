namespace InterneTaakAfhandeling.EndToEndTest.Werklijst
{
    /// <summary>
    /// Provides Mon-Fri, 24h/day business-hour arithmetic used to construct a ContactDatum
    /// a target number of business hours in the past, immune to which weekday the suite
    /// happens to run on (<see cref="SubtractBusinessHours"/>).
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
