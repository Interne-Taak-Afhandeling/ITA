
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
    public class InterneTaakQuery
    {
        public string? AanleidinggevendKlantcontact_Url { get; set; }
        public Guid? AanleidinggevendKlantcontact_Uuid { get; set; }

        public string? Actoren__Naam { get; set; }
        public string? Klantcontact__Nummer { get; set; }
        public Guid? Klantcontact__Uuid { get; set; }

        public string? Nummer { get; set; }

        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public string? Status { get; set; }

        public string? ToegewezenAanActor__Url { get; set; }
        public Guid? ToegewezenAanActor__Uuid { get; set; }

        public DateTime? ToegewezenOp { get; set; }
    }
 

    public class InternetakenResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Internetaak> Results { get; set; } = new();
    }

    public class InternetakenUpdateRequest
    {
        public required string Nummer { get; set; }

        public required string GevraagdeHandeling { get; set; }

        public required UuidObject AanleidinggevendKlantcontact { get; set; }

        public required List<UuidObject> ToegewezenAanActoren { get; set; }

        public required string Toelichting { get; set; }

        public required string Status { get; set; }

    }

    public class Internetaak
    {
        public required string Uuid { get; set; }

        public required string Url { get; set; }

        public required string Nummer { get; set; }

        public required string GevraagdeHandeling { get; set; }

        public Klantcontact AanleidinggevendKlantcontact { get; set; }

        public Actor? ToegewezenAanActor { get; set; }

        public List<Actor>? ToegewezenAanActoren { get; set; }

        public string? Toelichting { get; set; }

        public required string Status { get; set; }

        public required DateTimeOffset ToegewezenOp { get; set; }

        public DateTimeOffset? AfgehandeldOp { get; set; }

        public Zaak? Zaak { get; set; }

        public Internetaak()
        {
            Uuid = string.Empty;
            Url = string.Empty;
            Nummer = string.Empty;
            GevraagdeHandeling = string.Empty;
            Status = string.Empty;
            ToegewezenOp = DateTimeOffset.UtcNow;
        }
    }

    public class UuidObject
    {
        public Guid Uuid { get; set; }
    }
}
