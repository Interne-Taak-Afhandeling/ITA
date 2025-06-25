namespace InterneTaakAfhandeling.Web.Server.Services.Models;

public class KnownContactAction
{
    public static readonly KnownContactAction Completed = new()
    {
        Description = "afgerond",
        Type = "verwerkt"
    };

    public static readonly KnownContactAction CaseLinked = new()
    {
        Description = "zaak gekoppeld",
        Type = "zaak-gekoppeld"
    };

    public static readonly KnownContactAction CaseModified = new()
    {
        Description = "zaak gewijzigd",
        Type = "zaakkoppeling-gewijzigd"
    };

    public static readonly KnownContactAction AssignedToSelf = new()
    {
        Description = "opgepakt",
        Type = "toegewezen"
    };


    public required string Type { get; init; }
    public required string Description { get; init; }
}