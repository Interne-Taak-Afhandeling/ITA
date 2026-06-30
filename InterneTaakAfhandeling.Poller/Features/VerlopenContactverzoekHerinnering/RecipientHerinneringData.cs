namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public sealed class RecipientHerinneringData
{
    public required string ActorUuid { get; init; }
    public required bool IsMedewerker { get; init; }
    public required IReadOnlyList<string> EmailAdressen { get; init; }
    public required int AantalVerlopenContactVerzoeken { get; init; }
    public required int MaxAantalWerkdagenOpenstaan { get; init; }
}
