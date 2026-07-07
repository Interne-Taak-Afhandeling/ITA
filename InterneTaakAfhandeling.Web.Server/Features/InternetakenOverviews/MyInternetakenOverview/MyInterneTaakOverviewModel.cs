using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.Shared.Urgentie;

namespace InterneTaakAfhandeling.Web.Server.Features.InternetakenOverviews.MyInternetakenOverview;

public class MyInterneTaakOverviewItem
{
    public string Uuid { get; init; } = string.Empty;
    public string? Nummer { get; init; }
    public string? GevraagdeHandeling { get; init; }
    public string? Status { get; init; }
    public DateTimeOffset? ToegewezenOp { get; init; }
    public DateTimeOffset? AfgehandeldOp { get; init; }
    public Klantcontact? AanleidinggevendKlantcontact { get; init; }
    public UrgentieInfo? Urgentie { get; init; }
    public string? AfdelingNaam { get; init; }
}
