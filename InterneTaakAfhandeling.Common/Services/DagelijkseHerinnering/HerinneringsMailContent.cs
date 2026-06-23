namespace InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

public sealed class HerinneringsMailContent
{
    public required string Onderwerp { get; init; }
    public required string HtmlBody { get; init; }
    public required string PlainTextBody { get; init; }
}
