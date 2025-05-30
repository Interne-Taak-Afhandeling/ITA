 
using System.Text; 
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using InterneTaakAfhandeling.Common.Services.ZakenApi.Models;

namespace InterneTaakAfhandeling.Poller.Services.Emailservices.Content;

public interface IEmailContentService
{
    string BuildInternetakenEmailContent(Internetaak internetaken, Klantcontact klantcontact, List<DigitaleAdres>? digitaleAdressen,Zaak? zaak);
}

public class EmailContentService : IEmailContentService
{
    private const string EmailTemplate = @"<html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; line-height: 1.6; }
            dl { margin: 0; padding: 0; }
            dt { font-weight: bold; }
            dd { margin: 0 0 10px 0; padding: 0 0 0 10px; }
            .preserve-newline { white-space: pre-wrap; }
        </style>
    </head>
    <body>
        <dl>
             <dt>Starttijd</dt><dd>{Starttijd}</dd>
             <dt>Toelichting voor de collega</dt><dd class='preserve-newline'>{Toelichting}</dd>
             <dt>Status</dt><dd>  {Status} </dd>
             <dt>Zaaknummer</dt><dd> {Zaak}</dd>
             <dt>Naam betrokkene</dt><dd>  {BetrokkeneNaam} </dd> 
             <dt>Gevraagde handeling</dt><dd>  {GevraagdeHandeling} </dd> 
            {DigitaleAdressen}
            <dt>Aangemaakt door</dt><dd>  {AangemaaktDoor} </dd>
            <dt>Vraag</dt><dd> {Vraag} </dd>
            <dt>Toelichting</dt><dd> {Inhoud} </dd>
        </dl>
    </body>
    </html>";

    public string BuildInternetakenEmailContent(Internetaak internetaken, Klantcontact klantcontact, List<DigitaleAdres>? digitaleAdressen, Zaak? zaak)
    {
        var betrokkene = klantcontact.HadBetrokkenActoren.FirstOrDefault();
        
        var sb = new StringBuilder(EmailTemplate);
        sb.Replace("{Starttijd}", FormatPlaatsgevondenOp(klantcontact.PlaatsgevondenOp))
          .Replace("{Toelichting}", internetaken.Toelichting ?? "N/A")
          .Replace("{Status}", internetaken.Status ?? "N/A")
          .Replace("{GevraagdeHandeling}", internetaken.GevraagdeHandeling ?? "N/A")
          .Replace("{Inhoud}", klantcontact.Inhoud != null ? $"{klantcontact.Inhoud}</dd>" : "")
          .Replace("{Vraag}", klantcontact.Onderwerp != null ? klantcontact.Onderwerp : "")
          .Replace("{AangemaaktDoor}", betrokkene?.Naam != null ? betrokkene.Naam : "")
          .Replace("{DigitaleAdressen}", digitaleAdressen != null ? BuildDigitaleAdressen(digitaleAdressen) : "" )
          .Replace("{BetrokkeneNaam}", FullName(klantcontact.Expand?.HadBetrokkenen?.FirstOrDefault()?.Contactnaam))
          .Replace("{Zaak}", betrokkene?.Naam != null ? zaak?.Identificatie : "");

        return sb.ToString();
    }

    private string BuildDigitaleAdressen(List<DigitaleAdres> digitaleAdressen)
    {
        var sb = new StringBuilder();
        foreach (var adres in digitaleAdressen)
        {
            sb.Append($"<dt>{adres.Omschrijving ?? adres.SoortDigitaalAdres ?? "contact"}</dt><dd>{adres.Adres}</dd>");
        }
        return sb.ToString();
    }
    public static string FullName(Contactnaam? contact)
    {
        if(contact == null)
        {
            return string.Empty;
        }
        return string.Join(" ", new[] { contact.Voornaam ?? contact.Voorletters, contact.VoorvoegselAchternaam, contact.Achternaam }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public static string FormatPlaatsgevondenOp(DateTimeOffset plaatsgevondenOp)
    { 
         
        TimeZoneInfo dutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
        DateTimeOffset dutchTime = TimeZoneInfo.ConvertTime(plaatsgevondenOp, dutchTimeZone);

        return dutchTime.ToString("HH:mm");
    }
}
