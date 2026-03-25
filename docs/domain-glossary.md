# ITA Domain Glossary

Authoritative definitions for domain concepts used throughout the
ITA codebase. All terms are in Dutch as used by stakeholders (see
the [Dutch Domain Language Convention](../.specify/memory/constitution.md#dutch-domain-language-convention)
in the project constitution).

## Core Domain Concepts

| Term | Dutch | Definition |
|------|-------|------------|
| **Internetaak** | Interne taak | Internal task assigned to an actor, with status `te_verwerken` (to process) or `verwerkt` (processed) |
| **Klantcontact** | Klantcontact | The original citizen contact record — captures channel, subject, content, and involved parties |
| **Actor** | Actor | The assignee of an internetaak — a `medewerker` (employee), `groep` (group), or `afdeling` (department) |
| **Betrokkene** | Betrokkene | An involved party in a klantcontact, carrying digital addresses and personal information |
| **Zaak** | Zaak | A formal government case (zaakgericht werken) that can be linked to klantcontacten |
| **Kanaal** | Kanaal | Communication channel (e.g., telefoon, e-mail) — managed locally by functioneel beheerders |
| **Logboek** | Logboek | Audit log containing activiteiten (entries) that record actions performed on internetaken |

## Supporting Concepts

| Term | Dutch | Definition |
|------|-------|------------|
| **Onderwerpobject** | Onderwerpobject | Link object that connects a klantcontact to a zaak |
| **Contactnaam** | Contactnaam | Name record of a betrokkene (voornaam, voorvoegselAchternaam, achternaam) |
| **DigitaleAdres** | Digitale adres | Digital address of a betrokkene (e.g., e-mail, telefoonnummer) |
| **Partij** | Partij | A registered party (citizen or organisation) in the Open Klant system |
| **Medewerker** | Medewerker | An employee of the municipality, stored in the Objecten API |
| **Groep** | Groep | An organisational group of medewerkers, stored in the Objecten API |
| **Afdeling** | Afdeling | A department within the municipality, stored in the Objecten API |
| **Activiteit** | Activiteit | A single audit-log entry within a logboek |

## Status Values

| Entity | Status | Meaning |
|--------|--------|---------|
| Internetaak | `te_verwerken` | Task is open and awaiting processing |
| Internetaak | `verwerkt` | Task has been completed |

## Actor Types (`SoortActor`)

| Value | Meaning |
|-------|---------|
| `medewerker` | Individual employee |
| `organisatorische_eenheid` | Organisational unit (groep or afdeling) |
