using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models
{
    public class KlantcontactRequest
    {

        public string? Kanaal { get; set; }

        public string? Onderwerp { get; set; }

        public string? Inhoud { get; set; }

        public bool? IndicatieContactGelukt { get; set; }

        public string? Taal { get; set; }

        public bool? Vertrouwelijk { get; set; }

        [Required]
        public DateTimeOffset PlaatsgevondenOp { get; set; }
    }
}