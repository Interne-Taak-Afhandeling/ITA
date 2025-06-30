using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Services.Models;

public class KnownContactAction
{
    public required string Type { get; init; }
    public required string Description { get; init; }
    public ObjectIdentificator? HeeftBetrekkingOp { get; private init; }

    public static KnownContactAction Completed()
    {
        return new KnownContactAction
        {
            Description = "afgerond",
            Type = "verwerkt"
        };
    }

    public static KnownContactAction CaseLinked(Guid zaakId)
    {
        return new KnownContactAction
        {
            Description = "zaak gekoppeld",
            Type = "zaak-gekoppeld",
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeObjecttype = "zgw-Zaak",
                CodeRegister = "openzaak",
                CodeSoortObjectId = "uuid",
                ObjectId = zaakId.ToString()
            }
        };
    }

    public static KnownContactAction CaseModified(Guid zaakId)
    {
        return new KnownContactAction
        {
            Description = "zaak gewijzigd",
            Type = "zaakkoppeling-gewijzigd",
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openzaak",
                CodeObjecttype = "zgw-Zaak",
                CodeSoortObjectId = "uuid",
                ObjectId = zaakId.ToString()
            }
        };
    }

    public static KnownContactAction AssignedToSelf(Guid actorId)
    {
        return new KnownContactAction
        {
            Description = "opgepakt",
            Type = "toegewezen",
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "internetaak",
                CodeSoortObjectId = "uuid",
                ObjectId = actorId.ToString()
            }
        };
    }

    public static KnownContactAction Klantcontact(Guid klantContactId, string description)
    {
        return new KnownContactAction
        {
            Description = description,
            Type = "klantcontact",
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "klantcontact",
                CodeSoortObjectId = "uuid",
                ObjectId = klantContactId.ToString()
            }
        };
    }
}