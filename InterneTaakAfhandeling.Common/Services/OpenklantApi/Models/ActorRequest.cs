namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{
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