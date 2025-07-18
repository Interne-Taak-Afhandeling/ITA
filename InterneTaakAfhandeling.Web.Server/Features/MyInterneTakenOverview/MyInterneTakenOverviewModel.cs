using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview;


public enum Status
{
    All,
    TeVerwerken,
    Verwerkt
}

public class MyInterneTakenQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Naam { get; set; }
    [EnumDataType(typeof(Status))]
    public Status? Status { get; set; }
    public int GetValidatedPage() => Math.Max(1, Page);
    public int GetValidatedPageSize() => Math.Min(Math.Max(1, PageSize), 50);
}


public class MyInterneTakenResponse
{
    public int Count { get; set; }
    public string? Next { get; set; }
    public string? Previous { get; set; }
    public List<MyInterneTaakItem> Results { get; set; } = new();
}

public class MyInterneTaakItem
{
    public string Uuid { get; set; } = string.Empty;                    // Internetaak.Uuid
    public string Nummer { get; set; } = string.Empty;                  // Internetaak.Nummer
    public string GevraagdeHandeling { get; set; } = string.Empty;      // Internetaak.GevraagdeHandeling
    public string Status { get; set; } = string.Empty;                  // Internetaak.Status
    public DateTimeOffset ToegewezenOp { get; set; }                    // Internetaak.ToegewezenOp
    public DateTimeOffset? AfgehandeldOp { get; set; }                  // Internetaak.AfgehandeldOp

    public string? Onderwerp { get; set; }                             // Klantcontact.Onderwerp
    public DateTimeOffset? ContactDatum { get; set; }                  // Klantcontact.PlaatsgevondenOp

    public string? KlantNaam { get; set; }

    public string? AfdelingNaam { get; set; }
    public string? BehandelaarNaam { get; set; }
}
public class MedewerkerResponse
{
    public List<string>? Groepen { get; set; }
    public List<string>? Afdelingen { get; set; }
}
