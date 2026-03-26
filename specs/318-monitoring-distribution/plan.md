# Implementation Plan: Monitoring en Werkverdeling

**Branch**: `318-monitoring-distribution` | **Date**: 2026-03-25 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/318-monitoring-distribution/spec.md`

## Summary

Add coordinator monitoring and work distribution capabilities to
ITA. Two new roles (organisatie-coördinator, team-coördinator) get
a dedicated `/werklijst` page showing paginated, role-scoped
openstaande contactverzoeken with afhandeltermijn tracking and
bulk assignment (werkverdeling) to any afdeling/groep + medewerker.

## Technical Context

**Language/Version**: C# / .NET 8.0 (back-end), TypeScript 5.8 (front-end)
**Primary Dependencies**: ASP.NET Core 8, Vue 3.5, Vite 6.4, Pinia 3,
  Utrecht component library, Den Haag design-system packages
**Storage**: PostgreSQL via EF Core 8 (Npgsql) — only for
  afhandeltermijn configuration; domain data in external APIs
**Testing**: Playwright + MSTest (.NET 8) for E2E
**Target Platform**: Docker containers (Web.Server + Poller), Helm/K8s
**Project Type**: BFF web application
**Performance Goals**: Werklijst loads in <2s for up to 500 items;
  bulk assignment of 10 items completes in <5s
**Constraints**: Must use existing OpenKlant, Objecten, Zaken API
  clients; no new HTTP client libraries
**Scale/Scope**: Municipal-scale (~50 coordinators, ~500 open
  contactverzoeken at peak)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Technology Stack Compliance | ✅ Pass | .NET 8 + Vue 3/TS + PostgreSQL + Utrecht; no new frameworks |
| II. Architecture & Pattern Adherence | ✅ Pass | Vertical feature slices (back-end), Composition API + services/composables (front-end). Werklijst follows `InterneTakenOverzicht` + `AllContactverzoekenView` patterns |
| III. Code Quality | ✅ Pass | ESLint/Prettier, strict TS, `[ProducesResponseType]`, Serilog, ProblemDetails error handling |
| IV. Testing Standards | ✅ Pass | E2E Playwright scenarios for werklijst + werkverdeling |
| V. User Experience Consistency | ✅ Pass | Utrecht table/pagination/form components, toast feedback, `BevestigingsModal` for bulk assign, route guards for coordinator access |
| Dutch Domain Language | ✅ Pass | All new entities use Dutch terms (werklijst, werkverdeling, afhandeltermijn, coördinator) |
| External API Landscape | ✅ Pass | Uses existing `OpenKlantApiClient` (internetaken, actoren) + `ObjectApiClient` (medewerkers, groepen, afdelingen) |

## Project Structure

### Documentation (this feature)

```text
specs/318-monitoring-distribution/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api-endpoints.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
InterneTaakAfhandeling.Web.Server/
├── Features/
│   ├── Werklijst/                         # NEW feature slice
│   │   ├── WerklijstController.cs         # GET api/werklijst
│   │   ├── IWerklijstService.cs           # Service interface
│   │   ├── WerklijstService.cs            # Orchestrates API calls
│   │   └── WerklijstModels.cs             # Query params + response DTOs
│   └── Werkverdeling/                     # NEW feature slice
│       ├── WerkverdelingController.cs     # POST api/werkverdeling
│       ├── IWerkverdelingService.cs       # Service interface
│       ├── WerkverdelingService.cs        # Bulk assignment logic
│       └── WerkverdelingModels.cs         # Request/response models
├── Authentication/
│   ├── ITAUser.cs                         # MODIFIED — add coordinator role flags
│   └── AuthorizationPolicies.cs           # MODIFIED — add coordinator policies
├── Config/
│   ├── ServiceCollectionExtensions.cs     # MODIFIED — register new services
│   └── AfhandeltermijnOptions.cs          # NEW — configurable termijn

InterneTaakAfhandeling.Web.Client/src/
├── views/
│   └── WerklijstView.vue                  # NEW — main werklijst page
├── components/
│   ├── werklijst/                         # NEW — werklijst-specific components
│   │   ├── WerklijstTable.vue             # Table with overdue indicators
│   │   ├── WerklijstFilter.vue            # Afdeling/groep filter
│   │   └── WerkverdelingDialog.vue        # Bulk assignment dialog
│   └── interne-taken-tables/              # EXISTING — reference pattern
├── composables/
│   └── (reuse existing usePagination, useState)
├── services/
│   └── werklijstService.ts               # NEW — API calls
├── types/
│   └── werklijst.ts                       # NEW — TypeScript types
├── router/
│   └── index.ts                           # MODIFIED — add /werklijst route
├── plugins/
│   └── routerGuards.ts                    # MODIFIED — add coordinator guard
├── stores/
│   └── auth.ts                            # MODIFIED — add coordinator computed
└── types/
    └── user.ts                            # MODIFIED — add coordinator flags

InterneTaakAfhandeling.EndToEndTest/
└── Werklijst/                             # NEW — E2E test scenarios
    ├── WerklijstScenarios.cs
    ├── WerkverdelingScenarios.cs
    └── WerklijstLocators.cs
```

**Structure Decision**: Two new vertical feature slices on the
back-end (Werklijst + Werkverdeling), following the existing
`InterneTakenOverzicht` and `AssignInternetaakToMe` patterns.
Front-end adds a new view + dedicated components, following the
`AllContactverzoekenView` pattern with `usePagination`.

## Complexity Tracking

No constitution violations. All patterns follow existing conventions.
