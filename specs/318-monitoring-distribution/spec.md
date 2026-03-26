# Feature Specification: Monitoring en Werkverdeling

**Feature Branch**: `318-monitoring-distribution`
**Created**: 2026-03-25
**Status**: Draft
**Input**: GitHub Issue #318 — Monitoring en Werkverdeling

## Clarifications

### Session 2026-03-25

- Q: Where does the werklijst live in the application navigation? → A: New dedicated top-level page at route `/werklijst`, visible only to coordinator roles.
- Q: What data columns does each werklijst row display? → A: Onderwerp, assigned afdeling/groep, assigned medewerker, datum klantcontact, elapsed werkdagen, overdue indicator, kanaal.
- Q: Is the werklijst paginated? → A: Yes, paginated — consistent with the existing Alle Contactverzoeken view.

## User Scenarios & Testing *(mandatory)*

### User Story 1 — Werklijst inzien (Priority: P1)

As a coördinator I want to view a werklijst (work list) of all
openstaande contactverzoeken (interne taken) so that I can monitor
workload and identify items that need attention.

Two new roles determine the scope of the werklijst:

- **Organisatie-coördinator**: sees contactverzoeken across the
  entire organisation
- **Team-coördinator**: sees only contactverzoeken assigned to
  their own afdelingen/groepen

**Why this priority**: Without a werklijst there is nothing to
monitor or act on — every other story depends on this view.

**Independent Test**: Log in as each coordinator role and verify the
werklijst displays the correct set of contactverzoeken scoped to
the role.

**Acceptance Scenarios**:

1. **Given** a user with the organisatie-coördinator role,
   **When** they open the werklijst,
   **Then** they see all openstaande contactverzoeken across all
   afdelingen/groepen in the organisation.
2. **Given** a user with the team-coördinator role who belongs to
   afdelingen A and B,
   **When** they open the werklijst,
   **Then** they see only openstaande contactverzoeken assigned to
   afdeling A or B (or groepen within those afdelingen).
3. **Given** a contactverzoek is afgehandeld (status: verwerkt),
   **When** any coördinator views their werklijst,
   **Then** that contactverzoek no longer appears.

---

### User Story 2 — Werklijst filteren (Priority: P2)

As a coördinator I want to filter the werklijst by
afdeling/groep so that I can focus on a specific team's workload.

Filter options are scoped to the coordinator role:

- **Organisatie-coördinator**: can filter on ALL afdelingen/groepen
- **Team-coördinator**: can filter only on their OWN
  afdelingen/groepen

**Why this priority**: Filtering is essential for any werklijst
with more than a few items — it makes the monitoring view usable
in practice.

**Independent Test**: Log in as each coordinator role, apply
filters, and verify the werklijst narrows correctly and the
available filter options match the role scope.

**Acceptance Scenarios**:

1. **Given** an organisatie-coördinator viewing the werklijst,
   **When** they open the filter,
   **Then** they see all afdelingen/groepen in the organisation as
   filter options.
2. **Given** a team-coördinator belonging to afdelingen A and B,
   **When** they open the filter,
   **Then** they see only afdeling A and B (and their groepen) as
   filter options.
3. **Given** any coördinator with an active filter on afdeling A,
   **When** they view the werklijst,
   **Then** only contactverzoeken assigned to afdeling A (or its
   groepen) are shown.
4. **Given** a coördinator with an active filter,
   **When** they clear the filter,
   **Then** the werklijst returns to its default (full role-scoped)
   view.

---

### User Story 3 — Afhandeltermijn inzien (Priority: P2)

As a coördinator I want to see how long each contactverzoek has
been open compared to the configured maximum afhandeltermijn so
that I can identify overdue items and prioritise accordingly.

The elapsed time is calculated from the date the first
klantcontact took place. The maximum afhandeltermijn is a
configurable value expressed in werkdagen (business days).

**Why this priority**: Same level as filtering — duration
visibility is the core monitoring value that triggers the
coordinator to intervene.

**Independent Test**: Create contactverzoeken with known
klantcontact dates, configure the afhandeltermijn, and verify the
werklijst correctly displays elapsed time and overdue status.

**Acceptance Scenarios**:

1. **Given** a contactverzoek whose first klantcontact was 3
   werkdagen ago and the afhandeltermijn is set to 5 werkdagen,
   **When** a coördinator views the werklijst,
   **Then** the contactverzoek shows it has been open for 3 of 5
   werkdagen and is NOT marked as overdue.
2. **Given** a contactverzoek whose first klantcontact was 6
   werkdagen ago and the afhandeltermijn is set to 5 werkdagen,
   **When** a coördinator views the werklijst,
   **Then** the contactverzoek is clearly marked as overdue.
3. **Given** an administrator changes the afhandeltermijn
   configuration,
   **When** a coördinator refreshes the werklijst,
   **Then** the overdue calculation reflects the new value.

---

### User Story 4 — Werkverdeling (bulk toewijzen) (Priority: P1)

As a coördinator I want to assign one or more contactverzoeken
from the werklijst to a different afdeling/groep and/or a
medewerker so that work is distributed effectively.

Assignment follows a two-step selection:

1. Select a target **afdeling/groep** — for BOTH coordinator roles
   the picker shows ALL afdelingen/groepen (not filtered to own
   scope)
2. Optionally enter a **medewerker e-mailadres** — a freeform email
   input (consistent with the existing ContactverzoekDoorsturen
   pattern; no medewerker list endpoint exists)

Both single-item and bulk (multi-select) assignment MUST be
supported from the werklijst.

**Why this priority**: Werkverdeling is the primary action a
coordinator performs — the issue title names it explicitly.

**Independent Test**: Select multiple contactverzoeken in the
werklijst, assign them to a different afdeling/groep + medewerker,
and verify the assignment is updated.

**Acceptance Scenarios**:

1. **Given** a coördinator viewing the werklijst,
   **When** they select one or more contactverzoeken and choose
   "Toewijzen",
   **Then** an assignment dialog appears with a list of ALL
   afdelingen/groepen (regardless of coordinator role).
2. **Given** the assignment dialog with an afdeling/groep selected,
   **When** the coördinator optionally enters a medewerker
   e-mailadres,
   **Then** the email is validated client-side before submission.
3. **Given** a coördinator confirms the assignment,
   **When** the action completes,
   **Then** all selected contactverzoeken are reassigned to the
   chosen afdeling/groep (and medewerker, if selected).
4. **Given** a team-coördinator assigns a contactverzoek to an
   afdeling/groep outside their own scope,
   **When** the assignment succeeds,
   **Then** that contactverzoek disappears from the
   team-coördinator's werklijst (since it is no longer within
   their afdelingen/groepen).
5. **Given** a coördinator selects contactverzoeken for bulk
   assignment,
   **When** some assignments fail (e.g., a contactverzoek was
   already reassigned by another user),
   **Then** the system reports which succeeded and which failed,
   without rolling back the successful ones.

---

### User Story 5 — Toewijzing wijzigen vanuit detail (Priority: P3)

As a coördinator I want to change the toewijzing of a
contactverzoek from its detail page so that I can reassign while
reviewing the full context of the request.

Both coordinator roles can view and edit all contactverzoeken, the
same as all existing roles do currently. This includes the ability
to change the toewijzing from within the contactverzoek detail
view, consistent with how existing roles already operate.

**Why this priority**: This is an existing capability extended to
the new roles — lower priority because coordinators primarily
operate from the werklijst.

**Independent Test**: Open a contactverzoek detail page as a
coordinator and change its toewijzing; verify the change persists.

**Acceptance Scenarios**:

1. **Given** a coördinator viewing a contactverzoek detail page,
   **When** they choose to change the toewijzing,
   **Then** the same two-step assignment flow (afdeling/groep →
   medewerker) is available with all afdelingen/groepen listed.
2. **Given** a coördinator changes the toewijzing from the detail
   page,
   **When** they return to the werklijst,
   **Then** the werklijst reflects the updated assignment.

---

### Edge Cases

- What happens when a coördinator leaves the medewerker email
  field empty? The assignment proceeds to afdeling/groep only —
  medewerker is always optional.
- What happens when a contactverzoek is reassigned by another user
  while a coördinator has it selected for bulk assignment? The
  system MUST detect the conflict and report it without losing the
  rest of the bulk operation.
- What happens when a team-coördinator's own afdeling/groep
  membership changes? Their werklijst scope MUST update accordingly
  on the next page load.
- What if the afhandeltermijn configuration is not set? The system
  MUST display elapsed time without an overdue indicator, or use a
  sensible default.
- What happens when a user holds both organisatie-coördinator and
  team-coördinator roles? The broadest scope (organisatie) MUST
  take precedence.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support two new roles:
  organisatie-coördinator (organisation-wide scope) and
  team-coördinator (own afdelingen/groepen scope)
- **FR-002**: System MUST display a werklijst of openstaande
  contactverzoeken (interne taken) scoped to the coordinator's role
- **FR-003**: System MUST allow filtering the werklijst by
  afdeling/groep, where filter options are scoped to the
  coordinator's role (all for organisatie-coördinator, own for
  team-coördinator)
- **FR-004**: System MUST display elapsed time since the first
  klantcontact for each contactverzoek in the werklijst
- **FR-005**: System MUST compare elapsed time against a
  configurable maximum afhandeltermijn (in werkdagen) and visually
  indicate overdue items
- **FR-006**: System MUST allow coordinators to select one or more
  contactverzoeken from the werklijst and assign them to a
  different afdeling/groep and/or a medewerker within that
  afdeling/groep (werkverdeling)
- **FR-007**: The afdeling/groep picker in the assignment flow MUST
  show ALL afdelingen/groepen for both coordinator roles (not
  scoped to own)
- **FR-008**: The medewerker input in the assignment flow MUST be a
  freeform e-mailadres field with client-side email validation,
  consistent with the existing ContactverzoekDoorsturen pattern
- **FR-009**: When a team-coördinator assigns a contactverzoek
  outside their own afdelingen/groepen, the contactverzoek MUST
  disappear from their werklijst
- **FR-010**: Both coordinator roles MUST be able to view and edit
  all contactverzoeken (including changing toewijzing from the
  detail page), identical to existing roles
- **FR-011**: Bulk assignment MUST report partial success — if some
  items fail, the successful ones MUST persist and failures MUST be
  reported individually
- **FR-012**: The werklijst MUST be a dedicated top-level page at
  route `/werklijst`, accessible only to users with a coordinator
  role (organisatie-coördinator or team-coördinator)
- **FR-013**: The werklijst MUST be paginated, consistent with the
  existing Alle Contactverzoeken view

### Key Entities

- **Coordinator roles**: Two new authorization roles
  (organisatie-coördinator, team-coördinator) that determine
  werklijst scope and filter options but NOT assignment scope
- **Werklijst**: A filtered, role-scoped view of openstaande
  contactverzoeken displaying per row: onderwerp, assigned
  afdeling/groep, assigned medewerker, datum klantcontact, elapsed
  werkdagen, overdue indicator, and kanaal
- **Afhandeltermijn**: A configurable value (in werkdagen/business
  days) representing the maximum acceptable processing time for a
  contactverzoek
- **Werkverdeling (assignment)**: A two-step action — select
  afdeling/groep (from all), then optionally select medewerker
  (within selected afdeling/groep) — applicable to single or
  multiple contactverzoeken

### Assumptions

- The two new roles are additive — a user can hold a coordinator
  role alongside existing roles (medewerker, functioneel beheerder)
- "Own afdelingen/groepen" for a team-coördinator is derived from
  the user's existing afdeling/groep membership in the system
- The afhandeltermijn is a single organisation-wide setting (not
  per-afdeling or per-type); functional administrators configure it
- Werkdagen calculation excludes weekends; public holidays are not
  considered unless specified later
- The "vooralsnog" (for now) note in the issue — all roles
  including coordinators can view/edit all contactverzoeken — is
  accepted as current scope; future access restriction is out of
  scope for this feature

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Coordinators can identify all overdue
  contactverzoeken within their scope in under 30 seconds
- **SC-002**: Bulk assignment of 10 contactverzoeken completes in a
  single action (no need to assign one by one)
- **SC-003**: A team-coördinator can filter their werklijst to a
  specific afdeling/groep in 2 clicks or fewer
- **SC-004**: 100% of reassigned contactverzoeken correctly
  disappear from a team-coördinator's werklijst when assigned
  outside their scope
- **SC-005**: Overdue contactverzoeken are visually distinguishable
  from on-track items without requiring the user to read or
  calculate dates
