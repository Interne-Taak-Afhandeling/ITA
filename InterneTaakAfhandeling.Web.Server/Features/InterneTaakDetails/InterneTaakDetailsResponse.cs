using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Web.Server.Features.Internetaken;

public class InterneTaakDetailsResponse
{
    public required string Uuid { get; init; }
    public string? Nummer { get; init; }
    public string? GevraagdeHandeling { get; init; }
    public Klantcontact? AanleidinggevendKlantcontact { get; init; }
    public List<Actor>? ToegewezenAanActoren { get; init; }
    public string? Toelichting { get; init; }
    public string? Status { get; init; }
    public DateTimeOffset? ToegewezenOp { get; init; }
    public DateTimeOffset? AfgehandeldOp { get; init; }
    public Zaak? Zaak { get; init; }

    // Pre-resolved actor display fields
    public string? BehandelaarNaam { get; init; }
    public string? OrganisatorischeEenheidNaam { get; init; }
    public string? OrganisatorischeEenheidType { get; init; }

    // Pre-resolved contact display fields (replaces client-side processing in ContactmomentDetails)
    public string? KlantNaam { get; init; }
    public string? Organisatienaam { get; init; }
    public string? Email { get; init; }
    public TelefoonnummerItem? Telefoonnummer1 { get; init; }
    public TelefoonnummerItem? Telefoonnummer2 { get; init; }
    public DateTimeOffset? PlaatsgevondenOp { get; init; }
    public string? Kanaal { get; init; }
    public string? AangemaaktDoor { get; init; }
}

public class TelefoonnummerItem
{
    public required string Adres { get; init; }
    public required string Omschrijving { get; init; }
}
