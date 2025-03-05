using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ITA.Poller.Services.Openklant.Models
{
    public class Klantcontact
    {
        public string Uuid { get; set; }
        public string Url { get; set; }
        public List<object> GingOverOnderwerpobjecten { get; set; }
        public List<Actor> HadBetrokkenActoren { get; set; }
        public List<object> OmvatteBijlagen { get; set; }
        public List<Betrokkene> HadBetrokkenen { get; set; }
        public List<Internetaak> LeiddeTotInterneTaken { get; set; }
        public string Nummer { get; set; }
    }

    public class Actor
    {
        public string Uuid { get; set; }
        public string Url { get; set; }
        public string Naam { get; set; }
        public string SoortActor { get; set; }
        public bool IndicatieActief { get; set; }
        public Actoridentificator Actoridentificator { get; set; }
        public object ActorIdentificatie { get; set; }
    }

    public class Actoridentificator
    {
        public string ObjectId { get; set; }
        public string CodeObjecttype { get; set; }
        public string CodeRegister { get; set; }
        public string CodeSoortObjectId { get; set; }
    }

    public class Betrokkene
    {
        public string Uuid { get; set; }
        public string Url { get; set; }
    }

    public class Internetaak
    {
        public string Uuid { get; set; }
        public string Url { get; set; }
    }

}