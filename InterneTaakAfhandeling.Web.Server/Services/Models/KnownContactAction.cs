using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Features.KlantContact;

namespace InterneTaakAfhandeling.Web.Server.Services.Models;

public class KnownContactAction
{
    public required string Type { get; init; }
    public required string Description { get; init; }

    public ActiviteitActor Actor { get; init; }
    public ObjectIdentificator? HeeftBetrekkingOp { get; private init; }

    public static KnownContactAction Completed()
    {
        return new KnownContactAction
        {
            Description = "afgerond",
            Type = "verwerkt"
        };
    }

    public static KnownContactAction CaseLinked(Guid zaakId, string userId)
    {
        return new KnownContactAction
        {
            Description = "zaak gekoppeld",
            Type = "zaak-gekoppeld",
            Actor = new ActiviteitActor()
            {
                CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                ObjectId = userId,
            },
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeObjecttype = "zgw-Zaak",
                CodeRegister = "openzaak",
                CodeSoortObjectId = "uuid",
                ObjectId = zaakId.ToString()
            }
        };
    }

    public static KnownContactAction CaseModified(Guid zaakId, string userId)
    {
        return new KnownContactAction
        {
            Description = "zaak gewijzigd",
            Type = "zaakkoppeling-gewijzigd",
            Actor = new ActiviteitActor()
            {
                CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                ObjectId = userId,
            },
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
            Actor = new ActiviteitActor()
            {
                CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                ObjectId = actorId.ToString(),
            },
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "actor",
                CodeSoortObjectId = "uuid",
                ObjectId = actorId.ToString()
            }
        };
    }

    public static KnownContactAction Klantcontact(RelatedKlantcontactResult relatedKlantcontactResult)
    {
        var description = relatedKlantcontactResult.Klantcontact.IndicatieContactGelukt.HasValue && relatedKlantcontactResult.Klantcontact.IndicatieContactGelukt.Value
            ? "contact gehad"
            : "geen contact kunnen leggen";

        return new KnownContactAction
        {
            Description = description,
            Type = "klantcontact",
            Actor = new ActiviteitActor()
            {
                CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                ObjectId = relatedKlantcontactResult.ActorKlantcontact.Uuid,
            },
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "klantcontact",
                CodeSoortObjectId = "uuid",
                ObjectId = relatedKlantcontactResult.Klantcontact.Uuid.ToString()
            }
        };
    }
}