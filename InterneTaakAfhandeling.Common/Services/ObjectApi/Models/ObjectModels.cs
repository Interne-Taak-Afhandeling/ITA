using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi.Models
{
    public class ObjectModels<T>
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public required List<ObjectResult<T>> Results { get; set; }
    }

    public class ObjectResult<T>
    {
        public required string Url { get; set; }
        public required Guid Uuid { get; set; }
        public required string Type { get; set; }
        public required ObjectRecord<T> Record { get; set; }
    }

    public class ObjectRecord<T>
    {
        public int Index { get; set; }
        public int TypeVersion { get; set; }
        public required T Data { get; set; }
        public object? Geometry { get; set; }
        public required DateOnly StartAt { get; set; }
        public string? EndAt { get; set; }
        public  string? RegistrationAt { get; set; }
        public string? CorrectionFor { get; set; }
        public string? CorrectedBy { get; set; }
    }



    public class ObjectPatchModel<T>
    {
        public required Guid Uuid { get; set; }
        public required string Type { get; set; }
        public required ObjectRecord<T> Record { get; set; }
    }


    public class MedewerkerObjectData
    {
        public  List<Emails>? Emails { get; set; }
        public string? Email { get; set; }
        public  string? Skills { get; set; }
        public  string? Functie { get; set; }
        public  List<Groep>? Groepen { get; set; }
        public  string? Voornaam { get; set; }
        public  string? Achternaam { get; set; }
        public  List<Afdeling>? Afdelingen { get; set; }
        public  Vervanging? Vervanging { get; set; }
        public  string? Identificatie { get; set; }
        public  string? VolledigeNaam { get; set; }
        public  List<Telefoonnummers>? Telefoonnummers { get; set; }
        public  string? VoorvoegselAchternaam { get; set; }

        // Helper method to get all emails regardless of format
        [JsonIgnore]
        public List<string> EmailAddresses
        {
            get
            {
                return Email != null ? [Email] :
                       Emails != null ? Emails.Select(e => e.Email).ToList() :
                       [];
            }
        }
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