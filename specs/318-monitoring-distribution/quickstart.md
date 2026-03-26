# Quickstart: Monitoring en Werkverdeling

**Phase 1 output** | **Feature**: 318-monitoring-distribution

## Prerequisites

- .NET 8.0 SDK
- Node.js 20+ with npm
- Docker (for local API dependencies)
- An OIDC provider with coordinator claims configured

## OIDC Claims Setup

The feature requires two new OIDC role claims to be provisioned:

| Claim / Role | Description |
|--------------|-------------|
| `organisatie-coordinator` | Full organization-wide access to werklijst and werkverdeling |
| `team-coordinator` | Scoped access to own afdelingen/groepen |

These must be configured in the identity provider and mapped in
`AuthenticationExtensions.cs`. Without these claims, the werklijst
route will return 403.

## Configuration

Add to `appsettings.json` (or environment-specific overrides):

```json
{
  "Afhandeltermijn": {
    "WerkdagenTermijn": 5
  }
}
```

## Running Locally

```bash
# Backend
cd InterneTaakAfhandeling.Web.Server
dotnet run

# Frontend (separate terminal)
cd InterneTaakAfhandeling.Web.Client
npm run dev
```

Navigate to `/werklijst` — requires a logged-in user with one of the
coordinator roles.

## Testing

```bash
# E2E tests
cd InterneTaakAfhandeling.EndToEndTest
dotnet test --filter "Category=Werklijst"
```

## Key Files

| File | Purpose |
|------|---------|
| `Features/Werklijst/WerklijstController.cs` | Werklijst API endpoint |
| `Features/Werkverdeling/WerkverdelingController.cs` | Bulk assignment endpoint |
| `Authentication/ITAUser.cs` | Coordinator role flags |
| `src/views/WerklijstView.vue` | Main werklijst page |
| `src/components/werklijst/` | Werklijst UI components |
| `src/services/werklijstService.ts` | Frontend API service |
