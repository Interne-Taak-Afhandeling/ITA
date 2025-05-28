using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CloseInterneTaakWithKlantContact
{
    public class RequestModel
    {
        public required KlantcontactRequest KlantcontactRequest { get; set; }
        public required string AanleidinggevendKlantcontactUuid { get; set; }
        public string? PartijUuid { get; set; }
        public required string InterneTaakId { get; set; }

    }
}
