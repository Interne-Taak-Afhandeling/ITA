using System;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
    public class PartijReference
    {
        public required Guid Uuid { get; set; }
    }

    public class BetrokkeneRequest
    {
        public required PartijReference WasPartij { get; set; }
        public required KlantcontactReference HadKlantcontact { get; set; }
        public string Rol { get; set; } = "klant";
        public bool Initiator { get; set; } = true;
    }
}
