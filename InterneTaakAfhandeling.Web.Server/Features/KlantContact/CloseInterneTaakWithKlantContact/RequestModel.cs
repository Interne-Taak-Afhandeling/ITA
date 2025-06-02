using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CloseInterneTaakWithKlantContact
{
    public class RequestModel
    {
        public required KlantcontactRequest KlantcontactRequest { get; set; }
        public required Guid AanleidinggevendKlantcontactUuid { get; set; }
        public Guid? PartijUuid { get; set; }
        public required Guid InterneTaakId { get; set; }

    }
}
