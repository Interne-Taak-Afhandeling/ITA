# Data Model: Monitoring en Werkverdeling

**Phase 1 output** | **Feature**: 318-monitoring-distribution

## New Types / DTOs

### Backend (C#)

#### WerklijstOverzichtItem (Response DTO)

Extends the pattern of `InterneTaakOverzichtItem` with afhandeltermijn fields.
Flat DTO — no nested API objects.

```csharp
public record WerklijstOverzichtItem(
    string Url,
    string Uuid,
    string Nummer,
    string Status,
    // Klantcontact fields (resolved)
    string? Onderwerp,
    string? Kanaal,
    DateTime? PlaatsgevondenOp,
    // Actor fields (resolved)
    string? Afdeling,
    string? Groep,
    string? Medewerker,
    // Afhandeltermijn fields (calculated)
    DateOnly? AfhandeltermijnDatum,
    int VerstrekenWerkdagen,
    bool IsOverschreden
);
```

#### WerklijstQuery (Request params)

```csharp
public record WerklijstQuery(
    int? Page,
    int? PageSize,
    Guid? AfdelingUuid,
    Guid? GroepUuid,
    bool? AlleenOverschreden  // Filter only overdue items
);
```

#### WerklijstResponse (Paginated response)

```csharp
public record WerklijstResponse(
    int Count,
    string? Next,
    string? Previous,
    IReadOnlyList<WerklijstOverzichtItem> Results
);
```

#### WerkverdelingRequest (Bulk assignment input)

```csharp
public record WerkverdelingRequest(
    IReadOnlyList<string> InternetaakUuids,
    string? AfdelingUuid,
    string? GroepUuid,
    string? MedewerkerEmail
);
```

#### WerkverdelingResponse (Bulk assignment result)

```csharp
public record WerkverdelingResponse(
    int Succeeded,
    int Failed,
    IReadOnlyList<WerkverdelingItemResult> Results
);

public record WerkverdelingItemResult(
    string InternetaakUuid,
    bool Success,
    string? ErrorMessage
);
```

#### AfhandeltermijnOptions (Configuration)

```csharp
public class AfhandeltermijnOptions
{
    public const string SectionName = "Afhandeltermijn";
    public int WerkdagenTermijn { get; set; } = 5;
}
```

### Frontend (TypeScript)

#### werklijst.ts

```typescript
export interface WerklijstOverzichtItem {
  url: string;
  uuid: string;
  nummer: string;
  status: string;
  onderwerp?: string;
  kanaal?: string;
  plaatsgevondenOp?: string;
  afdeling?: string;
  groep?: string;
  medewerker?: string;
  afhandeltermijnDatum?: string;
  verstrekenWerkdagen: number;
  isOverschreden: boolean;
}

export interface WerkverdelingRequest {
  internetaakUuids: string[];
  afdelingUuid?: string;
  groepUuid?: string;
  medewerkerEmail?: string;
}

export interface WerkverdelingResponse {
  succeeded: number;
  failed: number;
  results: WerkverdelingItemResult[];
}

export interface WerkverdelingItemResult {
  internetaakUuid: string;
  success: boolean;
  errorMessage?: string;
}
```

#### user.ts (Modifications)

Add to existing `User` interface:

```typescript
// Add to User interface
hasOrganisatieCoordinatorAccess: boolean;
hasTeamCoordinatorAccess: boolean;
hasCoordinatorAccess: boolean;
```

## Modified Types

### ITAUser.cs

Add two new properties:

```csharp
public bool HasOrganisatieCoordinatorAccess { get; init; }
public bool HasTeamCoordinatorAccess { get; init; }

// Convenience property
public bool HasCoordinatorAccess =>
    HasOrganisatieCoordinatorAccess || HasTeamCoordinatorAccess;
```

## State Transitions

### Werkverdeling Assignment Flow

```
┌──────────────┐    User selects    ┌──────────────────┐
│  Werklijst   │ ──────────────────>│  Items Selected  │
│  (browsing)  │                    │  (checkbox state) │
└──────────────┘                    └────────┬─────────┘
                                             │
                                    User clicks "Toewijzen"
                                             │
                                             v
                                   ┌─────────────────────┐
                                   │ WerkverdelingDialog  │
                                   │ (select target)      │
                                   └────────┬────────────┘
                                             │
                                    User selects afdeling/
                                    groep + medewerker
                                    and confirms
                                             │
                                             v
                                   ┌─────────────────────┐
                                   │ POST /werkverdeling  │
                                   │ (bulk assignment)    │
                                   └────────┬────────────┘
                                             │
                                    ┌────────┴────────┐
                                    │                 │
                                    v                 v
                           ┌──────────────┐  ┌──────────────┐
                           │   Success    │  │ Partial Fail │
                           │   Toast ✓    │  │   Toast ⚠    │
                           └──────────────┘  └──────────────┘
                                    │                 │
                                    └────────┬────────┘
                                             │
                                    Refresh werklijst
                                             │
                                             v
                                   ┌──────────────────┐
                                   │  Werklijst       │
                                   │  (refreshed)     │
                                   └──────────────────┘
```

## External API Mapping

### Open Klant API → WerklijstOverzichtItem

| WerklijstOverzichtItem field | Source API | Source field |
|------------------------------|-----------|-------------|
| url, uuid, nummer, status | Open Klant | internetaak.url/uuid/nummer/status |
| onderwerp | Open Klant | klantcontact.onderwerp |
| kanaal | Open Klant | klantcontact.kanaal |
| plaatsgevondenOp | Open Klant | klantcontact.plaatsgevondenOp |
| afdeling | Objecten | actor → afdeling.naam |
| groep | Objecten | actor → groep.naam |
| medewerker | Objecten | actor → medewerker.naam |
| afhandeltermijnDatum | Calculated | plaatsgevondenOp + WerkdagenTermijn |
| verstrekenWerkdagen | Calculated | business days since plaatsgevondenOp |
| isOverschreden | Calculated | verstrekenWerkdagen > WerkdagenTermijn |
