using System.Net;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using System.Text;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.Content;



public interface IEmailContentService
{
    string BuildInternetakenEmailContent(Internetaak internetaak, string itaBaseUrl);
}

public class EmailContentService : IEmailContentService
{
    private const string EmailTemplate = @"<html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; line-height: 1.6; }
            a { color: #0066cc; text-decoration: none; }
            a:hover { text-decoration: underline; }
        </style>
    </head>
    <body>
        <p>Beste collega,</p>
        <p>Er is een vraag of verzoek binnengekomen van een inwoner of ondernemer.<br/>Je kunt het verzoek bekijken en afhandelen via onderstaande link:</p>
        <p><a href=""{Link}"">Contactverzoek {Nummer}</a></p>
        <p>We verzoeken je om dit zo spoedig mogelijk op te pakken.</p>
        <p>Met vriendelijke groet,<br/>KCC/Klantcontactcentrum</p>
    </body>
    </html>";

    public string BuildInternetakenEmailContent(Internetaak internetaak, string itaBaseUrl)
    {
        var contactmomentNummer = internetaak.AanleidinggevendKlantcontact?.Nummer
            ?? throw new InvalidOperationException($"AanleidinggevendKlantcontact.Nummer ontbreekt voor internetaak {internetaak.Nummer}");

        var deeplink = $"{itaBaseUrl.TrimEnd('/')}/contactmoment/{Uri.EscapeDataString(contactmomentNummer)}";

        var sb = new StringBuilder(EmailTemplate);
        sb.Replace("{Link}", WebUtility.HtmlEncode(deeplink))
          .Replace("{Nummer}", WebUtility.HtmlEncode(contactmomentNummer));

        return sb.ToString();
    }

}
