


using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
 
    public class Internetaak
    {
        public required string Uuid { get; set; }

        public required string Url { get; set; }

        public  string? Nummer { get; set; }

        public  string? GevraagdeHandeling { get; set; }

        public Klantcontact AanleidinggevendKlantcontact { get; set; }

        public Actor? ToegewezenAanActor { get; set; }

        public List<Actor>? ToegewezenAanActoren { get; set; }

        public string? Toelichting { get; set; }

        public string? Status { get; set; }

        public DateTimeOffset? ToegewezenOp { get; set; }

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


    public class InternetakenUpdateRequest
    {
        public required string Nummer { get; set; }

        public required string GevraagdeHandeling { get; set; }

        public required UuidObject AanleidinggevendKlantcontact { get; set; }

        public required List<UuidObject> ToegewezenAanActoren { get; set; }
         
        public required string Toelichting { get; set; }

        public required string Status { get; set; }
         
    }


    


    public class InternetakenPatchRequest
    {
        public  string? Status { get; set; }
    }

    public class UuidObject
    {
        public Guid Uuid { get; set; }
    }


}
