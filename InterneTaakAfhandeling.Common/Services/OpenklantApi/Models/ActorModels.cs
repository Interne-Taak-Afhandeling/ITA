namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
    public class ActorRequest
    {
        public required string Naam { get; set; }
        public required string SoortActor { get; set; } = "medewerker";
        public bool IndicatieActief { get; set; } = true;
        public required Actoridentificator Actoridentificator { get; set; }
    }

    public class ActorReference
    {
        public required string Uuid { get; set; }
    }

    public class ActorKlantcontactRequest
    {
        public required ActorReference Actor { get; set; }
        public required KlantcontactReference Klantcontact { get; set; }
    }

    public class ActorKlantcontact
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public required ActorReference Actor { get; set; }
        public required KlantcontactReference Klantcontact { get; set; }
    }
}