using System.ComponentModel.DataAnnotations;
using InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.AllInternetakenOverview;



public class InterneTakenOverviewQueryParameters
{
    [Range(1, 100)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

}

public class InterneTakenOverviewResponse
{
    public int Count { get; init; }
    public string? Next { get; init; }
    public string? Previous { get; init; }
    public List<InterneTaakOverviewItem> Results { get; init; } = [];
}

public class InterneTaakOverviewItem
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

    public string? ContactmomentNummer { get; set; }

    public UrgentieInfo? Urgentie { get; set; }
}
