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

### Poller Modes (POLLER_MODE)

De Poller ondersteunt meerdere uitvoermodi via de omgevingsvariabele `POLLER_MODE`. Elke modus wordt in productie aangestuurd door een eigen Kubernetes CronJob.

| Waarde | Doel | K8s CronJob schedule |
| ------ | ---- | -------------------- |
| `nieuwe-internetaak-notificatie` (default) | Controleert op nieuwe internetaken en verstuurt e-mailnotificaties naar toegewezen medewerkers | `*/15 * * * *` (elke 15 min) |
| `verlopen-contactverzoek-herinnering-notificatie` | Verstuurt dagelijkse herinneringsmails voor verlopen contactverzoeken | `0 7 * * 1-5` (werkdagen 07:00) |

**Lokaal draaien in een specifieke modus:**

```bash
# Standaard (nieuwe internetaak notificatie)
dotnet run --project InterneTaakAfhandeling.Poller

# Dagelijkse herinnering voor verlopen contactverzoeken
POLLER_MODE=verlopen-contactverzoek-herinnering-notificatie dotnet run --project InterneTaakAfhandeling.Poller
```

Op Windows (PowerShell):

```powershell
$env:POLLER_MODE = "verlopen-contactverzoek-herinnering-notificatie"
dotnet run --project InterneTaakAfhandeling.Poller
```

Bij een onbekende `POLLER_MODE`-waarde logt de Poller een fout en stopt zonder actie.

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

