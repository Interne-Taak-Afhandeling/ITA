using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CreateKlantContact
{
    public class CreateRelatedKlantcontactRequestModel
    {
        public required KlantcontactRequest KlantcontactRequest { get; set; }
        public string? AanleidinggevendKlantcontactUuid { get; set; }
        public string? PartijUuid { get; set; }
    }
}
