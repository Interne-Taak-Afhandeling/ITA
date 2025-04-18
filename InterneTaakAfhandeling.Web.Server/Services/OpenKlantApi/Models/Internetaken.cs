using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models
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

        public required DateTimeOffset ToegewezenOp { get; set; }

        public DateTimeOffset? AfgehandeldOp { get; set; }

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

    public class AssignedInternetaken { 
    
        public DateTimeOffset? Datum { get; set; }
        public string? Naam { get; set; }
        public string? Onderwerp { get; set; }
    }
}
