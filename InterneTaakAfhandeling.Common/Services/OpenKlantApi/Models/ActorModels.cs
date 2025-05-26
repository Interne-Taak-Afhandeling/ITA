using InterneTaakAfhandeling.Common.Services.OpenklantApi.Models;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
    public class ActorRequest
    {
        public required string Naam { get; set; }
        public required SoortActor SoortActor { get; set; } = SoortActor.medewerker;
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