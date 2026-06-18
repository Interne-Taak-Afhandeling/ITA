using System.Net;
using System.Text;
using InterneTaakAfhandeling.Common.Services;
using InterneTaakAfhandeling.Common.Services.DagelijkseHerinnering;

namespace InterneTaakAfhandeling.Poller.Features.DagelijkseHerinnering;

public sealed class DagelijkseHerinneringsTemplateService : IDagelijkseHerinneringsTemplateService
{
    public HerinneringsMailContent GenereerMailContent(RecipientHerinneringData ontvanger, string baseUrl)
    {
        var aantalCvs = ontvanger.VerlopenContactVerzoeken.Count;
        var maxWerkdagen = ontvanger.MaxAantalWerkdagenOpenstaan;
        var isMedewerker = ontvanger.Ontvanger.Actoridentificator?.CodeObjecttype ==
                          KnownMedewerkerIdentificators.CodeObjecttypeMedewerker;

        var werkvoorraadUrl = isMedewerker
            ? $"{baseUrl.TrimEnd('/')}/"
            : $"{baseUrl.TrimEnd('/')}/afdelings-contacten";

        var onderwerp = $"{aantalCvs} contactverzoeken wachten op jou";

        var htmlBody = BouwHtmlBody(aantalCvs, maxWerkdagen, werkvoorraadUrl);
        var plainTextBody = BouwPlainTextBody(aantalCvs, maxWerkdagen, werkvoorraadUrl);

        return new HerinneringsMailContent
        {
            Onderwerp = onderwerp,
            HtmlBody = htmlBody,
            PlainTextBody = plainTextBody
        };
    }

    private static string BouwHtmlBody(int aantalCvs, int maxWerkdagen, string werkvoorraadUrl)
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

    private static string BouwPlainTextBody(int aantalCvs, int maxWerkdagen, string werkvoorraadUrl)
    {
        var werkdagenLabel = maxWerkdagen == 1 ? "werkdag" : "werkdagen";
        return $"""
Beste collega,

Er staan {aantalCvs} contactverzoeken open die al verlopen zijn. Het langstlopende contactverzoek staat al {maxWerkdagen} {werkdagenLabel} open.

Inwoners waarderen een snelle afhandeling van hun verzoeken.

Neem contact op en handel deze contactverzoeken af.

Ga naar je werkvoorraad: {werkvoorraadUrl}

Fijne werkdag
""";
    }
}
