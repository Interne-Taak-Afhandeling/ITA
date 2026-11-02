using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InterneTaakAfhandeling.Common.Services.Urgentie;

/// <summary>
/// Waarschuwt bij configuraties die wel geldig zijn maar bijna zeker ongewenst:
/// een bijna-verlopen-drempel die hoger is dan de afhandeltermijn maakt elk nieuw
/// contactverzoek meteen oranje.
/// </summary>
public class UrgentieOptionsValidator(ILogger<UrgentieOptionsValidator> logger) : IValidateOptions<UrgentieOptions>
{
    public ValidateOptionsResult Validate(string? name, UrgentieOptions options)
    {
        if (options.BijnaVerlopenDrempelUren >= options.AfhandeltermijnUren)
        {
            logger.LogWarning(
                "Urgentie:BijnaVerlopenDrempelUren ({Drempel}) is groter dan of gelijk aan Urgentie:AfhandeltermijnUren ({Termijn}); elk nieuw contactverzoek wordt direct als bijna verlopen gemarkeerd.",
                options.BijnaVerlopenDrempelUren,
                options.AfhandeltermijnUren);
        }

        return ValidateOptionsResult.Success;
    }
}
