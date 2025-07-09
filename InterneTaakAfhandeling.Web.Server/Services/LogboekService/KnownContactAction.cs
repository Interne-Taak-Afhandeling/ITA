using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Features.KlantContact;

namespace InterneTaakAfhandeling.Web.Server.Services.LogboekService;

public class KnownContactAction
{
    public required string Type { get; init; }
    public required string Description { get; init; }

    public required ActiviteitActor Actor { get; init; }
    public ObjectIdentificator? HeeftBetrekkingOp { get; private init; }

    public static KnownContactAction Completed(ITAUser loggedByUser)
    {
        return new KnownContactAction
        {
            Description = "afgerond",
            Type = "verwerkt",
            Actor = CreateActor(loggedByUser),
        };
    }

    public static KnownContactAction CaseLinked(Guid zaakId, ITAUser loggedByUser)
    {
        return new KnownContactAction
        {
            Description = "zaak gekoppeld",
            Type = "zaak-gekoppeld",
            Actor = CreateActor(loggedByUser),
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeObjecttype = "zgw-Zaak",
                CodeRegister = "openzaak",
                CodeSoortObjectId = "uuid",
                ObjectId = zaakId.ToString()
            }
        };
    }

    public static KnownContactAction CaseModified(Guid zaakId, ITAUser loggedByUser)
    {
        return new KnownContactAction
        {
            Description = "zaak gewijzigd",
            Type = "zaakkoppeling-gewijzigd",
            Actor = CreateActor(loggedByUser),
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openzaak",
                CodeObjecttype = "zgw-Zaak",
                CodeSoortObjectId = "uuid",
                ObjectId = zaakId.ToString()
            }
        };
    }

    public static KnownContactAction AssignedToSelf(string actorId, ITAUser loggedByUser)
    {
        return new KnownContactAction
        {
            Description = "opgepakt",
            Type = "toegewezen",
            Actor = CreateActor(loggedByUser),
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "actor",
                CodeSoortObjectId = "uuid",
                ObjectId = actorId
            }
        };
    }

    public static KnownContactAction Klantcontact(RelatedKlantcontactResult relatedKlantcontactResult, ITAUser loggedByUser)
    {
        var description = relatedKlantcontactResult.Klantcontact.IndicatieContactGelukt.HasValue && relatedKlantcontactResult.Klantcontact.IndicatieContactGelukt.Value
            ? "contact gehad"
            : "geen contact kunnen leggen";

        return new KnownContactAction
        {
            Description = description,
            Type = "klantcontact",
            Actor = CreateActor(loggedByUser),
            HeeftBetrekkingOp = new ObjectIdentificator
            {
                CodeRegister = "openklant",
                CodeObjecttype = "klantcontact",
                CodeSoortObjectId = "uuid",
                ObjectId = relatedKlantcontactResult.Klantcontact.Uuid.ToString()
            }
        };
    }


    private static ActiviteitActor CreateActor(ITAUser loggedByUser)
    {
        return new ActiviteitActor()
        {
            Naam = loggedByUser.Name,
            Actoridentificator = new ActiviteitActoridentificator
            {
                CodeObjecttype = KnownMedewerkerIdentificators.EmailFromEntraId.CodeObjecttype,
                CodeSoortObjectId = KnownMedewerkerIdentificators.EmailFromEntraId.CodeSoortObjectId,
                CodeRegister = KnownMedewerkerIdentificators.EmailFromEntraId.CodeRegister,
                ObjectId = loggedByUser.Email,
            }
        };
    }


}