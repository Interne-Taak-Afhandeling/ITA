## ITA Contact
ITA is een applicatie waarmee een organisatie contactverzoeken kan afhandelen. Het is gebaseerd op de Klantinteracties API’s zoals die geïmplementeerd zijn in Open Klant 2.x. Het kan Contactverzoeken afhandelen vanuit een register dat middels deze API’s communiceert.

ITA is een afkorting van Interne Taak Afhandeling. Dit is een verwijzing naar het object Interne Taak uit het Klantinteracties informatiemodel. Een Contactverzoek bestaat uit een interneTaak en het klantcontact dat leidde tot die interneTaak.

## Docker Compose Configuratie

### Profielen
Zet Docker-compose als startup project. In Visual Studio selecteer je het gewenste profiel via de launch profile dropdown bovenin, vervolgens start je met **F5**.

#### Web Profiel
- Start alleen de website.
- Database wordt gestart zonder debugging.

#### Poller Profiel
- Start alleen de poller.
- Database wordt gestart zonder debugging.

### Handmatig Starten met Docker Compose

Je kunt de services ook handmatig starten via de command line (soms start de database later op en moet je poller of web opnieuw draaien):

```bash
# Alleen website starten
docker-compose --profile web up

# Alleen poller starten
docker-compose --profile poller up
```

## Configuratie

### Verbindingsreeksen

De poller gebruikt verschillende configuratiebestanden:

- **appsettings.json**: Basisconfiguratie
- **appsettings.Development.json**: Configuratie specifiek voor de ontwikkelomgeving
- **appsettings.Docker.json	**: Docker-specifieke instellingen

## Database Migraties

### Migratie Aanmaken

```powershell
Add-Migration InitialMigration -Project InterneTaakAfhandeling.Poller -StartupProject InterneTaakAfhandeling.Poller
```

### Migratie Verwijderen

```powershell
Remove-Migration -Project InterneTaakAfhandeling.Poller -StartupProject InterneTaakAfhandeling.Poller
```

# ITA

## Documentatie
[Documentatie](https://ita.readthedocs.io/)
