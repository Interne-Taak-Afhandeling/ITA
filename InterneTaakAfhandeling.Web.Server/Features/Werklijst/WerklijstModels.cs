using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Features.Werklijst;

public class WerklijstQuery
{
    [Range(1, 100)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public Guid? AfdelingUuid { get; set; }
    public Guid? GroepUuid { get; set; }
}

public class WerklijstResponse
{
    public int Count { get; init; }
    public string? Next { get; init; }
    public string? Previous { get; init; }
    public List<WerklijstOverzichtItem> Results { get; init; } = [];
}

public record WerklijstOverzichtItem
{
    public string Url { get; set; } = string.Empty;
    public string Uuid { get; set; } = string.Empty;
    public string? Nummer { get; set; }
    public string? Status { get; set; }
    public string? Onderwerp { get; set; }
    public string? Kanaal { get; set; }
    public DateTimeOffset? PlaatsgevondenOp { get; set; }
    public string? Afdeling { get; set; }
    public string? Groep { get; set; }
    public string? Medewerker { get; set; }
}
