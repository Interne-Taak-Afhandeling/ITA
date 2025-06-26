namespace InterneTaakAfhandeling.Web.Server.Services.Models;

public class KnownContactAction
{
    public static readonly KnownContactAction Completed = new()
    {
        Description = "afgerond",
        Type = "verwerkt",
        CodeRegister = "openklant",
        CodeObjectType ="internetaak",
        CodeSoortObjectId = "uuid"
    };

    public static readonly KnownContactAction CaseLinked = new()
    {
        Description = "zaak gekoppeld",
        Type = "zaak-gekoppeld",
        CodeObjectType = "zgw-Zaak",
        CodeRegister = "openzaak",
        CodeSoortObjectId = "uuid"
    };

    public static readonly KnownContactAction CaseModified = new()
    {
        Description = "zaak gewijzigd",
        Type = "zaakkoppeling-gewijzigd",
        CodeRegister = "openklant",
        CodeObjectType ="internetaak",
        CodeSoortObjectId = "uuid"
    };

    public static readonly KnownContactAction AssignedToSelf = new()
    {
        Description = "opgepakt",
        Type = "toegewezen",
        CodeRegister = "openklant",
        CodeObjectType ="internetaak",
        CodeSoortObjectId = "uuid"
    };
    public static readonly KnownContactAction Klantcontact = new()
    {
        Description = "geen contact kunnen leggen",
        Type = "klantcontact",
        CodeRegister = "openklant",
        CodeObjectType ="klantcontact",
        CodeSoortObjectId = "uuid"
    };

    public KnownContactAction WithDescription(string newDescription) =>
        new KnownContactAction
        {
            Type = this.Type,
            Description = newDescription,
            CodeRegister = this.CodeRegister,
            CodeObjectType = this.CodeObjectType,
            CodeSoortObjectId = this.CodeSoortObjectId
        };
    public required string Type { get; init; }
    public required string Description { get; init; }
    public required string CodeRegister { get; init; }
    public required string CodeObjectType { get; init; }
    public required string CodeSoortObjectId { get; init; }
}