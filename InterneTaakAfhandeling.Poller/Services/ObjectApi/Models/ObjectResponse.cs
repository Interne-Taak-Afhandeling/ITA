namespace InterneTaakAfhandeling.Poller.Services.ObjectApi.Models
{
    public class ObjectResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public required List<ObjectResult> Results { get; set; }
    }

    public class ObjectResult
    {
        public required string Url { get; set; }
        public required string Uuid { get; set; }
        public required string Type { get; set; }
        public required ObjectRecord Record { get; set; }
    }

    public class ObjectRecord
    {
        public int Index { get; set; }
        public int TypeVersion { get; set; }
        public required ObjectData Data { get; set; }
        public object? Geometry { get; set; }
        public required string StartAt { get; set; }
        public string? EndAt { get; set; }
        public  string? RegistrationAt { get; set; }
        public string? CorrectionFor { get; set; }
        public string? CorrectedBy { get; set; }
    }

    public class ObjectData
    {
        public required List<Emails> Emails { get; set; }
        public required string Skills { get; set; }
        public required string Functie { get; set; }
        public required List<Groep> Groepen { get; set; }
        public  string? Voornaam { get; set; }
        public required string Achternaam { get; set; }
        public required List<Afdeling> Afdelingen { get; set; }
        public  Vervanging? Vervanging { get; set; }
        public required string Identificatie { get; set; }
        public  string? VolledigeNaam { get; set; }
        public required List<Telefoonnummers> Telefoonnummers { get; set; }
        public required string VoorvoegselAchternaam { get; set; }
    }

    public class Emails
    {
        public required string Naam { get; set; }
        public required string Email { get; set; }
    }

    public class Groep
    {
        public required string Groepsnaam { get; set; }
    }

    public class Afdeling
    {
        public required string Afdelingnaam { get; set; }
    }

    public class Vervanging
    {
        public  string? Name { get; set; }
    }

    public class Telefoonnummers
    {
        public required string Telefoonnummer { get; set; }
    }
}