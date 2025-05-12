


using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{

    public class Internetaken
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
         
        public Internetaken()
        {
            Uuid = string.Empty;
            Url = string.Empty;
            Nummer = string.Empty;
            GevraagdeHandeling = string.Empty;
            Status = string.Empty;
            ToegewezenOp = DateTimeOffset.UtcNow;
        }
    }
}
