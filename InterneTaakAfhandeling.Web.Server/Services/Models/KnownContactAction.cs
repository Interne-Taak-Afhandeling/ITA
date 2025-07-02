using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Services.Models;

public class KnownContactAction
{
    public required string Type { get; init; }
    public required string Description { get; init; }
    public ObjectIdentificator? HeeftBetrekkingOp { get; private init; }
    public List<ObjectIdentificator> AlleObjecten { get; private init; } = new();

    public static KnownContactAction Completed()
    {
        return new KnownContactAction
        {
            Description = "afgerond",
            Type = "verwerkt"
        };
    }

    public static KnownContactAction CaseLinked(Guid zaakId, Guid? actorId = null)
    {
        var action = new KnownContactAction
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

        // Voeg actor toe als tweede object
        if (actorId.HasValue)
        {
            action.AlleObjecten.Add(new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "actor",
                CodeSoortObjectId = "uuid",
                ObjectId = actorId.Value.ToString()
            });
        }

        return action;
    }

    public static KnownContactAction CaseModified(Guid zaakId, Guid? actorId = null)
    {
        var action = new KnownContactAction
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

        // Voeg actor toe als tweede object
        if (actorId.HasValue)
        {
            action.AlleObjecten.Add(new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "actor",
                CodeSoortObjectId = "uuid",
                ObjectId = actorId.Value.ToString()
            });
        }

        return action;
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
                CodeObjecttype = "actor",
                CodeSoortObjectId = "uuid",
                ObjectId = actorId.ToString()
            }
        };
    }

    public static KnownContactAction Klantcontact(Klantcontact klantContact)
    {
        var description = klantContact.IndicatieContactGelukt.HasValue && klantContact.IndicatieContactGelukt.Value
            ? "contact gehad"
            : "geen contact kunnen leggen";

        return new KnownContactAction
        {
            Description = description,
            Type = "klantcontact",
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "klantcontact",
                CodeSoortObjectId = "uuid",
                ObjectId = klantContact.Uuid.ToString()
            }
        };
    }
}