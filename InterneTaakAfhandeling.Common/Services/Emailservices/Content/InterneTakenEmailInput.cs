using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.Content
{
    public record InterneTakenEmailInput(
        Internetaak InterneTaak,
        Klantcontact Klantcontact,
        IReadOnlyList<DigitaleAdres> DigitaleAdressen,
        Zaak? Zaak
        );
}
