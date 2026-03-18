# Design Spec: GET /api/guizmos/{userId}/recommended

**Date:** 2026-03-18
**Status:** Approved

---

## Overview

A new endpoint that returns a list of recommended Guizmos for a given user. The list of Guizmo IDs is determined by an external API (currently stubbed). The endpoint fetches the matching Guizmos from the database and returns full object details including category name.

---

## Endpoint

```
GET /api/guizmos/{userId:min(1):int}/recommended?guizmoId={int?}
```

**Route parameter:**
- `userId` (int, required) — the user to fetch recommendations for; must be ≥ 1 (enforced by route constraint)

**Query parameter:**
- `guizmoId` (int, optional) — an optional hint passed to the external API

**Response:** `200 OK` with `IEnumerable<GuizmoDto>`

---

## Data Flow

1. ASP.NET Core route constraint rejects `userId ≤ 0` with `400 Bad Request`
2. `AddFluentValidationAutoValidation` validates `GuizmoRecommendedQuery` (the `[FromQuery]` DTO)
3. Controller calls `IGuizmoService.GetRecommendedAsync(userId, guizmoId?, ct)`
4. Service calls `IExternalGuizmoApiClient.GetRecommendedIdsAsync(userId, guizmoId?, ct)` → returns `IEnumerable<int>`
5. Service fetches matching Guizmos from DB with `Category` included
6. IDs returned by the external API that do not exist in the DB are silently excluded
7. Returns mapped `IEnumerable<GuizmoDto>`

---

## New Files

| Layer | File | Purpose |
|---|---|---|
| Application | `DTOs/GuizmoRecommendedQuery.cs` | `[FromQuery]` DTO with optional `GuizmoId` |
| Application | `Interfaces/IExternalGuizmoApiClient.cs` | Contract for external API client |
| Application | `Validators/GuizmoRecommendedQueryValidator.cs` | FluentValidation: `GuizmoId > 0` + exists in DB |
| Infrastructure | `ExternalApi/StubExternalGuizmoApiClient.cs` | Stub returning 3 random GuizmoIds from DB |

## Modified Files

| Layer | File | Change |
|---|---|---|
| Application | `Interfaces/IGuizmoService.cs` | Add `GetRecommendedAsync` |
| Application | `Services/GuizmoService.cs` | Implement `GetRecommendedAsync` with `IExternalGuizmoApiClient` |
| Infrastructure | `DependencyInjection.cs` | Register `StubExternalGuizmoApiClient` as `IExternalGuizmoApiClient` |
| Api | `Controllers/GuizmosController.cs` | Add `GET /{userId}/recommended` action |
| Tests | `Application.Tests/Services/GuizmoServiceTests.cs` | Tests for `GetRecommendedAsync` |
| Tests | `Application.Tests/Validators/GuizmoRecommendedQueryValidatorTests.cs` | Validator unit tests |
| Tests | `Api.Tests/Controllers/GuizmosControllerTests.cs` | Controller unit tests |

---

## Validation

### Route Constraint
- `{userId:min(1):int}` — ASP.NET Core rejects `userId ≤ 0` automatically with `400 Bad Request`

### `GuizmoRecommendedQueryValidator` (FluentValidation, auto-invoked)
- `GuizmoId`, when provided: must be `> 0`
- `GuizmoId`, when provided and `> 0`: must exist in `_context.Guizmos` (async `MustAsync` rule)
- Validator receives `AppDbContext` via DI

### Error Handling
| Scenario | Response |
|---|---|
| `userId ≤ 0` | `400 Bad Request` (route constraint) |
| `guizmoId ≤ 0` | `400 Bad Request` (FluentValidation) |
| `guizmoId` not found in DB | `400 Bad Request` (FluentValidation async rule) |
| External API client throws | `500 Internal Server Error` (existing `ExceptionHandlingMiddleware`) |
| External API returns IDs not in DB | Silently excluded from results |

---

## External API Client

### Interface
```csharp
// Application/Interfaces/IExternalGuizmoApiClient.cs
public interface IExternalGuizmoApiClient
{
    Task<IEnumerable<int>> GetRecommendedIdsAsync(int userId, int? guizmoId, CancellationToken ct = default);
}
```

### Stub Implementation
Since the external API does not exist yet, `StubExternalGuizmoApiClient` returns 3 randomly selected Guizmo IDs from the database. When the real API is built, only a new implementation of `IExternalGuizmoApiClient` needs to be registered — no changes to `GuizmoService`.

---

## `GuizmoService.GetRecommendedAsync`

```
1. Call IExternalGuizmoApiClient.GetRecommendedIdsAsync(userId, guizmoId?, ct)
2. Query DB: Guizmos WHERE Id IN (returned IDs), Include Category, AsNoTracking
3. Map to IEnumerable<GuizmoDto> and return
```

---

## Testing

### `GuizmoApi.Application.Tests` (in-memory EF + Moq on `IExternalGuizmoApiClient`)

**`GuizmoServiceTests` additions:**
- Returns full `GuizmoDto` list (with category) for IDs returned by client
- Excludes IDs not found in DB (returns only matching Guizmos)
- Returns empty list when client returns no IDs

**`GuizmoRecommendedQueryValidatorTests` (new file):**
- Valid when `GuizmoId` is null
- Valid when `GuizmoId > 0` and exists in DB
- Invalid when `GuizmoId ≤ 0`
- Invalid when `GuizmoId > 0` but does not exist in DB

### `GuizmoApi.Api.Tests` (Moq on `IGuizmoService`)

**`GuizmosControllerTests` additions:**
- `GetRecommended` returns `200 OK` with list
- `GetRecommended` passes `userId` and `guizmoId` to service correctly

---

## DI Registration

- `StubExternalGuizmoApiClient` registered as `IExternalGuizmoApiClient` in `Infrastructure/DependencyInjection.cs`
- `GuizmoRecommendedQueryValidator` auto-registered via existing `AddValidatorsFromAssemblyContaining<CreateGuizmoRequestValidator>()`
