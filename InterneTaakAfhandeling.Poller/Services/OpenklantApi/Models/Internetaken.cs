using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Poller.Services.Openklant.Models
{

    public class Internetaken
    {
        public required string Uuid { get; set; }

        public required string Url { get; set; }

        public required string Nummer { get; set; }

        public required string GevraagdeHandeling { get; set; }

        public Klantcontact? AanleidinggevendKlantcontact { get; set; }

        public Actor? ToegewezenAanActor { get; set; }

        public List<Actor>? ToegewezenAanActoren { get; set; }

        public string? Toelichting { get; set; }

        public required string Status { get; set; }

        public required DateTime ToegewezenOp { get; set; }

        public DateTime? AfgehandeldOp { get; set; }
         
        public Internetaken()
        {
            Uuid = string.Empty;
            Url = string.Empty;
            Nummer = string.Empty;
            GevraagdeHandeling = string.Empty;
            Status = string.Empty;
            ToegewezenOp = DateTime.UtcNow;
        }
    }
}
