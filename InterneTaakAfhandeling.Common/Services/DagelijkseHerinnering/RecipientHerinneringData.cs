using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

public sealed class RecipientHerinneringData
{
    public required Actor Ontvanger { get; init; }
    public required IReadOnlyList<Internetaak> VerlopenContactVerzoeken { get; init; }
    public required int MaxAantalWerkdagenOpenstaan { get; init; }
}
