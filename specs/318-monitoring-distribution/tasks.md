# Tasks: Monitoring en Werkverdeling

**Input**: Design documents from `/specs/318-monitoring-distribution/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓

**Tests**: E2E Playwright tests included in final phase per constitution Principle IV.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- All paths relative to repository root

---

## Phase 1: User Story 1 — Werklijst inzien (Priority: P1) 🎯 MVP

**Goal**: Coordinators can view a paginated werklijst of all openstaande contactverzoeken scoped to their role — delivered as a complete vertical slice including auth, types, routing, backend API, and frontend view.

**Independent Test**: Log in as each coordinator role and verify the werklijst displays the correct role-scoped set of contactverzoeken with onderwerp, afdeling/groep, medewerker, datum, and kanaal columns

### Setup

- [ ] T001 [US1] Create backend feature directories `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` and `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/`
- [ ] T002 [P] [US1] Create frontend component directory `InterneTaakAfhandeling.Web.Client/src/components/werklijst/`

### Backend Auth & Config

- [ ] T003 [US1] Add coordinator role constants and boolean properties (`HasOrganisatieCoordinatorAccess`, `HasTeamCoordinatorAccess`, `HasCoordinatorAccess`) to `InterneTaakAfhandeling.Web.Server/Authentication/ITAUser.cs` following existing `HasITASystemAccess`/`HasFunctioneelBeheerderAccess` pattern
- [ ] T004 [US1] Add coordinator OIDC claim resolution logic in `InterneTaakAfhandeling.Web.Server/Authentication/AuthenticationExtensions.cs` — map new role claims to ITAUser properties, following existing claim-to-property mapping pattern
- [ ] T005 [P] [US1] Add `CoordinatorPolicy` authorization policy (requires either coordinator role) in `InterneTaakAfhandeling.Web.Server/Authentication/AuthorizationPolicies.cs` following existing `ITAPolicy`/`FunctioneelBeheerderPolicy` pattern
- [ ] T006 [P] [US1] Create `AfhandeltermijnOptions.cs` in `InterneTaakAfhandeling.Web.Server/Config/` with `WerkdagenTermijn` (int, default 5) and `SectionName` constant, using `IOptions<T>` pattern with `ValidateDataAnnotations()` and `ValidateOnStart()` per constitution §II
- [ ] T007 [P] [US1] Add `Afhandeltermijn` section with `WerkdagenTermijn: 5` to `InterneTaakAfhandeling.Web.Server/appsettings.json` and `appsettings.Development.json`
- [ ] T008 [US1] Extend `/api/me` endpoint in `InterneTaakAfhandeling.Web.Server/Authentication/AuthenticationExtensions.cs` to include `hasOrganisatieCoordinatorAccess`, `hasTeamCoordinatorAccess`, and `hasCoordinatorAccess` in the response

### Backend Werklijst API

- [ ] T009 [P] [US1] Create `WerklijstModels.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` with `WerklijstQuery`, `WerklijstOverzichtItem`, and `WerklijstResponse` records per data-model.md — follow `InterneTakenOverzichtModel.cs` pattern
- [ ] T010 [P] [US1] Create `IWerklijstService.cs` interface in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` with `GetWerklijstAsync(WerklijstQuery query, ITAUser user)` method signature
- [ ] T011 [US1] Implement `WerklijstService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` — inject `OpenKlantApiClient` + `ObjectApiClient`, query internetaken with `Status = te_verwerken`, apply role-based scoping (organisatie-coördinator: all; team-coördinator: resolve own afdelingen/groepen from `ObjectregisterMedewerkerId`; dual-role: broadest scope takes precedence per spec edge case), enrich with klantcontact + actor names into flat DTOs, add Serilog structured logging with `SecureLogging.SanitizeAndTruncate()` for PII — follow `InterneTakenOverzichtService.cs` pattern
- [ ] T012 [US1] Create `WerklijstController.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` — `GET /api/werklijst` with `[Authorize(Policy = "CoordinatorPolicy")]`, bind `WerklijstQuery` from query string, return `WerklijstResponse` — follow `AlleOverzichtController.cs` pattern with `[ProducesResponseType]` attributes
- [ ] T013 [US1] Register `IWerklijstService`/`WerklijstService` and bind `AfhandeltermijnOptions` from configuration in `InterneTaakAfhandeling.Web.Server/Config/ServiceCollectionExtensions.cs`

### Frontend Auth & Types

- [ ] T014 [P] [US1] Add `hasOrganisatieCoordinatorAccess`, `hasTeamCoordinatorAccess`, and `hasCoordinatorAccess` boolean fields to the `User` interface in `InterneTaakAfhandeling.Web.Client/src/types/user.ts`
- [ ] T015 [P] [US1] Add coordinator computed properties to auth store in `InterneTaakAfhandeling.Web.Client/src/stores/auth.ts` — `isCoordinator`, `isOrganisatieCoordinator`, `isTeamCoordinator`
- [ ] T016 [P] [US1] Create werklijst TypeScript types (`WerklijstOverzichtItem`, `WerkverdelingRequest`, `WerkverdelingResponse`, `WerkverdelingItemResult`) in `InterneTaakAfhandeling.Web.Client/src/types/werklijst.ts` per data-model.md

### Frontend Werklijst View

- [ ] T017 [P] [US1] Create `werklijstService.ts` in `InterneTaakAfhandeling.Web.Client/src/services/` — `fetchWerklijst(params)` function using `fetchWrapper` with pagination query params, returning `PaginationResponse<WerklijstOverzichtItem>` — follow existing service pattern (e.g., `interneTakenService.ts` or equivalent)
- [ ] T018 [US1] Create `WerklijstTable.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — `<utrecht-table>` displaying columns: onderwerp, afdeling/groep, medewerker, datum klantcontact, kanaal — follow `AllInterneTakenTable.vue` flat DTO pattern, accept `items` prop
- [ ] T019 [US1] Create `WerklijstView.vue` in `InterneTaakAfhandeling.Web.Client/src/views/` — use `usePagination` composable with `fetchWerklijst`, `<simple-spinner>` for loading, `<utrecht-alert>` for errors, `<utrecht-pagination>` for page nav — follow `AllContactverzoekenView.vue` layout pattern

### Frontend Routing & Navigation

- [ ] T020 [US1] Add `/werklijst` route entry with `requiresCoordinatorAccess: true` meta to `InterneTaakAfhandeling.Web.Client/src/router/index.ts` pointing to lazy-loaded `WerklijstView.vue`
- [ ] T021 [US1] Add coordinator route guard logic in `InterneTaakAfhandeling.Web.Client/src/plugins/routerGuards.ts` — redirect non-coordinators, following existing `requiresITAAccess`/`requiresFunctioneelBeheerderAccess` guard pattern
- [ ] T022 [US1] Add werklijst navigation link (visible only to coordinators) to `InterneTaakAfhandeling.Web.Client/src/components/TheHeader.vue` — follow existing nav item pattern for role-scoped items (cf. Beheer link conditional on `hasFunctioneelBeheerderAccess`)

### E2E Tests & Documentation

- [ ] T023 [P] [US1] Create E2E test locators in `InterneTaakAfhandeling.EndToEndTest/Werklijst/WerklijstLocators.cs` — define locators for werklijst table, filter controls, assignment dialog, pagination, overdue indicators
- [ ] T024 [P] [US1] Create E2E werklijst scenarios in `InterneTaakAfhandeling.EndToEndTest/Werklijst/WerklijstScenarios.cs` — test coordinator login → werklijst visible, role-scoped data, pagination — follow existing `{Feature}Scenarios` naming and Page Object pattern
- [ ] T025 [US1] Update `docs/domain-glossary.md` with new terms: werklijst, werkverdeling, afhandeltermijn, coördinator roles

**Checkpoint**: User Story 1 complete — coordinators see a paginated, role-scoped werklijst. This is the MVP. Auth, types, routing, backend API, frontend view, and E2E tests all delivered as one vertical slice.

---

## Phase 2: User Story 4 — Werkverdeling / bulk toewijzen (Priority: P1)

**Goal**: Coordinators can select one or more contactverzoeken from the werklijst and assign them to a different afdeling/groep and/or medewerker

**Independent Test**: Select multiple contactverzoeken, assign to a different afdeling/groep + medewerker, verify assignments update and partial failures are reported

**Depends on**: US1 (werklijst must exist for selection)

### Backend

- [ ] T026 [P] [US4] Create `WerkverdelingModels.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` with `WerkverdelingRequest` (with `MedewerkerEmail` instead of UUID), `WerkverdelingResponse`, and `WerkverdelingItemResult` records per data-model.md
- [ ] T027 [P] [US4] Create `IWerkverdelingService.cs` interface in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` with `AssignInternetakenAsync(WerkverdelingRequest request, ITAUser user)` method signature
- [ ] T028 [US4] Implement `WerkverdelingService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` — iterate `InternetaakUuids`, get/create Actor for target, PATCH each internetaak via `OpenKlantApiClient.PatchInternetaakActorAsync`, resolve optional `MedewerkerEmail` to actor (following `ForwardContactRequestService` pattern), collect per-item success/failure results, add Serilog structured logging with PII sanitization — follow `AssignInternetaakToMeService` pattern
- [ ] T029 [US4] Create `WerkverdelingController.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` — `POST /api/werkverdeling` with `[Authorize(Policy = "CoordinatorPolicy")]`, validate request (non-empty UUIDs, max 20, at least one of afdelingUuid/groepUuid required, medewerkerEmail optional with email format validation), return `WerkverdelingResponse` — follow existing controller pattern with `[ProducesResponseType]`
- [ ] T030 [US4] Register `IWerkverdelingService`/`WerkverdelingService` in `InterneTaakAfhandeling.Web.Server/Config/ServiceCollectionExtensions.cs`

### Frontend

- [ ] T031 [P] [US4] Add werkverdeling API methods to `InterneTaakAfhandeling.Web.Client/src/services/werklijstService.ts` — `postWerkverdeling(request)` using existing `/api/afdelingen` and `/api/groepen` endpoints for the picker data
- [ ] T032 [US4] Create `WerkverdelingDialog.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — `<utrecht-alert-dialog>` modal with two-step flow: Step 1 select afdeling/groep (ALL afdelingen/groepen shown via existing `/api/afdelingen` + `/api/groepen`), Step 2 optionally enter medewerker e-mailadres (freeform `<utrecht-textbox type="email">` with client-side validation, following `ContactverzoekDoorsturen.vue` pattern) — follow `BevestigingsModal` pattern, emit `success`/`cancel` events
- [ ] T033 [US4] Add row checkbox selection and "Toewijzen" action button to `WerklijstTable.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — emit selected UUIDs, disable button when no selection
- [ ] T034 [US4] Integrate `WerkverdelingDialog` into `WerklijstView.vue` — wire checkbox selection to dialog trigger, handle success (toast + refresh werklijst) and partial failure (warning toast with counts), handle cancel — follow existing toast feedback pattern

### E2E Tests & Build Validation

- [ ] T035 [P] [US4] Create E2E werkverdeling scenarios in `InterneTaakAfhandeling.EndToEndTest/Werklijst/WerkverdelingScenarios.cs` — test single + bulk assignment flow, partial failure handling, verify contactverzoek disappears from team-coordinator werklijst after out-of-scope reassignment (FR-009), coordinator detail-page access (verify existing Doorsturen/Toewijzen actions work for coordinator roles) — follow existing scenario pattern
- [ ] T036 [US4] Verify full build succeeds (`dotnet build` for backend, `npm run build` for frontend) and no lint errors
- [ ] T037 [US4] Run quickstart.md validation — verify local dev setup instructions work end-to-end

**Checkpoint**: User Stories 1 + 4 complete — coordinators can view the werklijst AND perform bulk assignment. Core P1 features delivered. Full build validated.

---

## Phase 3: User Story 2 — Werklijst filteren (Priority: P2)

**Goal**: Coordinators can filter the werklijst by afdeling/groep, with filter options scoped to their role

**Independent Test**: Log in as each coordinator role, apply afdeling/groep filters, verify werklijst narrows correctly and filter options match role scope

**Depends on**: US1 (werklijst must exist to filter it)

- [ ] T038 [P] [US2] Extend `WerklijstService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` to apply `AfdelingUuid` and `GroepUuid` filters from `WerklijstQuery` when building the OpenKlant API query
- [ ] T039 [P] [US2] Add endpoint or logic in `WerklijstController.cs` to return role-scoped filter options (afdelingen/groepen the user may filter on) — organisatie-coördinator: all, team-coördinator: own scope only
- [ ] T040 [P] [US2] Add `fetchFilterOptions()` method to `InterneTaakAfhandeling.Web.Client/src/services/werklijstService.ts` to retrieve role-scoped afdelingen/groepen for the filter dropdown (reuses existing `/api/afdelingen` + `/api/groepen` endpoints for organisatie-coördinator; uses scope-specific endpoint for team-coördinator)
- [ ] T041 [US2] Create `WerklijstFilter.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — afdeling/groep dropdown(s) using Utrecht form components, emit selected filter values, include clear-filter action
- [ ] T042 [US2] Integrate `WerklijstFilter.vue` into `WerklijstView.vue` — connect filter state to werklijst query params, re-fetch werklijst on filter change, reset pagination to page 1 on filter change

**Checkpoint**: User Story 2 complete — filtering works independently. Werklijst is now usable at scale.

---

## Phase 4: User Story 3 — Afhandeltermijn inzien (Priority: P2)

**Goal**: Each werklijst row shows elapsed werkdagen since klantcontact and a visual overdue indicator based on configurable afhandeltermijn

**Independent Test**: Create contactverzoeken with known dates, configure afhandeltermijn, verify elapsed time and overdue status display correctly in the werklijst

**Depends on**: US1 (werklijst must exist to show afhandeltermijn columns)

- [ ] T043 [US3] Implement werkdagen calculation logic in `WerklijstService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` — count business days (Mon–Fri) from `klantcontact.plaatsgevondenOp` to today, compute `AfhandeltermijnDatum`, `VerstrekenWerkdagen`, and `IsOverschreden` using injected `IOptions<AfhandeltermijnOptions>` — populate these fields on each `WerklijstOverzichtItem`
- [ ] T044 [US3] Add `alleenOverschreden` filter support to `WerklijstService.cs` — when `AlleenOverschreden = true` in query, post-filter results to only include items where `IsOverschreden == true`
- [ ] T045 [US3] Add afhandeltermijn columns to `WerklijstTable.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — display `verstrekenWerkdagen` / `afhandeltermijnDatum`, add visual overdue indicator (color/icon) when `isOverschreden` is true — follow Utrecht component styling conventions

**Checkpoint**: User Story 3 complete — coordinators can identify overdue items at a glance. Core monitoring value delivered.

---

## Dependencies & Execution Order

### Phase Dependencies

- **US1 (Phase 1)**: No dependencies — complete vertical slice including auth, types, routing, backend API, frontend view, E2E tests, and docs. MVP target
- **US4 (Phase 2)**: Depends on US1 (werklijst must exist for selection UI). Includes E2E tests + build validation
- **US2 (Phase 3)**: Depends on US1 (werklijst must exist to add filters)
- **US3 (Phase 4)**: Depends on US1 (werklijst must exist to add columns)

### User Story Dependencies

```
Phase 1 (US1: Werklijst + Auth + Routing + E2E) ─── MVP ───
               /    |    \
              ▼     ▼     ▼
     Phase 3  Phase 4  Phase 2
     (US2)    (US3)    (US4: Werkverdeling + E2E + Build)
```

- **US2 and US3 are independent** of each other — can be implemented in parallel
- **US2 and US3 are independent** of US4 — can be implemented in parallel

### Within Each User Story

- Models/DTOs before services
- Services before controllers
- Backend before frontend (API must exist for frontend to call)
- Core implementation before integration wiring

### Parallel Opportunities

- **Phase 1 (US1)**: T002, T005, T006, T007, T009, T010, T014, T015, T016, T017 can run in parallel batches (different files). Backend auth → backend API → frontend auth → frontend view → E2E is the natural flow within the slice.
- **Phase 2 (US4)**: T026 + T027 parallel, then T028/T029. T031 parallel with backend. T032 after T031. T035 (E2E) after frontend.
- **Phase 3 + 4**: Entire phases can run in parallel (independent stories, different files)

---

## Parallel Examples

### US1 Vertical Slice Batches

```
Batch A (parallel — setup + auth):
  T001: Create directories
  T002: Create frontend directory
  T003: ITAUser.cs coordinator properties
  T005: AuthorizationPolicies.cs
  T006: AfhandeltermijnOptions.cs
  T007: appsettings.json

Batch B (sequential — depends on T003/T004):
  T004: AuthenticationExtensions.cs claim resolution
  T008: /api/me coordinator flags

Batch C (parallel — backend models + frontend types):
  T009: WerklijstModels.cs
  T010: IWerklijstService.cs
  T014: types/user.ts
  T015: stores/auth.ts
  T016: types/werklijst.ts
  T017: werklijstService.ts

Batch D (sequential — depends on C):
  T011: WerklijstService.cs
  T012: WerklijstController.cs
  T013: ServiceCollectionExtensions.cs

Batch E (sequential — depends on D + frontend types):
  T018: WerklijstTable.vue
  T019: WerklijstView.vue
  T020: router/index.ts
  T021: routerGuards.ts
  T022: TheHeader.vue navigation link

Batch F (E2E — depends on E):
  T023: WerklijstLocators.cs
  T024: WerklijstScenarios.cs
  T025: domain-glossary.md
```

### US2 + US3 Full Parallel

```
Developer A (US2 — filtering):
  T038 → T039 → T040 → T041 → T042

Developer B (US3 — afhandeltermijn):
  T043 → T044 → T045
```

---

## Implementation Strategy

### MVP First (US1 — Single Vertical Slice)

1. Complete Phase 1: US1 — Werklijst inzien (auth + types + routing + backend API + frontend view)
2. **STOP and VALIDATE**: Coordinators can view a paginated, role-scoped werklijst
3. Deploy/demo if ready — this is the minimum viable product

### Incremental Delivery

1. US1 → Test independently → **Deploy (MVP!)**
2. Add US4 → Test independently → Deploy (core workflow: view + assign)
3. Add US2 + US3 in parallel → Test independently → Deploy (full monitoring)
4. Final E2E validation → Release

### Parallel Team Strategy

With 2 developers after US1:

1. Both complete US1 together (vertical slice)
2. **Developer A**: US4 (Phase 2)
3. **Developer B**: US2 (Phase 3) + US3 (Phase 4) in parallel
4. Both: Final E2E validation and release

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps each task to its user story for traceability
- All backend patterns follow existing vertical feature slices in `Features/`
- All frontend patterns follow existing Composition API + Utrecht components
- Dutch domain terms used throughout (werklijst, werkverdeling, afhandeltermijn, coördinator)
- AfhandeltermijnOptions fields default to 0/false in the DTO — US3 populates them with calculated values, US1 can ship without displaying those columns
- US1 is a complete vertical slice: auth → types → routing → backend API → frontend view
- No separate "foundational" phase — all infrastructure is delivered through user stories
- Commit after each task or logical group
- Stop at any checkpoint to validate the story independently
