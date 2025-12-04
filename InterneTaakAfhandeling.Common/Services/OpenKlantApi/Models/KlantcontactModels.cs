using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
    public class KlantcontactOnderwerpobjectRequest
    {
        public KlantcontactReference? Klantcontact { get; set; }
        public KlantcontactReference? WasKlantcontact { get; set; }
        public required Onderwerpobjectidentificator Onderwerpobjectidentificator { get; set; }
    }

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
        public string? Klantnaam { get; set; }
    }

    public class KlantcontactReference
    {
        public required Guid Uuid { get; set; }
    }

    public class KlantcontactenResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Klantcontact> Results { get; set; } = new List<Klantcontact>();
    }
}
