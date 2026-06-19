***************
Decision-record
***************

Notificeren van nieuwe Contactverzoeken
---------------------------------------------
Een poller controleert periodiek (momenteel een keer per minuut) of er nieuwe Contactverzoeken zijn. Alle toegewezen Actoren waarvan een e-mailadres achterhaald kan worden, ontvangen per Contactverzoek een e-mail.


In een interne database wordt bijgehouden wat de aanmaak datum/tijd is van het Contactverzoek (InterneTaak) dat als laatste verwerkt is. De poller haalt steeds de nieuwste items op en gaat door totdat er geen items meer zijn die nieuwer zijn dan de bijgehouden datum/tijd. Er wordt niet uitgegaan van het id, maar van het tijdstip, omdat het item in de bron verwijderd zou kunnen worden, waarna we niet meer zouden kunnen bepalen welke items al verwerkt zijn. Er is voor gekozen om niet gebruik te maken van het statusveld in OpenKlant om bij te houden voor welke items al een notificatie is verstuurd. De toegestane waardes zijn daarvoor niet toereikend.  

Er is voor gekozen om (voorlopig) geen gebruik te maken van de notificatie mogelijkheden in OpenKlant als alternatief voor de poller. De reden daarvoor was dat de mogelijkheden en beperkingen van het notificeren vanuit OpenKlant niet tijdig voldoende helder gemaakt konden worden.  

Koppelen van contactmomenten bij Contactverzoeken
-------------------------------------------------

Het datamodel staat niet toe dat een contactmoment (klantcontact) dat voortkomt uit een Contactverzoek (interne taak) wordt gekoppeld aan die interne taak.
Het contactmoment kan alleen, via een onderwerpobject, gekoppeld worden aan een eerder contactmoment. Daarbij hebben we twee keuzes moeten maken. (gerelateerd aan issue #13)

**Probleem**: Er kan niet op het aanleidinggevende klantcontact gezocht wordt (daar is een issue voor aangemaakt op de backlog van OpenKlant).

* Keuze: We slaan de benodigde gegevens op in de onderwerpobjectidentificator velden. Daar kan wel op gezocht worden.
* Overwegingen: Wachten op aanpassing van de OpenKlant api duurt te lang. Het zou ook opgelost kunnen worden door de benodigde gegevens apart op te slaan in een soort logboek van de interne taak. Zoiets staat op de planning, maar was een te omvangrijk ingreep om dit probleem nu mee op te lossen.

**Probleem**: We zouden het nieuwe contactmoment altijd kunnen koppelen aan het eerste contactmoment, of aan het laatste. 

* Keuze: We hebben gekozen voor het laatste, omdat op die manier een keten te maken is waarbij de volgorde klopt. 
* Overwegingen: We hadden erop kunnen gokken dat de items altijd door de API in de zelfde volgorde van moment van aanmaken geretourneerd worden, maar daar worden geen garanties voor gegeven. Of we zouden kunnen sorteren op datum, maar aangezien de datum geen verplicht veld is leek dat ook geen stabiele betrouwbare oplossing. De gekozen oplossing is omslachtig, maar lijkt wel de veiligste oplossing.


Koppelen van zaken bij Contactverzoeken
---------------------------------------------

Het OpenKlant-model staat in principe toe dat er meer dan één Zaak gekoppeld is aan een Contactverzoek (via een ``onderwerpobject`` bij het ``klantcontact`` van het Contactverzoek). ITA ondersteunt dit op dit moment niet. De kans dat dit voorkomt is zeer klein. In de oorspronkelijke opzet van Klantinteracties was het idee dat elke ``klantcontact`` één Onderwerp heeft. Dit maakt meerdere ``onderwerpobjecten`` onwaarschijnlijk. 

Dus bij het koppelen van een Zaak aan een Contactverzoek hebben we gekozen voor:

* Als een gebruiker een Zaak koppelt aan een Contactverzoek, creëert ITA een ``onderwerpobject``
* Als er al een Zaak gekoppeld is, dan vervangt ITA de eerder gekoppelde Zaak door de nieuw gekoppelde Zaak
* Als het voorkomt dat er al 2 of meer Zaken zijn gekoppeld aan een Contactverzoek, dan is het niet mogelijk om een Zaakkoppeling te leggen of aan te passen. In dat geval verschijnt er een melding: "Er is een fout opgetreden bij het koppelen van de zaak: Het koppelen van een nieuwe zaak wordt niet ondersteund omdat er al meerdere zaken gekoppeld zijn aan dit Contactverzoek."


Communicatie met de e-Suite via de podiumd-adapter
---------------------------------------------------

ITA ondersteunt naast OpenZaak ook de e-Suite als zaaksysteem. De communicatie met de e-Suite verloopt niet rechtstreeks, maar via de podiumd-adapter. De adapter vertaalt de standaard ZGW API-aanroepen naar e-Suite API-aanroepen. Dit is dezelfde aanpak als KISS hanteert. Door de adapter te gebruiken hoeft ITA geen e-Suite specifieke logica te bevatten en kan het via de standaard ZGW API's blijven communiceren. Er zijn geen nieuwe omgevingsvariabelen nodig; de bestaande ``zaakSysteem`` configuratie (baseUrl, clientId, key) wordt naar de adapter verwezen.

Tonen van digitale adressen
---------------------------------

Bij een Contactverzoek worden de contactgegevens van de klant opgeslagen in ``digitaleAdressen`` die verstrekt zijn door de betrokkene van het Klantcontact. 
De interface van ITA gaat uit van 1 of 2 telefoonnummers, en 1 e-mailadres. 
Omdat bij de telefoonnummers, zeker een tweede telefoonnummer, een omschrijving opgeslagen kan zijn die door de klant is doorgegeven, gebruiken we voor de labels van telefoonnummers de waarde van ``digitaalAdres.omschrijving``. Voor het e-mailadres doen we dat niet.  
