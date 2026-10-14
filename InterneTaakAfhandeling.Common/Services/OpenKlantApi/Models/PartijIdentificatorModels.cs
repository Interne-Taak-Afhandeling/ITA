namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

public class PartijIdentificatie
{
    public string? Bsn { get; init; }
    public string? KvkNummer { get; init; }
    public string? Vestigingsnummer { get; init; }
}

public static class KnownPartijIdentificatorSoorten
{
    public const string Bsn = "bsn";
    public const string KvkNummer = "kvk_nummer";
    public const string Vestigingsnummer = "vestigingsnummer";
}
