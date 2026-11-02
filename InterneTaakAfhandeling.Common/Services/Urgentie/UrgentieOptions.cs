using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Common.Services.Urgentie;

/// <summary>
/// Instellingen die de streefdatum en het urgentielabel van een contactverzoek bepalen.
/// Gedeeld tussen de werklijst (Web) en de herinneringsmail (Poller) zodat beide dezelfde termijn hanteren.
/// </summary>
public class UrgentieOptions
{
    public const int DefaultAfhandeltermijnUren = 48;
    public const int DefaultBijnaVerlopenDrempelUren = 6;

    /// <summary>
    /// Aantal werkdaguren waarbinnen een contactverzoek afgehandeld dient te zijn. Weekenden tellen niet mee.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Urgentie:AfhandeltermijnUren moet een positief geheel getal zijn.")]
    public int AfhandeltermijnUren { get; set; } = DefaultAfhandeltermijnUren;

    /// <summary>
    /// Aantal resterende werkdaguren waarna het urgentielabel oranje wordt.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Urgentie:BijnaVerlopenDrempelUren moet een positief geheel getal zijn.")]
    public int BijnaVerlopenDrempelUren { get; set; } = DefaultBijnaVerlopenDrempelUren;
}
