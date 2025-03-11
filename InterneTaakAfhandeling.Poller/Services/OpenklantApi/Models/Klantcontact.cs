using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using InterneTaakAfhandeling.Poller.Services.Emailservices.SmtpMailService;

namespace InterneTaakAfhandeling.Poller.Services.Openklant.Models
{
    public class Klantcontact
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public  List<object>? GingOverOnderwerpobjecten { get; set; }
        public  List<Actor>? HadBetrokkenActoren { get; set; }
        public  List<object>? OmvatteBijlagen { get; set; }
        public  List<Betrokkene>? HadBetrokkenen { get; set; }
        public  List<Internetaak>? LeiddeTotInterneTaken { get; set; }
        public  string? Nummer { get; set; }
    }

    public class Actor
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public  string? Naam { get; set; }
        public  string? SoortActor { get; set; }
        public  bool? IndicatieActief { get; set; }
        public  Actoridentificator? Actoridentificator { get; set; }
        public  object? ActorIdentificatie { get; set; }

       
        // email validation added for by passing data mapping issue (email address with CodeRegister obj), in the future it needs to be removed 
 


    }

    public class Actoridentificator
    {
        public required string ObjectId { get; set; }
        public required string CodeObjecttype { get; set; }
        public required string CodeRegister { get; set; }
        public required string CodeSoortObjectId { get; set; }

      
    }

    public class Betrokkene
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
    }

    public class Internetaak
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
    }

}