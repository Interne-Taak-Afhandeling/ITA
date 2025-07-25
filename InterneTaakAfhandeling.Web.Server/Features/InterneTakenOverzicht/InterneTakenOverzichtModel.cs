using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Features.InterneTakenOverzicht;



public class InterneTakenOverzichtQueryParameters
{
    [Range(1, 100)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

}

public class InterneTakenOverzichtResponse
{
    public int Count { get; init; }
    public string? Next { get; init; }
    public string? Previous { get; init; }
    public List<InterneTaakOverzichtItem> Results { get; init; } = [];
}

public class InterneTaakOverzichtItem
{


    public string Uuid { get; set; } = string.Empty;
    public string? Nummer { get; set; } = string.Empty;
    public string? GevraagdeHandeling { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public DateTimeOffset? ToegewezenOp { get; init; }
    public DateTimeOffset? AfgehandeldOp { get; set; }

    public string? Onderwerp { get; set; }
    public DateTimeOffset? ContactDatum { get; set; }

    public string? KlantNaam { get; set; }

    public string? AfdelingNaam { get; set; }
    public string? BehandelaarNaam { get; set; }
}
