using System.Net;
using System.Text;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinneringNotificatie;

public sealed class VerlopenContactverzoekHerinneringNotificatieTemplateService
{
    public static string GenereerMailBody(int aantalCvs, int maxWerkdagen, string werkvoorraadUrl)
    {
        var sb = new StringBuilder();
        sb.Append(@"<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; }
        a { color: #0066cc; text-decoration: none; }
        a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <p>Beste collega,</p>
    <p>Er staan <strong>");
        sb.Append(aantalCvs);
        sb.Append(@"</strong> contactverzoeken open die al verlopen zijn. Het langstlopende contactverzoek staat al <strong>");
        sb.Append(maxWerkdagen);
        sb.Append(@"</strong> werkdag");
        if (maxWerkdagen != 1) sb.Append("en");
        sb.Append(@" open.</p>
    <p>Inwoners waarderen een snelle afhandeling van hun verzoeken.</p>
    <p>Neem contact op en handel deze contactverzoeken af.</p>
    <p><a href=""");
        sb.Append(WebUtility.HtmlEncode(werkvoorraadUrl));
        sb.Append(@""">Ga naar je werkvoorraad</a></p>
    <p>Fijne werkdag</p>
</body>
</html>");
        return sb.ToString();
    }

    public static string GenereerMailBodyPlainText(int aantalCvs, int maxWerkdagen, string werkvoorraadUrl)
    {
        var sb = new StringBuilder();
        sb.Append("Beste collega,\n\n");
        sb.Append("Er staan ").Append(aantalCvs).Append(" contactverzoeken open die al verlopen zijn. Het langstlopende contactverzoek staat al ").Append(maxWerkdagen).Append(" werkdag");
        if (maxWerkdagen != 1) sb.Append("en");
        sb.Append(" open.\n\n");
        sb.Append("Inwoners waarderen een snelle afhandeling van hun verzoeken.\n");
        sb.Append("Neem contact op en handel deze contactverzoeken af.\n\n");
        sb.Append("Ga naar je werkvoorraad: ").Append(werkvoorraadUrl).Append("\n\n");
        sb.Append("Fijne werkdag");
        return sb.ToString();
    }

    public static string GenereerMailSubject(int aantalVerlopenContactVerzoeken)
    {
       return aantalVerlopenContactVerzoeken == 1
                ? "1 contactverzoek wacht op jou"
                : $"{aantalVerlopenContactVerzoeken} contactverzoeken wachten op jou";

    }

}
