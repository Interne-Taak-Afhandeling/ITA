using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTakenOverview;


public enum IntertaakStatus
{ 
    TeVerwerken,
    Verwerkt
}

public class MyInterneTakenQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? NaamActeur { get; set; }
    [EnumDataType(typeof(IntertaakStatus))]
    public IntertaakStatus? Status { get; set; }
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
    public string Uuid { get; set; } = string.Empty;                    
    public string Nummer { get; set; } = string.Empty;                   
    public string GevraagdeHandeling { get; set; } = string.Empty;       
    public string Status { get; set; } = string.Empty;                 
    public DateTimeOffset ToegewezenOp { get; set; }                 
    public DateTimeOffset? AfgehandeldOp { get; set; }                 

    public string? Onderwerp { get; set; }                            
    public DateTimeOffset? ContactDatum { get; set; }                  
    public string? KlantNaam { get; set; }

    public string? AfdelingNaam { get; set; }
    public string? BehandelaarNaam { get; set; }
}
