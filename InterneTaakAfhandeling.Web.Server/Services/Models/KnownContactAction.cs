namespace InterneTaakAfhandeling.Web.Server.Services.Models;

public class KnownContactAction
{
    public static readonly KnownContactAction Completed = new()
    {
        Key = "Completed",
        Description = "afgerond",
        Title = "Logboek contactverzoek"
    };

    public static readonly KnownContactAction CaseLinked = new()
    {
        Key = "CaseLinked",
        Description = "zaak gekoppel",
        Title = "Logboek contactverzoek"
    };

    public static readonly KnownContactAction CaseModified = new()
    {
        Key = "CaseModified",
        Description = "zaak gewijzigd",
        Title = "Logboek contactverzoek"
    };

    public static readonly KnownContactAction AssignedToSelf = new()
    {
        Key = "AssignedToSelf",
        Description = "opgepakt",
        Title = "Logboek contactverzoek"
    };

    public static readonly KnownContactAction ContactSuccessful = new()
    {
        Key = "ContactSuccessful",
        Description = "contact successful",
        Title = "Logboek contactverzoek"
    };


    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; } 


}