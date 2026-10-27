namespace InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;

public class PartijIdentificatie
{
    public List<string> Bsn { get; init; } = [];
    public List<string> KvkNummer { get; init; } = [];
    public List<string> Vestigingsnummer { get; init; } = [];
}

public static class KnownPartijIdentificatorSoorten
{
    public const string Bsn = "bsn";
    public const string KvkNummer = "kvk_nummer";
    public const string Vestigingsnummer = "vestigingsnummer";
}
