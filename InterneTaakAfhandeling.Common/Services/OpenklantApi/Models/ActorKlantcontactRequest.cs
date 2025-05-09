namespace InterneTaakAfhandeling.Web.Server.Services.OpenKlantApi.Models
{
    public class ActorKlantcontactRequest
    {
        public required ActorReference Actor { get; set; }
        public required KlantcontactReference Klantcontact { get; set; }
    }

    public class ActorReference
    {
        public required string Uuid { get; set; }
    }

    public class KlantcontactReference
    {
        public required string Uuid { get; set; }
    }

    public class ActorKlantcontact
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public required ActorReference Actor { get; set; }
        public required KlantcontactReference Klantcontact { get; set; }
    }
}