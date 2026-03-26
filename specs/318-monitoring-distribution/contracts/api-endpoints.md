# API Contracts: Monitoring en Werkverdeling

**Phase 1 output** | **Feature**: 318-monitoring-distribution

## New Endpoints

### GET /api/werklijst

Paginated, filtered overview of openstaande contactverzoeken for coordinators.

**Authorization**: Requires `CoordinatorPolicy` (either organisatie- or team-coördinator)

**Query Parameters**:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| page | int | No | 1 | Page number |
| pageSize | int | No | 10 | Items per page |
| afdelingUuid | guid | No | — | Filter by afdeling |
| groepUuid | guid | No | — | Filter by groep |
| alleenOverschreden | bool | No | false | Show only overdue items |

**Scoping** (implicit, not query params):
- Organisatie-coördinator: Sees all afdelingen/groepen
- Team-coördinator: Scoped to own afdelingen/groepen

**Response** `200 OK`:

```json
{
  "count": 42,
  "next": "/api/werklijst?page=2&pageSize=10",
  "previous": null,
  "results": [
    {
      "url": "https://openkl.../internetaken/abc-123",
      "uuid": "abc-123",
      "nummer": "INT-2025-001",
      "status": "te_verwerken",
      "onderwerp": "Vraag over vergunning",
      "kanaal": "Telefoon",
      "plaatsgevondenOp": "2025-03-18T10:30:00Z",
      "afdeling": "Burgerzaken",
      "groep": "Vergunningen",
      "medewerker": null,
      "afhandeltermijnDatum": "2025-03-25",
      "verstrekenWerkdagen": 5,
      "isOverschreden": false
    }
  ]
}
```

**Error Responses**:
- `401 Unauthorized` — Not authenticated
- `403 Forbidden` — Not a coordinator
- `400 Bad Request` — Invalid query parameters (ProblemDetails)

---

### POST /api/werkverdeling

Bulk-assign internetaken to a target afdeling/groep and optionally a medewerker.

**Authorization**: Requires `CoordinatorPolicy`

**Request Body**:

```json
{
  "internetaakUuids": ["abc-123", "def-456", "ghi-789"],
  "afdelingUuid": "afd-001",
  "groepUuid": "grp-001",
  "medewerkerEmail": "j.jansen@gemeente.nl"
}
```

**Validation**:
- `internetaakUuids` — required, non-empty, max 20 items
- At least one of `afdelingUuid`, `groepUuid`, `medewerkerEmail` must be provided
- `medewerkerEmail` is optional (freeform email; follows existing ContactverzoekDoorsturen pattern)

**Response** `200 OK`:

```json
{
  "succeeded": 2,
  "failed": 1,
  "results": [
    { "internetaakUuid": "abc-123", "success": true, "errorMessage": null },
    { "internetaakUuid": "def-456", "success": true, "errorMessage": null },
    { "internetaakUuid": "ghi-789", "success": false, "errorMessage": "Internetaak niet gevonden" }
  ]
}
```

**Error Responses**:
- `401 Unauthorized` — Not authenticated
- `403 Forbidden` — Not a coordinator
- `400 Bad Request` — Validation errors (ProblemDetails)

---

## Modified Endpoints

### GET /api/me

**Changes**: Response includes two new boolean fields.

**Added fields**:

```json
{
  "...existing fields...",
  "hasOrganisatieCoordinatorAccess": true,
  "hasTeamCoordinatorAccess": false,
  "hasCoordinatorAccess": true
}
```

---

## Pagination Contract

Follows existing convention matching Django REST Framework / Open Klant format:

```typescript
interface PaginationResponse<T> {
  count: number;
  next: string | null;
  previous: string | null;
  results: T[];
}
```

The werklijst backend must return responses in this format to be compatible
with the existing `usePagination` composable on the frontend.
