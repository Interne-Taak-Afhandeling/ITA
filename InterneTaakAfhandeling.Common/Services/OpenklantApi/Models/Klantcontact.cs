using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models
{

    public class Klantcontact
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public List<Onderwerpobject>? GingOverOnderwerpobjecten { get; set; }
        public List<Actor> HadBetrokkenActoren { get; set; }
        public List<object>? OmvatteBijlagen { get; set; }
        public List<Betrokkene> HadBetrokkenen { get; set; }
        public List<Internetaak>? LeiddeTotInterneTaken { get; set; }
        public string? Nummer { get; set; }
        public string? Kanaal { get; set; }
        public string? Onderwerp { get; set; }
        public string? Inhoud { get; set; }
        public bool? IndicatieContactGelukt { get; set; }
        public string? Taal { get; set; }
        public bool? Vertrouwelijk { get; set; }
        public DateTimeOffset PlaatsgevondenOp { get; set; }
        [JsonPropertyName("_expand")]
        public Expand? Expand { get; set; }
    }

    public class Onderwerpobject
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public Klantcontact? Klantcontact { get; set; }
        public object? WasKlantcontact { get; set; }
        public Onderwerpobjectidentificator? Onderwerpobjectidentificator { get; set; }
    }

    public class Onderwerpobjectidentificator
    {
        public required string ObjectId { get; set; }
        public required string CodeObjecttype { get; set; }
        public required string CodeRegister { get; set; }
        public required string CodeSoortObjectId { get; set; }
    }
    public class ActorResponse
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<Actor> Results { get; set; } = new();
    }
    public class Actor
    {
        public required string Uuid { get; set; }
        public  string? Url { get; set; }
        public string? Naam { get; set; }
        public string? SoortActor { get; set; }
        public bool? IndicatieActief { get; set; }
        public Actoridentificator? Actoridentificator { get; set; }
        public object? ActorIdentificatie { get; set; }
    }

    public class Actoridentificator
    {
        public required string ObjectId { get; set; }
        public required string CodeObjecttype { get; set; }
        public required string CodeRegister { get; set; }
        public required string CodeSoortObjectId { get; set; }
    }

    public class Internetaak
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public string? Nummer { get; set; }
        public string? GevraagdeHandeling { get; set; }
        public Klantcontact? AanleidinggevendKlantcontact { get; set; }
        public Actor? ToegewezenAanActor { get; set; }
        public List<Actor>? ToegewezenAanActoren { get; set; }
        public string? Toelichting { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? ToegewezenOp { get; set; }
        public DateTimeOffset? AfgehandeldOp { get; set; }
    }

    public class DigitaleAdres
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public Betrokkene? VerstrektDoorBetrokkene { get; set; }
        public object? VerstrektDoorPartij { get; set; }
        public string? Adres { get; set; }
        public string? SoortDigitaalAdres { get; set; }
        public bool IsStandaardAdres { get; set; }
        public string? Omschrijving { get; set; }
    }

    public class Betrokkene
    {
        public required string Uuid { get; set; }
        public required string Url { get; set; }
        public object? WasPartij { get; set; }
        public Klantcontact? HadKlantcontact { get; set; }
        public List<DigitaleAdres> DigitaleAdressen { get; set; }
        public Adres? Bezoekadres { get; set; }
        public Adres? Correspondentieadres { get; set; }
        public Contactnaam? Contactnaam { get; set; }
        public string? VolledigeNaam { get; set; }
        public string? Rol { get; set; }
        public string? Organisatienaam { get; set; }
        public bool Initiator { get; set; }
        [JsonPropertyName("_expand")]
        public BetrokkeneExpand? Expand { get; set; }
    }

    public class Adres
    {
        public string NummeraanduidingId { get; set; }
        public string Adresregel1 { get; set; }
        public string Adresregel2 { get; set; }
        public string Adresregel3 { get; set; }
        public string Land { get; set; }
    }

    public class Contactnaam
    {
        public string Voorletters { get; set; }
        public string Voornaam { get; set; }
        public string VoorvoegselAchternaam { get; set; }
        public string Achternaam { get; set; }
    }

    public class Expand
    {
        public List<Onderwerpobject>? GingOverOnderwerpobjecten { get; set; }
        public List<Betrokkene>? HadBetrokkenen { get; set; }
        public List<Internetaak>? LeiddeTotInterneTaken { get; set; }
    }

    public class BetrokkeneExpand
    {
        public List<DigitaleAdres>? DigitaleAdressen { get; set; }
    }
}
