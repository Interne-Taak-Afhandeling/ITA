<!--
  Sync Impact Report
  ==================
  Version change: 1.1.3 → 1.2.0
  Modified principles:
    - II. Codebase Architecture & Pattern Adherence — added Vertical
      Slice Delivery Rule requiring features with multiple user stories
      to be planned, implemented, and delivered as independent vertical
      slices. Motivated by scope-creep observed during #318/#320.
  Added sections: none
  Removed sections: none
  Templates requiring updates:
    - .specify/templates/plan-template.md        ⚠ pending (Constitution Check section is generic)
    - .specify/templates/spec-template.md         ✅ compatible
    - .specify/templates/tasks-template.md        ✅ updated (anti-pattern callout added)
    - .specify/templates/checklist-template.md    ✅ compatible
    - .specify/templates/agent-file-template.md   ✅ compatible
  Follow-up TODOs: none
-->

# Interne Taak Afhandeling (ITA) Constitution

ITA is a Dutch government application for handling contactverzoeken
(citizen contact requests). Municipal employees use it to manage
internal tasks that originate from the Open Klant 2.x
Klantinteracties API. The application operates as a
Backend-for-Frontend (BFF), orchestrating external ZGW-compliant
APIs while maintaining minimal local state.

## Core Principles

### I. Technology Stack Compliance

Every project artifact MUST target the approved technology stack.
Introducing a new framework, language, or major library MUST be
justified and approved before adoption.

- **Back-end**: .NET 8.0 — ASP.NET Core Web API (Web.Server),
  Console App (Poller), shared class library (Common)
- **Front-end**: Vue 3.5 with TypeScript 5.8, Vite 6.4, Pinia 3
- **Database**: PostgreSQL via Entity Framework Core 8 (Npgsql)
- **Authentication**: OIDC (Duende AccessTokenManagement)
- **UI component system**: NL Design System — Utrecht component
  library + Den Haag design-system packages
- **Containerisation**: Docker images, Helm charts, GHCR registry
- **CI/CD**: GitHub Actions (lint, build, E2E, Docker + Helm release)
- New packages MUST be compatible with the versions above and MUST
  NOT introduce conflicting paradigms (e.g., no Axios alongside the
  existing fetch wrapper, no Options API components alongside
  Composition API)

### II. Codebase Architecture & Pattern Adherence

New features MUST follow the established architecture.

> **⚠ Pattern Reuse Rule (NON-NEGOTIABLE)**
>
> When a new back-end or front-end feature requires logic similar
> to an existing feature or view, the new implementation MUST
> follow the same patterns (e.g., services, composables,
> components), conventions, and typings. If the duplicated logic
> is substantial, extract it into a shared module after proper
> research into the existing implementation.

> **⚠ Vertical Slice Delivery Rule (NON-NEGOTIABLE)**
>
> Features with multiple user stories MUST be planned,
> implemented, and delivered as **independent vertical slices** —
> each slice covers exactly one user story end-to-end (auth →
> types → API → UI → tests → docs). Tasks for a given story
> MUST NOT include models, configuration, DTO fields, or types
> that only serve a future story; defer those to their respective
> slice. No horizontal layers across stories.
>
> *Rationale*: During #318/#320, config and type definitions for
> US3/US4 leaked into the US1 task list, causing scope creep that
> had to be cleaned up mid-implementation.

**Back-end patterns**

- Vertical feature-slice layout: each feature lives under
  `Features/{FeatureName}/` with co-located Controller, Interface,
  Service, and request/response models
- Thin `[ApiController]` controllers delegating to scoped services
  via interface injection; controllers MUST NOT contain business logic
- Primary constructors (C# 12) for dependency injection in services
  and controllers
- Typed `HttpClient` registrations for external APIs (OpenKlant,
  ObjectAPI, ZakenAPI) configured in `ServiceCollectionExtensions`
- Direct `ApplicationDbContext` injection for local data access — no
  Repository or Unit-of-Work abstraction
- Options pattern (`IOptions<T>`) with `ValidateDataAnnotations()`
  and `ValidateOnStart()` for structured configuration
- Global exception handling via `IExceptionHandler` mapping to
  RFC 7807 ProblemDetails
- Manual DI registration in `ServiceCollectionExtensions
  .RegisterServices()`

**Front-end patterns**

- 100% Composition API with `<script setup lang="ts">` — no
  Options API
- Feature-based directory structure: `views/`, `components/`,
  `composables/`, `services/`, `types/`
- Custom `fetchWrapper` (native `fetch`) for API communication —
  service objects (not classes) export typed functions (`get<T>`,
  `post<T>`, etc.)
- Pinia 3 stores using Composition API (`defineStore("id", () => {})`)
  for global state; local state via `ref()` / `computed()`
- Composables for reusable reactive logic (`useLoader`,
  `usePagination`, `useState`, `useBackNavigation`)
- Manual form handling with typed form-factory functions,
  native HTML validation, and `defineProps<T>()` / `defineEmits<T>()`
  with TypeScript generics
- Scoped SCSS per component (`<style lang="scss" scoped>`)
- CSS custom properties for theming; CSS layers
  (`@layer base, layout`) for cascade management

### III. Code Quality

All contributed code MUST meet the quality gates defined below.

- **Linting & formatting**: ESLint + Prettier MUST pass
  (`npm run lint:ci && npm run format:ci`); CI enforces this on every
  PR to `main`
- **Strong typing**: TypeScript `strict` mode is enabled; `any` MUST
  NOT be used without an inline justification comment
- **Back-end typing**: all controller actions MUST carry
  `[ProducesResponseType]` annotations; services MUST expose
  interfaces
- **Logging**: structured Serilog logging on the back-end;
  `SecureLogging.SanitizeAndTruncate()` MUST be used when logging
  values that could contain PII
- **Naming conventions**: C# — PascalCase for public members,
  primary-constructor parameters; TypeScript — camelCase for
  variables/functions, PascalCase for types/interfaces/components
- **Route conventions**: ASP.NET Core routes MUST use lowercase URLs
  (`options.LowercaseUrls = true`); Vue Router paths MUST use
  kebab-case
- **Error handling**: back-end errors MUST flow through the global
  `ExceptionToProblemDetailsMapper`; front-end MUST handle 401
  (re-auth) and 404 gracefully via `fetchWrapper`

### IV. Testing Standards

Quality MUST be validated through automated tests aligned with
the current testing strategy.

- **E2E framework**: Playwright + MSTest (.NET 8) — the sole
  automated test layer
- **Test class naming**: `{Feature}Scenarios`
  (e.g., `ContactverzoekScenarios`)
- **Test method naming**: `User_{Action}_{Context}`
  (e.g., `User_ClickContactverzoekToViewDetails_FromDashboard`)
  with a human-readable `[TestMethod("…")]` display name
- **Page Object pattern**: reusable page classes
  (e.g., `KanalenPage`) and `IPage` extension-method locators
  (e.g., `SharedLocator.cs`, `Locators.cs`)
- **Step-based reporting**: every logical step MUST call
  `await Step("description")` for the generated HTML report
- **Tracing**: Playwright tracing MUST remain enabled; trace
  archives are uploaded as CI artifacts
- **Test isolation**: tests that mutate shared state MUST be
  annotated with `[DoNotParallelize]`
- **Test data**: managed via `TestDataHelper` calling real APIs —
  no mocks; `RegisterCleanup()` MUST be used to guarantee teardown
- **CI pipeline**: E2E tests run daily (scheduled) and on PRs that
  touch `EndToEndTest/**`; results publish to GitHub Pages

### V. User Experience Consistency

Every user-facing change MUST maintain visual and interaction
consistency with the existing application.

- **Component library**: Utrecht component library (30+ globally
  registered components) is the primary UI toolkit; Den Haag
  design-system packages are used for specific patterns (timeline,
  process steps, tabs)
- **Design tokens**: theming MUST use CSS custom properties
  (`--utrecht-*`, `--denhaag-*`, `--ita-*`); runtime theme tokens
  are loaded from the API — hardcoded colour values MUST NOT appear
  in component styles
- **Feedback patterns**: toast notifications for success/error
  feedback; `BevestigingsModal` for destructive or irreversible
  actions
- **Navigation**: `useBackNavigation` composable for consistent
  "Terug naar …" links; route guards MUST enforce authentication,
  ITA system access, and Functioneel Beheerder access where required
- **Accessibility**: components MUST leverage the built-in a11y of
  the Utrecht library; custom elements MUST provide appropriate
  ARIA attributes
- **Responsive design**: layouts MUST respect the defined SCSS
  breakpoint (`$breakpoint-md: 64rem`)

## External API Landscape

ITA is a BFF (Backend-for-Frontend) that orchestrates three external
ZGW-compliant APIs. Most domain data lives in these systems — the
local PostgreSQL database stores only channels (Kanalen) and poller
state. Feature specs and plans MUST identify which APIs are involved
and reference the domain boundaries below.

| API | Client class | Domain | Key resources |
|-----|-------------|--------|---------------|
| **Open Klant API** | `OpenKlantApiClient` | Klantinteractie | InterneTaken, Klantcontacten, Actoren, Betrokkenen, Partijen |
| **Objecten API** | `ObjectApiClient` | Organisatie | Medewerkers, Groepen, Afdelingen |
| **Zaken API** | `ZakenApiClient` | Zaakgericht werken | Zaken (case management) |

- All API clients are registered as **typed `HttpClient`** instances
  in `ServiceCollectionExtensions` with pre-configured base URLs
  and authentication headers
- Open Klant and Objecten API authenticate via API key headers;
  Zaken API authenticates via ZGW JWT tokens
  (`ZgwTokenProvider` + `ZgwAuthenticationHandler`)
- New features that interact with an external API MUST use the
  existing typed client; adding a new typed client MUST follow the
  same registration pattern
- Feature specs MUST state which API(s) are consumed and which
  resources are read or written

## Dutch Domain Language Convention

All domain terminology MUST remain in Dutch as used by stakeholders
and throughout the existing codebase. For authoritative definitions
of all domain terms, see [`docs/domain-glossary.md`](../../docs/domain-glossary.md).

This convention applies to:

- Entity and model names (`Internetaak`, `Klantcontact`,
  `Contactverzoek`, `Betrokkene`, `Kanaal`, `Zaak`, `Afdeling`,
  `Groep`, `Onderwerpobject`, `Actor`, `Logboek`, `Activiteit`)
- Service and feature-folder names (`InterneTakenOverzicht`,
  `AssignInternetaakToMe`, `ForwardContactRequest`)
- TypeScript types and interfaces (`Internetaken`, `Betrokkene`,
  `SoortActor`, `DigitaleAdres`, `Contactnaam`)
- UI labels, route paths (`/contactverzoek`, `/afdelings-contacten`,
  `/beheer/kanalen`), and user-facing text
- Test scenario names and test-data constants
  (`ContactverzoekNummers`, `TestZaakIdentificatie`)

Technical infrastructure names (e.g., `fetchWrapper`, `useLoader`,
`ApplicationDbContext`) MUST remain in English.

New domain concepts MUST use the Dutch term as defined by
stakeholders. When uncertain, consult the existing codebase or
domain glossary before introducing a new term.

## Development Workflow & CI/CD

All changes MUST follow the established development workflow.

- **Branching**: feature branches off `main`; PRs MUST pass all
  CI checks before merge
- **CI gates** (GitHub Actions):
  - `lint.yml` — ESLint + Prettier on the Vue front-end (PRs to
    `main`)
  - `playwright.yaml` — Playwright E2E suite (daily + PR changes
    to `EndToEndTest/**`)
  - `docker-and-helm.yml` — Docker image build + Helm chart
    packaging (push to `main`, `v*.*.*` tags, PRs)
  - `version-check.yml` — Helm Chart.yaml version comparison
    (reusable workflow)
- **Docker**: Web.Server and Poller each produce a Docker image
  pushed to GHCR; `docker-compose.yml` with profiles (`web`,
  `poller`) for local development
- **Database migrations**: EF Core migrations via
  `Add-Migration` / `Remove-Migration`; applied automatically at
  startup (`Database.Migrate()`)
- **Configuration precedence**: `appsettings.json` →
  `appsettings.{Environment}.json` → environment variables →
  user secrets (Development/Docker)

## Governance

This constitution is the authoritative reference for all development
decisions within the ITA project. It supersedes ad-hoc practices
and informal conventions.

- **Amendment procedure**: any change to this constitution MUST be
  proposed via a PR with a clear rationale. The Sync Impact Report
  (HTML comment at the top of this file) MUST be updated to reflect
  the change.
- **Versioning policy**: this document follows semantic versioning —
  MAJOR for principle removals or incompatible redefinitions, MINOR
  for new principles or materially expanded guidance, PATCH for
  clarifications and non-semantic wording changes.
- **Compliance review**: every PR and code review SHOULD verify
  adherence to the principles above. The `plan-template.md`
  Constitution Check section MUST reference these principles.
- **Template synchronisation**: when principles change, dependent
  templates (`plan-template.md`, `spec-template.md`,
  `tasks-template.md`) MUST be reviewed and updated if affected.

**Version**: 1.2.0 | **Ratified**: 2026-03-25 | **Last Amended**: 2026-04-02
