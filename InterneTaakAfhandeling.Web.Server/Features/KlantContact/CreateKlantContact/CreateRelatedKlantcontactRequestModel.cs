using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.KlantContact.CreateKlantContact
{
    public class CreateRelatedKlantcontactRequestModel
    {
        public required KlantcontactRequest KlantcontactRequest { get; set; }
        public Guid AanleidinggevendKlantcontactUuid { get; set; }
        public Guid? PartijUuid { get; set; }
        public required Guid InterneTaakId { get; set; }
        public string? InterneNotitie { get; set; }
    }
}