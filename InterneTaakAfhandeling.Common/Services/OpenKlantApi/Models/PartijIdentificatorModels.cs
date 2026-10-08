namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

// Kept separate from Partij on purpose: Partij is part of the Klantcontact that is sent to the client,
// and partij-identificatoren (BSN/KVK) may only be sent when the installation enables it.
public class PartijMetIdentificatoren
{
    public List<PartijIdentificatorItem>? PartijIdentificatoren { get; set; }
}

public class PartijIdentificatorItem
{
    public PartijIdentificator? PartijIdentificator { get; set; }
}

public class PartijIdentificator
{
    public string? CodeObjecttype { get; set; }
    public string? CodeSoortObjectId { get; set; }
    public string? ObjectId { get; set; }
    public string? CodeRegister { get; set; }
}

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
