using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Services.OpenklantApi.Models
{
    public class CreateRelatedKlantcontactRequest
    {
        public required KlantcontactRequest KlantcontactRequest { get; set; }
        public string? PreviousKlantcontactUuid { get; set; }
    }

    public class RelatedKlantcontactResult
    {
        public required Klantcontact Klantcontact { get; set; }
        public required ActorKlantcontact ActorKlantcontact { get; set; }
        public Onderwerpobject? Onderwerpobject { get; set; }
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
    }

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

    public class ActorRequest
    {
        public required string Naam { get; set; }
        public required string SoortActor { get; set; } = "medewerker";
        public bool IndicatieActief { get; set; } = true;
        public required ActorIdentificator Actoridentificator { get; set; }
        public ActorIdentificatie? ActorIdentificatie { get; set; }
    }

    public class ActorIdentificatie
    {
        public string? Functie { get; set; }
        public string? Emailadres { get; set; }
        public string? Telefoonnummer { get; set; }
    }
    public class ActorIdentificator
    {
        public required string ObjectId { get; set; }
        public required string CodeObjecttype { get; set; }
        public required string CodeRegister { get; set; }
        public required string CodeSoortObjectId { get; set; }
    }

}
