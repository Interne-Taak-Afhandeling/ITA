---
name: Feature
about: A user-facing capability within an Epic
title: "🚀 Feature: "
labels: feature
assignees: ""
---

# 🚀 Feature: {{title}}

## Bounded Context

<!-- In welk deel van het systeem / werkproces valt deze feature? Beschrijf kort het raakvlak met andere onderdelen als dat van toepassing is. -->

---

## User Stories

<!-- Geschreven door de Product Owner -->

- Als **_, wil ik _** zodat \_\_\_

## Design Handoff

<!-- Link naar Figma frames, componentspecificaties of geannoteerde mockups -->

- Figma:
- Schermen: (standaard / laden / fout / leeg)
- Toegankelijkheid:

---

## Acceptance Criteria

<!-- Deze vormen direct de basis voor Gherkin-scenario's in onderliggende taken. Eén acceptatiecriterium →
één of meer scenario's. -->

- [ ]

## Edge Cases

<!-- Benoem ze hier expliciet zodat taken ze in Gherkin kunnen afdekken. Gebruik domeintaal. -->

## Domain Events

<!-- Welke gegevens verstuurt of verwerkt deze feature vanuit andere onderdelen? -->

- Verstuurt:
- Verwerkt:

---

## Definition of Ready

<!-- Deze feature is klaar om in taken op te splitsen wanneer: -->

- [ ] User stories goedgekeurd door PO
- [ ] Ontwerp afgerond
- [ ] Acceptance criteria geschreven en beoordeeld
- [ ] Edge cases geïdentificeerd

---

## Feature Completion Checklist

<!-- Each item is "if applicable" — skip items that don't apply, but document why. Source: docs/steering/QA.md -->

- [ ] **Changelog** — An entry has been added to the top of the changelog with the feature/bug name and a link to the GitHub issue. If there are new environment variables or other upgrade-relevant changes, they are mentioned here.
- [ ] **Manual** — Installation & configuration manual have been updated. Environment variables and anything else needed to install/run ITA are explained.
- [ ] **Helm** — New release variables have been included in the Helm charts/values files.
- [ ] **Readme** — README has been updated with information on how to run the applications locally.
- [ ] **User secrets** — User secrets in 1Password have been added/updated.
- [ ] **Decision record** — Any significant architectural or design decisions have been documented in `docs/decision-record/`.
- [ ] **Cleanup** — Branches, test data, temporary copies of databases, files, and temporary K8S clusters have been cleaned up.
- [ ] **UX** — Significant design changes have been checked with a UX specialist.
- [ ] **Test documentation** — Regression test scenarios not covered by automated tests, known issues, and things deliberately not tested are documented.

---

## Proposed Tasks

<!-- Onderliggende taken — link issues zodra ze aangemaakt worden. -->

- [ ]
