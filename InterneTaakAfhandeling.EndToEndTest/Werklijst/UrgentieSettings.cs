using System.Globalization;

namespace InterneTaakAfhandeling.EndToEndTest.Werklijst
{
    /// <summary>
    /// Leest de urgentie-instellingen van de omgeving waartegen de E2E suite draait, zodat
    /// scenario's hun ContactDatum relatief ten opzichte van de werkelijk geconfigureerde
    /// afhandeltermijn en bijna-verlopen drempel kunnen maken. Zonder expliciete configuratie
    /// vallen we terug op de applicatie-defaults (48 / 6).
    /// </summary>
    /// <remarks>
    /// Env-vars volgen de ASP.NET Core conventie: dubbele underscores (`__`) worden omgezet
    /// naar section-separators. Hetzelfde patroon dat de Helm ConfigMap gebruikt om de
    /// app te configureren wordt hier doorgespiegeld naar de teststage.
    /// </remarks>
    internal static class UrgentieSettings
    {
        internal static double AfhandeltermijnUren =>
            ReadDouble("Urgentie__AfhandeltermijnUren", defaultValue: 48);

        internal static double BijnaVerlopenDrempelUren =>
            ReadDouble("Urgentie__BijnaVerlopenDrempelUren", defaultValue: 6);

        private static double ReadDouble(string envVarName, double defaultValue)
        {
            var raw = Environment.GetEnvironmentVariable(envVarName);
            return !string.IsNullOrWhiteSpace(raw) &&
                   double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : defaultValue;
        }
    }
}
