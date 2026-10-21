using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Common.Services.Urgentie;

/// <summary>
/// Instellingen die de streefdatum en het urgentielabel van een contactverzoek bepalen.
/// Gedeeld tussen de werklijst (Web) en de herinneringsmail (Poller) zodat beide dezelfde termijn hanteren.
/// </summary>
public class UrgentieOptions
{
    public const double DefaultAfhandeltermijnUren = 48;
    public const double DefaultBijnaVerlopenDrempelUren = 6;

    /// <summary>
    /// Aantal werkdaguren waarbinnen een contactverzoek afgehandeld dient te zijn. Weekenden tellen niet mee.
    /// </summary>
    [Range(double.Epsilon, double.MaxValue, ErrorMessage = "Urgentie:AfhandeltermijnUren moet een positief getal zijn.")]
    public double AfhandeltermijnUren { get; set; } = DefaultAfhandeltermijnUren;

    /// <summary>
    /// Aantal resterende werkdaguren waarna het urgentielabel oranje wordt.
    /// </summary>
    [Range(double.Epsilon, double.MaxValue, ErrorMessage = "Urgentie:BijnaVerlopenDrempelUren moet een positief getal zijn.")]
    public double BijnaVerlopenDrempelUren { get; set; } = DefaultBijnaVerlopenDrempelUren;
}
