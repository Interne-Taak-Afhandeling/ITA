using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using System.Text;

namespace InterneTaakAfhandeling.Common.Services.Emailservices.Content;



public interface IEmailContentService
{
    string BuildInternetakenEmailContent(InterneTakenEmailInput input, string itaBaseUrl);
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
        <p>Er is een nieuw contactverzoek voor jou.</p>
        <p>Bekijk <a href=""{Link}"">contactverzoek {Nummer} in ITA Contactverzoeken</a>.</p>
    </body>
    </html>";

    public string BuildInternetakenEmailContent(InterneTakenEmailInput input, string itaBaseUrl)
    {
        var (internetaken, _, _, _) = input;

        var deeplink = $"{itaBaseUrl.TrimEnd('/')}/contactverzoek/{internetaken.Nummer}";

        var sb = new StringBuilder(EmailTemplate);
        sb.Replace("{Link}", deeplink)
          .Replace("{Nummer}", internetaken.Nummer);

        return sb.ToString();
    }

}
