# Research: Monitoring en Werkverdeling

**Phase 0 output** | **Feature**: 318-monitoring-distribution

## Key Decisions

### 1. Coordinator Role Mechanism

**Decision**: Extend `ITAUser` with two new boolean flags resolved from OIDC claims.

**Rationale**: The existing auth pattern resolves `ITAUser` from OIDC claims in
`AuthenticationExtensions.cs`. Adding `HasOrganisatieCoordinatorAccess` and
`HasTeamCoordinatorAccess` follows the same mechanism as the existing
`HasITASystemAccess` and `HasFunctioneelBeheerderAccess` flags.

**Implementation**:
- Add two new role constants to `Roles.cs` (or create if inline)
- Add boolean properties to `ITAUser.cs`
- Add claim resolution logic in `AuthenticationExtensions.cs`
- Register two new authorization policies in `AuthorizationPolicies.cs`
  (one for "either coordinator role", one per specific role if needed)
- Mirror on frontend in `types/user.ts` and `stores/auth.ts`

### 2. Werklijst Backend Pattern

**Decision**: Follow the `InterneTakenOverzicht` vertical feature slice pattern exactly.

**Rationale**: The existing `InterneTakenOverzicht` feature provides the exact
template — controller → service interface → service → flat DTOs. The werklijst
is essentially a differently-scoped, enriched version of the same data.

**Key differences from existing overzicht**:
- Scope: All `te_verwerken` internetaken (not just the user's own)
- Role-based filtering: Organisatie-coördinator sees all; team-coördinator sees own afdelingen/groepen
- Extra fields: `afhandeltermijn` (deadline), `verstreken` (elapsed days), `overschreden` (overdue flag)
- Enrichment: Requires joining internetaak + klantcontact + actor resolution (same as existing)

### 3. Afhandeltermijn Calculation

**Decision**: Server-side calculation using configurable werkdagen (business days) setting.

**Rationale**: The afhandeltermijn is a business-rule concept — calculating it
server-side keeps the logic centralized and testable. The number of werkdagen
should be configurable without code changes.

**Implementation**:
- `AfhandeltermijnOptions` bound from `appsettings.json` section `Afhandeltermijn`
- Properties: `WerkdagenTermijn` (int, default 5), `FeestdagenCalendar` (string, optional future extension)
- Registration via `IOptions<AfhandeltermijnOptions>` pattern
- Calculation: Count werkdagen (Mon–Fri) from klantcontact `plaatsgevondenOp` date
- The werklijst DTO includes: `afhandeltermijnDatum` (Date), `verstrekenWerkdagen` (int), `isOverschreden` (bool)
- Initially skip Dutch holiday calendar (spec says "vooralsnog werkdagen = ma–vr")

### 4. Werklijst Filtering

**Decision**: Extend existing `InterneTaakQuery` pattern with werklijst-specific filters.

**Filters**:
- `Afdeling` (UUID) — filter by afdeling actor
- `Groep` (UUID) — filter by groep actor  
- `Overdue` (bool) — filter only overdue items
- Implicit filter: `Status = te_verwerken` (always applied for werklijst)

**Scoping** (not user-selectable, derived from role):
- Organisatie-coördinator: No scope restriction
- Team-coördinator: Scoped to own afdelingen/groepen (resolved from `ObjectregisterMedewerkerId` → associated afdelingen/groepen)

### 5. Werkverdeling (Bulk Assignment)

**Decision**: Two-step assignment process following existing `AssignInternetaakToMe` pattern.

**Step 1**: Select internetaken + target afdeling/groep (or direct to medewerker)
**Step 2**: Confirm assignment in dialog (following `BevestigingsModal` pattern)

**Medewerker selection**: Freeform e-mailadres input — NOT a dropdown
picker. This follows the existing `ContactverzoekDoorsturen.vue` pattern
(constitution Principle II: Pattern Reuse Rule). No medewerker list
endpoint or `ObjectApiClient` query-by-groep method exists; building one
would require paging through all medewerkers server-side. The freeform
approach is consistent and pragmatic.

**Backend**: Sequential PATCH calls to Open Klant API per internetaak
(no bulk endpoint available in Open Klant 2.x). The service iterates
selected internetaken and calls `PatchInternetaakActorAsync` for each.

**Error handling**: Partial failure is possible (some PATCHes succeed,
others fail). Return a result summary with per-item status. Frontend
shows toast with count of successes and any failures.

### 6. Team-Coördinator Scope Resolution

**Decision**: Resolve coordinator scope via Objecten API medewerker → afdelingen/groepen mapping.

**Rationale**: The existing `ObjectApiClient` already queries medewerkers with
their associated afdelingen and groepen. A team-coördinator's scope is determined
by looking up their medewerker record and extracting linked afdelingen/groepen.

**Implementation**:
- On werklijst load: resolve current user's `ObjectregisterMedewerkerId` → fetch medewerker → extract afdelingen/groep UUIDs
- Use these UUIDs as scope filter when querying internetaken
- Cache scope for the duration of the request (no session-level caching needed)

## Open Questions (for spec/stakeholder)

None — all questions resolved during `/speckit.clarify`.

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Open Klant API has no bulk PATCH endpoint | Medium — sequential calls for bulk assignment could be slow | Limit bulk selection to reasonable batch size (10-20); show progress indicator |
| Afhandeltermijn without holiday calendar may be inaccurate | Low — stakeholders aware ("vooralsnog") | Design `AfhandeltermijnOptions` to accommodate future holiday calendar |
| Team-coördinator scope resolution adds extra API call | Low — one extra Objecten API call per request | Scope resolved once per request, not per item |
| New OIDC claims may not be provisioned yet in IAM | High — feature blocked without claims | Document required claims in quickstart.md; coordinate with IAM team early |
