
Omgevingsvariabelen
===================

**Alle variabelen zijn verplicht, tenzij anders vermeld.** 

OpenID Connect settings
---------------------------------
| **Variabele**                                           | **Waarde**                                                                                                  |
|---------------------------------------------------------|----------------------------------------------------------------------------------------------------------|
| web.oidc.authority                                      | URL van de OpenID Connect Identity Provider <details> <summary>Meer informatie </summary>Bijvoorbeeld: `https://login.microsoftonline.com/ce1a3f2d-2265-4517-a8b4-3e4f381461ab/v2.0` </details>         |
| web.oidc.clientId                                       | Voor toegang tot de OpenID Connect Identity Provider <details> <summary>Meer informatie </summary>Bijvoorbeeld: `54f66f54-71e5-45f1-8634-9158c41f602a` </details>  |
| web.oidc.clientSecret                                   | Secret voor de OpenID Connect Identity Provider <details> <summary>Meer informatie </summary>Bijvoorbeeld: `VM2B!ccnebNe.M*gxH63*NXc8iTiAGhp` </details>    |
| web.oidc.itaSystemAccessRole                            | De waarde van de role claim in het JWT token van de OpenID Connect Provider voor toegang tot ITA <details> <summary>Meer informatie </summary>Bijvoorbeeld: `ITA-Gebruiker` </details>     |
| web.oidc.functioneelBeheerderRole                       | De waarde van de role claim in het JWT token van de OpenID Connect Provider voor toegang tot beheerfuncties (zoals het beheren van kanalen) <details> <summary>Meer informatie </summary>Bijvoorbeeld: `ITA-Functioneel-Beheerder` </details>     |
| web.oidc.nameClaimType                                  | De naam van de claim in het JWT token van de OpenID Connect Provider waarin de volledige naam van de ingelogde gebruiker staat <br/> (default waarde is `name`) |
| web.oidc.roleClaimType                                  | De naam van de claim in het JWT token van de OpenID Connect Provider waarin de rollen van de ingelogde gebruiker staan. <br/> (default waarde is `roles`)  |
| web.oidc.objectregisterMedewerkerIdClaimType            | De naam van de claim in het JWT token van de OpenID Connect Provider waarin een unieke identificatie van de ingelogde gebruiker staat die overeenkomt met het attribuut `identificatie` van de medewerker in het Medewerkersregister in OverigeObjecten. <br/><br/>In de meeste gevallen zal hier waarschijnlijk 'email' voor gebruikt worden. <br/>Dit gegeven wordt gebruikt om Contactverzoeken te vinden die zijn toegewezen aan de ingelogde gebruiker en om Contactverzoeken te vinden die zijn toegewezen aan de afdelingen en groepen van de ingelogde gebruiker. <br/><br/>**Let op:** deze waarde wordt ook als `user_id` claim meegestuurd in het JWT token richting het zaaksysteem. Bij gebruik van de e-Suite via de podiumd-adapter gebruikt de e-Suite deze waarde om te bepalen welke zaken de ingelogde gebruiker mag inzien. De waarde moet daarom overeenkomen met de gebruikersnaam van de medewerker in de e-Suite.  |
| web.oidc.emailClaimType                                 | De naam van de claim in het JWT token van de OpenID Connect Provider waarin het e-mailadres van de ingelogde gebruiker staat. <br/> (default waarde is `email`)   |

Notificatie settings
---------------------------------

| **Variabele**        | **Waarde**                                                                                               |
|----------------------|----------------------------------------------------------------------------------------------------------|
| smtp.host            | Adres van uw mailserver                                                                                  |
| smtp.port            | De poort waarover van uw mailserver benaderbaar is                                                       |
| smtp.username        | Credentials voor toegang tot uw mailserver                                                               |
| smtp.password        | Credentials voor toegang tot uw mailserver                                                               |
| smtp.enableSsl       | true/false                                                                                               |
| smtp.fromEmail       | Afzenderadres voor mails die door ITA verstuurd worden                                                   |
| ita.baseUrl          | De base URL van de ITA website, gebruikt voor deeplinks in email notificaties naar contactverzoeken. Bijvoorbeeld: `https://itawebsite.nl` |


 
Koppelingen met externe registers
---------------------------------

ITA gaat ervan uit dat emailadressen van medewerkers en organisatorische eenheden in een overige objecten registratie te vinden zijn en voldoen aan deze formats:
- medewerker: https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Medewerker/medewerker-schema.json
- groep: https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Afdeling%20en%20Groep/groep-schema.json
- afdeling:  https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Afdeling%20en%20Groep/afdeling-schema.json

| **Variabele**                          | **Waarde**                                                                                                  |
|----------------------------------------|----------------------------------------------------------------------------------------------------------|
| apiConnections.objectApi.baseUrl       | https://uw-overige-objecten-adres.nl/api/v2/  |
| apiConnections.objectApiKey            | min. 32 karakters  |
| apiConnections.openKlantApi.baseUrl    | https://uw-openklant-adres/klantinteracties/api/v1/  |
| apiConnections.openKlantApiKey         | min. 32 karakters  |
| apiConnections.zaakSysteem.baseUrl     |  De base URL van het zaaksysteem. Dit kan een directe koppeling zijn met bijvoorbeeld OpenZaak (`https://uw-openzaak-adres.nl`) of, bij gebruik van de e-Suite als zaaksysteem, de URL van de podiumd-adapter (`https://podiumd-adapter.uw-domein.nl`). Bij gebruik van de adapter wordt de root-URL gebruikt; de adapter routeert op basis van de ZGW API-paden (bijv. `/zaken/api/v1/`). |
| apiConnections.zaakSysteem.clientId    | De client ID voor authenticatie bij het zaaksysteem. Bij gebruik van de podiumd-adapter is dit de client ID zoals geconfigureerd in de adapter.  |
| apiConnections.zaakSysteemKey          |  De secret/key voor authenticatie bij het zaaksysteem. Min. 32 karakters. Bij gebruik van de podiumd-adapter is dit de client secret zoals geconfigureerd in de adapter.  |
| logboek.type                           | De url van het logboek objecttype in de objecttypen api. zie de objecttypen pagina in de documentatie for meer informatie  |
| logboek.typeVersion                    | De versie van het logboek objecttype dat gebruikt wordt (hoogstwaarschijnlijk 1) |
| afdeling.type                          | De url van het afdeling objecttype in de objecttypen api. |
| afdeling.typeVersion                   | De versie van het afdeling objecttype dat gebruikt wordt (waarschijnlijk 1) |
| groep.type                             | De url van het groep objecttype in de objecttypen api.   |
| groep.typeVersion                      | De versie van het groep objecttype dat gebruikt wordt (waarschijnlijk 1) |

Database configuratie
---------------------------------
| **Variabele**            | **Waarde**                                                                                  |
|--------------------------|---------------------------------------------------------------------------------------------|
| database.name            | Naam van de database voor ITA                                                               |
| database.username        | Gebruikersnaam voor toegang tot de database                                                 |
| database.password        | Wachtwoord voor toegang tot de database                                                     |
| database.host            | Hostnaam van de database server *(optioneel - alleen nodig als `postgresql.enabled=false`)* |

Theming
------------------

Deze theming variabelen zijn allemaal optioneel.

| Variabele                      | Omschrijving |
|--------------------------------|--------------|
| web.resources.logoUrl          | Publieke URL waar het logo van de gemeente beschikbaar is, bijvoorbeeld: `https://www.mijn-gemeente.nl/logo.svg` |
| web.resources.faviconUrl       | Publieke URL waar het favicon van de gemeente beschikbaar is, bijvoorbeeld: `https://www.mijn-gemeente.nl/favicon.ico` |
| web.resources.designTokensUrl  | Publieke URL waar het CSS-bestand met NL Design System tokens beschikbaar is, om de applicatie te stylen in gemeentehuisstijl, bijvoorbeeld: `https://unpkg.com/@gemeente/design-tokens/dist/index.css` |
| web.resources.webFontSources   | Publieke URL – of meerdere publieke URL's – als verwijzing naar web-font-bestand(en) horend bij de gemeentehuisstijl, bijvoorbeeld: `['https://fonts.mijn-gemeente.nl/custom-regular-font.woff2', 'https://fonts.mijn-gemeente.nl/custom-bold-font.woff2']`. Een enkele verwijzing naar de locatie waar alle font-style-bestanden staan kan ook: `['https://fonts.mijn-gemeente.nl/custom-font/']`. **Let op:** deze configuratie is alleen bedoeld om de font-bestanden onder CORS te kunnen inladen. Verwijzingen naar bestanden zullen ook in de theme styling onder een `@font-face` ruleset gespecificeerd moeten worden. |



