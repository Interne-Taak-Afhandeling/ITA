using System.Net;

namespace InterneTaakAfhandeling.Poller.Features.VerlopenContactverzoekHerinneringNotificatie;

public sealed class VerlopenContactverzoekHerinneringNotificatieTemplateService
{
    public static string GenereerMailBody(int aantalCvs, int maxWerkdagen, string werkvoorraadUrl)
    {
        var werkdagSuffix = maxWerkdagen != 1 ? "en" : "";
        var werkvoorraadUrlEncoded = WebUtility.HtmlEncode(werkvoorraadUrl);

        return $$"""
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; line-height: 1.6; }
                    a { color: #0066cc; text-decoration: none; }
                    a:hover { text-decoration: underline; }
                </style>
            </head>
            <body>
                <p>Beste collega,</p>
                <p>Er staan <strong>{{aantalCvs}}</strong> contactverzoeken open die al verlopen zijn. Het langstlopende contactverzoek staat al <strong>{{maxWerkdagen}}</strong> werkdag{{werkdagSuffix}} open.</p>
                <p>Inwoners waarderen een snelle afhandeling van hun verzoeken.</p>
                <p>Neem contact op en handel deze contactverzoeken af.</p>
                <p><a href="{{werkvoorraadUrlEncoded}}">Ga naar je werkvoorraad</a></p>
                <p>Fijne werkdag</p>
            </body>
            </html>
            """;
    }

    public static string GenereerMailBodyPlainText(int aantalCvs, int maxWerkdagen, string werkvoorraadUrl)
    {
        var werkdagSuffix = maxWerkdagen != 1 ? "en" : "";

        return $"""
            Beste collega,

            Er staan {aantalCvs} contactverzoeken open die al verlopen zijn. Het langstlopende contactverzoek staat al {maxWerkdagen} werkdag{werkdagSuffix} open.

            Inwoners waarderen een snelle afhandeling van hun verzoeken.
            Neem contact op en handel deze contactverzoeken af.

            Ga naar je werkvoorraad: {werkvoorraadUrl}

            Fijne werkdag
            """;
    }

    public static string GenereerMailSubject(int aantalVerlopenContactVerzoeken)
    {
       return aantalVerlopenContactVerzoeken == 1
                ? "1 contactverzoek wacht op jou"
                : $"{aantalVerlopenContactVerzoeken} contactverzoeken wachten op jou";

    }

}
