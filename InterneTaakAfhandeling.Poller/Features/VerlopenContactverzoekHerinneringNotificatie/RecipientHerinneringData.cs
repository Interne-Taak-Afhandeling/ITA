using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinneringNotificatie;

public sealed class RecipientHerinneringData
{
    public required string Link { get; init; }
    public required string ActorUuid { get; init; }
    [MinLength(1)]
    public required IReadOnlyList<string> EmailAdressen { get; init; }
    public required int AantalVerlopenContactVerzoeken { get; init; }
    public required int MaxAantalWerkdagenOpenstaan { get; init; }
}
