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
        public List<DigitaleAdres>? DigitaleAdress { get { return AanleidinggevendKlantcontact?.Expand?.HadBetrokkenen?.SelectMany(x => x?.Expand?.DigitaleAdressen).ToList(); } }
        public Actor? Betrokken { get { return AanleidinggevendKlantcontact?.HadBetrokkenActoren.FirstOrDefault(); }   }

         
    }

   
}
