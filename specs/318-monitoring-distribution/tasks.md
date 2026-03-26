# Tasks: Monitoring en Werkverdeling

**Input**: Design documents from `/specs/318-monitoring-distribution/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓

**Tests**: E2E Playwright tests included in final phase per constitution Principle IV.

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US5)
- All paths relative to repository root

---

## Phase 1: Setup

**Purpose**: Create new directories and boilerplate files for the feature

- [ ] T001 Create backend feature directories `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` and `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/`
- [ ] T002 [P] Create frontend component directory `InterneTaakAfhandeling.Web.Client/src/components/werklijst/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Coordinator authentication/authorization, shared types, configuration, and routing — MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Backend Auth & Config

- [ ] T003 Add coordinator role constants and boolean properties (`HasOrganisatieCoordinatorAccess`, `HasTeamCoordinatorAccess`, `HasCoordinatorAccess`) to `InterneTaakAfhandeling.Web.Server/Authentication/ITAUser.cs` following existing `HasITASystemAccess`/`HasFunctioneelBeheerderAccess` pattern
- [ ] T004 Add coordinator OIDC claim resolution logic in `InterneTaakAfhandeling.Web.Server/Authentication/AuthenticationExtensions.cs` — map new role claims to ITAUser properties, following existing claim-to-property mapping pattern
- [ ] T005 [P] Add `CoordinatorPolicy` authorization policy (requires either coordinator role) in `InterneTaakAfhandeling.Web.Server/Authentication/AuthorizationPolicies.cs` following existing `ITAPolicy`/`FunctioneelBeheerderPolicy` pattern
- [ ] T006 [P] Create `AfhandeltermijnOptions.cs` in `InterneTaakAfhandeling.Web.Server/Config/` with `WerkdagenTermijn` (int, default 5) and `SectionName` constant, using `IOptions<T>` pattern
- [ ] T007 [P] Add `Afhandeltermijn` section with `WerkdagenTermijn: 5` to `InterneTaakAfhandeling.Web.Server/appsettings.json` and `appsettings.Development.json`
- [ ] T008 Extend `/api/me` endpoint in `InterneTaakAfhandeling.Web.Server/Authentication/AuthenticationExtensions.cs` to include `hasOrganisatieCoordinatorAccess`, `hasTeamCoordinatorAccess`, and `hasCoordinatorAccess` in the response

### Frontend Auth & Types

- [ ] T009 [P] Add `hasOrganisatieCoordinatorAccess`, `hasTeamCoordinatorAccess`, and `hasCoordinatorAccess` boolean fields to the `User` interface in `InterneTaakAfhandeling.Web.Client/src/types/user.ts`
- [ ] T010 [P] Add coordinator computed properties to auth store in `InterneTaakAfhandeling.Web.Client/src/stores/auth.ts` — `isCoordinator`, `isOrganisatieCoordinator`, `isTeamCoordinator`
- [ ] T011 [P] Create werklijst TypeScript types (`WerklijstOverzichtItem`, `WerkverdelingRequest`, `WerkverdelingResponse`, `WerkverdelingItemResult`) in `InterneTaakAfhandeling.Web.Client/src/types/werklijst.ts` per data-model.md

### Frontend Routing

- [ ] T012 Add `/werklijst` route entry with `requiresCoordinatorAccess: true` meta to `InterneTaakAfhandeling.Web.Client/src/router/index.ts` pointing to lazy-loaded `WerklijstView.vue`
- [ ] T013 Add coordinator route guard logic in `InterneTaakAfhandeling.Web.Client/src/plugins/routerGuards.ts` — redirect non-coordinators, following existing `requiresITAAccess`/`requiresFunctioneelBeheerderAccess` guard pattern

**Checkpoint**: Foundation ready — coordinator auth end-to-end, types defined, route accessible. User story implementation can begin.

---

## Phase 3: User Story 1 — Werklijst inzien (Priority: P1) 🎯 MVP

**Goal**: Coordinators can view a paginated werklijst of all openstaande contactverzoeken scoped to their role

**Independent Test**: Log in as each coordinator role and verify the werklijst displays the correct role-scoped set of contactverzoeken with onderwerp, afdeling/groep, medewerker, datum, and kanaal columns

### Backend

- [ ] T014 [P] [US1] Create `WerklijstModels.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` with `WerklijstQuery`, `WerklijstOverzichtItem`, and `WerklijstResponse` records per data-model.md — follow `InterneTakenOverzichtModel.cs` pattern
- [ ] T015 [P] [US1] Create `IWerklijstService.cs` interface in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` with `GetWerklijstAsync(WerklijstQuery query, ITAUser user)` method signature
- [ ] T016 [US1] Implement `WerklijstService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` — inject `OpenKlantApiClient` + `ObjectApiClient`, query internetaken with `Status = te_verwerken`, apply role-based scoping (organisatie-coördinator: all; team-coördinator: resolve own afdelingen/groepen from `ObjectregisterMedewerkerId`; dual-role: broadest scope takes precedence per spec edge case), enrich with klantcontact + actor names into flat DTOs, add Serilog structured logging with `SecureLogging.SanitizeAndTruncate()` for PII — follow `InterneTakenOverzichtService.cs` pattern
- [ ] T017 [US1] Create `WerklijstController.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` — `GET /api/werklijst` with `[Authorize(Policy = "CoordinatorPolicy")]`, bind `WerklijstQuery` from query string, return `WerklijstResponse` — follow `AlleOverzichtController.cs` pattern with `[ProducesResponseType]` attributes
- [ ] T018 [US1] Register `IWerklijstService`/`WerklijstService` and bind `AfhandeltermijnOptions` from configuration in `InterneTaakAfhandeling.Web.Server/Config/ServiceCollectionExtensions.cs`

### Frontend

- [ ] T019 [P] [US1] Create `werklijstService.ts` in `InterneTaakAfhandeling.Web.Client/src/services/` — `fetchWerklijst(params)` function using `fetchWrapper` with pagination query params, returning `PaginationResponse<WerklijstOverzichtItem>` — follow existing service pattern (e.g., `interneTakenService.ts` or equivalent)
- [ ] T020 [US1] Create `WerklijstTable.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — `<utrecht-table>` displaying columns: onderwerp, afdeling/groep, medewerker, datum klantcontact, kanaal — follow `AllInterneTakenTable.vue` flat DTO pattern, accept `items` prop
- [ ] T021 [US1] Create `WerklijstView.vue` in `InterneTaakAfhandeling.Web.Client/src/views/` — use `usePagination` composable with `fetchWerklijst`, `<simple-spinner>` for loading, `<utrecht-alert>` for errors, `<utrecht-pagination>` for page nav — follow `AllContactverzoekenView.vue` layout pattern
- [ ] T022 [US1] Add werklijst navigation link (visible only to coordinators) to `InterneTaakAfhandeling.Web.Client/src/components/TheHeader.vue` — follow existing nav item pattern for role-scoped items (cf. Beheer link conditional on `hasFunctioneelBeheerderAccess`)

**Checkpoint**: User Story 1 complete — coordinators see a paginated, role-scoped werklijst. This is the MVP.

---

## Phase 4: User Story 4 — Werkverdeling / bulk toewijzen (Priority: P1)

**Goal**: Coordinators can select one or more contactverzoeken from the werklijst and assign them to a different afdeling/groep and/or medewerker

**Independent Test**: Select multiple contactverzoeken, assign to a different afdeling/groep + medewerker, verify assignments update and partial failures are reported

**Depends on**: US1 (werklijst must exist for selection)

### Backend

- [ ] T023 [P] [US4] Create `WerkverdelingModels.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` with `WerkverdelingRequest` (with `MedewerkerEmail` instead of UUID), `WerkverdelingResponse`, and `WerkverdelingItemResult` records per data-model.md
- [ ] T024 [P] [US4] Create `IWerkverdelingService.cs` interface in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` with `AssignInternetakenAsync(WerkverdelingRequest request, ITAUser user)` method signature
- [ ] T025 [US4] Implement `WerkverdelingService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` — iterate `InternetaakUuids`, get/create Actor for target, PATCH each internetaak via `OpenKlantApiClient.PatchInternetaakActorAsync`, resolve optional `MedewerkerEmail` to actor (following `ForwardContactRequestService` pattern), collect per-item success/failure results, add Serilog structured logging with PII sanitization — follow `AssignInternetaakToMeService` pattern
- [ ] T026 [US4] Create `WerkverdelingController.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werkverdeling/` — `POST /api/werkverdeling` with `[Authorize(Policy = "CoordinatorPolicy")]`, validate request (non-empty UUIDs, max 20, at least one target, email format if provided), return `WerkverdelingResponse` — follow existing controller pattern with `[ProducesResponseType]`
- [ ] T027 [US4] Register `IWerkverdelingService`/`WerkverdelingService` in `InterneTaakAfhandeling.Web.Server/Config/ServiceCollectionExtensions.cs`

### Frontend

- [ ] T028 [P] [US4] Add werkverdeling API methods to `InterneTaakAfhandeling.Web.Client/src/services/werklijstService.ts` — `postWerkverdeling(request)` using existing `/api/afdelingen` and `/api/groepen` endpoints for the picker data
- [ ] T029 [US4] Create `WerkverdelingDialog.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — `<utrecht-alert-dialog>` modal with two-step flow: Step 1 select afdeling/groep (ALL afdelingen/groepen shown via existing `/api/afdelingen` + `/api/groepen`), Step 2 optionally enter medewerker e-mailadres (freeform `<utrecht-textbox type="email">` with client-side validation, following `ContactverzoekDoorsturen.vue` pattern) — follow `BevestigingsModal` pattern, emit `success`/`cancel` events
- [ ] T030 [US4] Add row checkbox selection and "Toewijzen" action button to `WerklijstTable.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — emit selected UUIDs, disable button when no selection
- [ ] T031 [US4] Integrate `WerkverdelingDialog` into `WerklijstView.vue` — wire checkbox selection to dialog trigger, handle success (toast + refresh werklijst) and partial failure (warning toast with counts), handle cancel — follow existing toast feedback pattern

**Checkpoint**: User Stories 1 + 4 complete — coordinators can view the werklijst AND perform bulk assignment. Core P1 features delivered.

---

## Phase 5: User Story 2 — Werklijst filteren (Priority: P2)

**Goal**: Coordinators can filter the werklijst by afdeling/groep, with filter options scoped to their role

**Independent Test**: Log in as each coordinator role, apply afdeling/groep filters, verify werklijst narrows correctly and filter options match role scope

**Depends on**: US1 (werklijst must exist to filter it)

- [ ] T032 [P] [US2] Extend `WerklijstService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` to apply `AfdelingUuid` and `GroepUuid` filters from `WerklijstQuery` when building the OpenKlant API query
- [ ] T033 [P] [US2] Add endpoint or logic in `WerklijstController.cs` to return role-scoped filter options (afdelingen/groepen the user may filter on) — organisatie-coördinator: all, team-coördinator: own scope only
- [ ] T034 [P] [US2] Add `fetchFilterOptions()` method to `InterneTaakAfhandeling.Web.Client/src/services/werklijstService.ts` to retrieve role-scoped afdelingen/groepen for the filter dropdown (reuses existing `/api/afdelingen` + `/api/groepen` endpoints for organisatie-coördinator; uses scope-specific endpoint for team-coördinator)
- [ ] T035 [US2] Create `WerklijstFilter.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — afdeling/groep dropdown(s) using Utrecht form components, emit selected filter values, include clear-filter action
- [ ] T036 [US2] Integrate `WerklijstFilter.vue` into `WerklijstView.vue` — connect filter state to werklijst query params, re-fetch werklijst on filter change, reset pagination to page 1 on filter change

**Checkpoint**: User Story 2 complete — filtering works independently. Werklijst is now usable at scale.

---

## Phase 6: User Story 3 — Afhandeltermijn inzien (Priority: P2)

**Goal**: Each werklijst row shows elapsed werkdagen since klantcontact and a visual overdue indicator based on configurable afhandeltermijn

**Independent Test**: Create contactverzoeken with known dates, configure afhandeltermijn, verify elapsed time and overdue status display correctly in the werklijst

**Depends on**: US1 (werklijst must exist to show afhandeltermijn columns)

- [ ] T037 [US3] Implement werkdagen calculation logic in `WerklijstService.cs` in `InterneTaakAfhandeling.Web.Server/Features/Werklijst/` — count business days (Mon–Fri) from `klantcontact.plaatsgevondenOp` to today, compute `AfhandeltermijnDatum`, `VerstrekenWerkdagen`, and `IsOverschreden` using injected `IOptions<AfhandeltermijnOptions>` — populate these fields on each `WerklijstOverzichtItem`
- [ ] T038 [US3] Add `alleenOverschreden` filter support to `WerklijstService.cs` — when `AlleenOverschreden = true` in query, post-filter results to only include items where `IsOverschreden == true`
- [ ] T039 [US3] Add afhandeltermijn columns to `WerklijstTable.vue` in `InterneTaakAfhandeling.Web.Client/src/components/werklijst/` — display `verstrekenWerkdagen` / `afhandeltermijnDatum`, add visual overdue indicator (color/icon) when `isOverschreden` is true — follow Utrecht component styling conventions

**Checkpoint**: User Story 3 complete — coordinators can identify overdue items at a glance. Core monitoring value delivered.

---

## Phase 7: User Story 5 — Toewijzing wijzigen vanuit detail (Priority: P3)

**Goal**: Coordinators can change a contactverzoek's toewijzing from its detail page using the same two-step assignment flow

**Independent Test**: Open a contactverzoek detail page as a coordinator, change its toewijzing via the assignment dialog, verify the change persists and the werklijst reflects the update

**Depends on**: US4 (reuses `WerkverdelingDialog` component)

- [ ] T040 [US5] Ensure coordinator roles have access to contactverzoek detail pages — verify existing detail view route guards allow coordinator roles, add `requiresCoordinatorAccess` handling if needed in `InterneTaakAfhandeling.Web.Client/src/plugins/routerGuards.ts`
- [ ] T041 [US5] Add "Toewijzing wijzigen" button to the contactverzoek detail view for coordinator roles — reuse `WerkverdelingDialog.vue` component in single-item mode (pre-fill with single UUID), wire success event to refresh detail view data

**Checkpoint**: All user stories complete — full monitoring and werkverdeling feature set delivered.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: E2E tests, documentation, cleanup

- [ ] T042 [P] Create E2E test locators in `InterneTaakAfhandeling.EndToEndTest/Werklijst/WerklijstLocators.cs` — define locators for werklijst table, filter controls, assignment dialog, pagination, overdue indicators
- [ ] T043 [P] Create E2E werklijst scenarios in `InterneTaakAfhandeling.EndToEndTest/Werklijst/WerklijstScenarios.cs` — test coordinator login → werklijst visible, role-scoped data, pagination, filtering, afhandeltermijn display — follow existing `{Feature}Scenarios` naming and Page Object pattern
- [ ] T044 [P] Create E2E werkverdeling scenarios in `InterneTaakAfhandeling.EndToEndTest/Werklijst/WerkverdelingScenarios.cs` — test single + bulk assignment flow, partial failure handling, detail-page assignment — follow existing scenario pattern
- [ ] T045 Update `docs/domain-glossary.md` with new terms: werklijst, werkverdeling, afhandeltermijn, coördinator roles
- [ ] T046 Verify full build succeeds (`dotnet build` for backend, `npm run build` for frontend) and no lint errors
- [ ] T047 Run quickstart.md validation — verify local dev setup instructions work end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Phase 2 — MVP target
- **US4 (Phase 4)**: Depends on US1 (werklijst must exist for selection UI)
- **US2 (Phase 5)**: Depends on US1 (werklijst must exist to add filters)
- **US3 (Phase 6)**: Depends on US1 (werklijst must exist to add columns)
- **US5 (Phase 7)**: Depends on US4 (reuses WerkverdelingDialog component)
- **Polish (Phase 8)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 1 (Setup) → Phase 2 (Foundational)
                        │
                        ▼
                   Phase 3 (US1: Werklijst) ─── MVP ───
                   /    |    \
                  ▼     ▼     ▼
         Phase 5  Phase 6  Phase 4
         (US2)    (US3)    (US4: Werkverdeling)
                              │
                              ▼
                         Phase 7 (US5: Detail)
                              │
                              ▼
                         Phase 8 (Polish)
```

- **US2 and US3 are independent** of each other — can be implemented in parallel
- **US2 and US3 are independent** of US4 — can be implemented in parallel
- **US5 depends on US4** — reuses the assignment dialog component

### Within Each User Story

- Models/DTOs before services
- Services before controllers
- Backend before frontend (API must exist for frontend to call)
- Core implementation before integration wiring

### Parallel Opportunities

- **Phase 2**: T005, T006, T007, T009, T010, T011 can all run in parallel (different files)
- **Phase 3**: T014 + T015 parallel (models + interface), then T016 (service), then T017 (controller). T019 parallel with backend. T020 after T019.
- **Phase 4**: T023 + T024 parallel, then T025/T026. T028 parallel with backend. T029 after T028.
- **Phase 5 + 6**: Entire phases can run in parallel (independent stories, different files)
- **Phase 8**: T042, T043, T044 all parallel (different test files)

---

## Parallel Examples

### Phase 2 Parallel Batch

```
Batch A (parallel — all different files):
  T005: AuthorizationPolicies.cs
  T006: AfhandeltermijnOptions.cs
  T007: appsettings.json
  T009: types/user.ts
  T010: stores/auth.ts
  T011: types/werklijst.ts
```

### US1 Backend + Frontend Parallel

```
Batch B (parallel — backend models):
  T014: WerklijstModels.cs
  T015: IWerklijstService.cs

Batch C (sequential — depends on Batch B):
  T016: WerklijstService.cs
  T017: WerklijstController.cs
  T018: ServiceCollectionExtensions.cs

Batch D (parallel with Batch B — frontend types ready from Phase 2):
  T019: werklijstService.ts

Batch E (sequential — depends on D):
  T020: WerklijstTable.vue
  T021: WerklijstView.vue
  T022: Navigation link
```

### US2 + US3 Full Parallel

```
Developer A (US2 — filtering):
  T032 → T033 → T034 → T035 → T036

Developer B (US3 — afhandeltermijn):
  T037 → T038 → T039
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks everything)
3. Complete Phase 3: US1 — Werklijst inzien
4. **STOP and VALIDATE**: Coordinators can view a paginated, role-scoped werklijst
5. Deploy/demo if ready — this is the minimum viable product

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Test independently → **Deploy (MVP!)**
3. Add US4 → Test independently → Deploy (core workflow: view + assign)
4. Add US2 + US3 in parallel → Test independently → Deploy (full monitoring)
5. Add US5 → Test independently → Deploy (complete feature)
6. Polish + E2E tests → Final release

### Parallel Team Strategy

With 2 developers after Phase 2:

1. Both complete Setup + Foundational together
2. **Developer A**: US1 (Phase 3) → US4 (Phase 4) → US5 (Phase 7)
3. **Developer B**: Waits for US1 checkpoint → US2 (Phase 5) + US3 (Phase 6) in parallel
4. Both: Phase 8 (Polish)

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps each task to its user story for traceability
- All backend patterns follow existing vertical feature slices in `Features/`
- All frontend patterns follow existing Composition API + Utrecht components
- Dutch domain terms used throughout (werklijst, werkverdeling, afhandeltermijn, coördinator)
- AfhandeltermijnOptions fields are nullable in DTO — US3 populates them, US1 can ship without
- Commit after each task or logical group
- Stop at any checkpoint to validate the story independently
