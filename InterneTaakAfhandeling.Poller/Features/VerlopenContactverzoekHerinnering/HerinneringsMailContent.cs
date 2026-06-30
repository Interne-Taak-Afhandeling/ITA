namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinnering;

public sealed class HerinneringsMailContent
{
    public required string Onderwerp { get; init; }
    public required string HtmlBody { get; init; }
    public required string PlainTextBody { get; init; }
}
