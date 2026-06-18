namespace InterneTaakAfhandeling.Common.Services.Afhandeltermijn;

// Hardcoded at 48 hours; swap this registration for a configurable implementation in #329.
public sealed class HardcodedAfhandeltermijnProvider : IAfhandeltermijnProvider
{
    public TimeSpan GetAfhandeltermijn() => TimeSpan.FromHours(48);
}
