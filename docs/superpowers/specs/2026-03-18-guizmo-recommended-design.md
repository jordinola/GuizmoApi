# Design Spec: GET /api/guizmos/{userId}/recommended

**Date:** 2026-03-18
**Status:** Approved

---

## Overview

A new endpoint that returns a list of recommended Guizmos for a given user. The list of Guizmo IDs is determined by an external API (currently stubbed). The endpoint fetches the matching Guizmos from the database and returns full object details including category name.

---

## Endpoint

```
GET /api/guizmos/{userId:int:min(1)}/recommended?guizmoId={int?}
```

> Note: ASP.NET Core route constraint syntax requires the type constraint before the value constraint: `{userId:int:min(1)}`, not `{userId:min(1):int}`.

**Route parameter:**
- `userId` (int, required) — the user to fetch recommendations for; must be ≥ 1 (enforced by route constraint, returns `400 Bad Request` automatically)

**Query parameter:**
- `guizmoId` (int?, optional) — an optional hint passed to the external API

**Response:** `200 OK` with `IEnumerable<GuizmoDto>` (empty array `[]` if no recommendations are found)

---

## Data Flow

1. ASP.NET Core route constraint `{userId:int:min(1)}` rejects `userId ≤ 0` with `400 Bad Request`
2. `AddFluentValidationAutoValidation` validates `GuizmoRecommendedQuery` (bound via `[FromQuery]` on the action parameter)
3. Controller calls `IGuizmoService.GetRecommendedAsync(userId, guizmoId?, ct)`
4. Service calls `IExternalGuizmoApiClient.GetRecommendedIdsAsync(userId, guizmoId?, ct)` → returns `IEnumerable<int>`
5. Service fetches matching Guizmos from DB with `Category` included
6. IDs returned by the external API that do not exist in the DB are silently excluded
7. Returns mapped `IEnumerable<GuizmoDto>` (may be empty)

---

## New Files

| Layer | File | Purpose |
|---|---|---|
| Application | `DTOs/GuizmoRecommendedQuery.cs` | Plain record DTO with optional `GuizmoId`; `[FromQuery]` is applied at the action parameter level |
| Application | `Interfaces/IExternalGuizmoApiClient.cs` | Contract for external API client |
| Application | `Validators/GuizmoRecommendedQueryValidator.cs` | FluentValidation: `GuizmoId > 0` + exists in DB (async) |
| Infrastructure | `ExternalApi/StubExternalGuizmoApiClient.cs` | Stub: returns up to 3 randomly selected Guizmo IDs from DB |

## Modified Files

| Layer | File | Change |
|---|---|---|
| Application | `Interfaces/IGuizmoService.cs` | Add `GetRecommendedAsync(int userId, int? guizmoId, CancellationToken ct)` |
| Application | `Services/GuizmoService.cs` | Add `IExternalGuizmoApiClient` constructor parameter; implement `GetRecommendedAsync` |
| Infrastructure | `DependencyInjection.cs` | Add `services.AddTransient<IExternalGuizmoApiClient, StubExternalGuizmoApiClient>()` inside `AddInfrastructure` |
| Api | `Controllers/GuizmosController.cs` | Add `GET /{userId:int:min(1)}/recommended` action |
| Tests | `Application.Tests/Services/GuizmoServiceTests.cs` | Add tests for `GetRecommendedAsync` |
| Tests | `Application.Tests/Validators/GuizmoRecommendedQueryValidatorTests.cs` | New file: validator unit tests |
| Tests | `Api.Tests/Controllers/GuizmosControllerTests.cs` | Add tests for `GetRecommended` controller action |

---

## Validation

### Route Constraint
- `{userId:int:min(1)}` on the action route — ASP.NET Core rejects `userId ≤ 0` automatically with `400 Bad Request`

### `GuizmoRecommendedQuery` DTO
```csharp
// Application/DTOs/GuizmoRecommendedQuery.cs
public record GuizmoRecommendedQuery(int? GuizmoId);
```
No attribute on the class itself. The `[FromQuery]` attribute is placed on the action parameter:
```csharp
public async Task<ActionResult<IEnumerable<GuizmoDto>>> GetRecommended(
    [FromRoute] int userId,
    [FromQuery] GuizmoRecommendedQuery query,
    CancellationToken ct)
```

### `GuizmoRecommendedQueryValidator` (FluentValidation, auto-invoked)
- `GuizmoId`, when provided: must be `> 0`
- `GuizmoId`, when provided and `> 0`: must exist in `_context.Guizmos` (async `MustAsync` rule)
- Validator receives `AppDbContext` via constructor injection
- Registered as `Transient` (default via `AddValidatorsFromAssemblyContaining`) — safe because `AppDbContext` is scoped and validators are resolved per-request

### Error Handling
| Scenario | Response |
|---|---|
| `userId ≤ 0` | `400 Bad Request` (route constraint) |
| `guizmoId ≤ 0` | `400 Bad Request` (FluentValidation) |
| `guizmoId` not found in DB | `400 Bad Request` (FluentValidation async rule) |
| External API client throws | `500 Internal Server Error` (existing `ExceptionHandlingMiddleware`) |
| External API returns IDs not in DB | Silently excluded from results |
| No results | `200 OK` with empty array `[]` |

---

## External API Client

### Interface
```csharp
// Application/Interfaces/IExternalGuizmoApiClient.cs
namespace GuizmoApi.Application.Interfaces;

public interface IExternalGuizmoApiClient
{
    Task<IEnumerable<int>> GetRecommendedIdsAsync(int userId, int? guizmoId, CancellationToken ct = default);
}
```

### Stub Implementation
`StubExternalGuizmoApiClient` selects up to 3 Guizmo IDs at random from `_context.Guizmos`. If the database has fewer than 3 Guizmos, it returns however many exist (no error). When the real API is built, only a new implementation of `IExternalGuizmoApiClient` needs to be registered — no changes to `GuizmoService`.

---

## `GuizmoService` Changes

**Constructor:** Add `IExternalGuizmoApiClient` as a new constructor parameter alongside the existing `AppDbContext`:
```csharp
public GuizmoService(AppDbContext context, IExternalGuizmoApiClient externalClient)
```

**`GetRecommendedAsync` implementation:**
```
1. Call _externalClient.GetRecommendedIdsAsync(userId, guizmoId, ct) → IEnumerable<int> ids
2. Query DB: Guizmos WHERE Id IN (ids), Include(x => x.Category), AsNoTracking
3. Map to IEnumerable<GuizmoDto> via existing ToDto() and return
```

---

## Controller Action

```csharp
/// <summary>Returns recommended Guizmos for a user, based on an external API.</summary>
[HttpGet("{userId:int:min(1)}/recommended")]
[ProducesResponseType(typeof(IEnumerable<GuizmoDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<IEnumerable<GuizmoDto>>> GetRecommended(
    [FromRoute] int userId,
    [FromQuery] GuizmoRecommendedQuery query,
    CancellationToken ct)
{
    var result = await _service.GetRecommendedAsync(userId, query.GuizmoId, ct);
    return Ok(result);
}
```

---

## DI Registration

**`Infrastructure/DependencyInjection.cs`** — extend `AddInfrastructure`:
```csharp
services.AddTransient<IExternalGuizmoApiClient, StubExternalGuizmoApiClient>();
```

**Validator** — auto-registered via existing `AddValidatorsFromAssemblyContaining<CreateGuizmoRequestValidator>()` in `Application/DependencyInjection.cs`. No additional registration needed.

---

## Testing

### `GuizmoApi.Application.Tests`

**`GuizmoServiceTests` additions** (in-memory EF + Moq on `IExternalGuizmoApiClient`):
- Returns full `GuizmoDto` list (with category name) for IDs returned by client
- Excludes IDs not found in DB (returns only matching Guizmos)
- Returns empty list when client returns no IDs

**`GuizmoRecommendedQueryValidatorTests` (new file)** (in-memory EF for async DB check):
- Valid when `GuizmoId` is null
- Valid when `GuizmoId > 0` and exists in DB
- Invalid when `GuizmoId ≤ 0`
- Invalid when `GuizmoId > 0` but does not exist in DB

### `GuizmoApi.Api.Tests`

**`GuizmosControllerTests` additions** (Moq on `IGuizmoService`):
- `GetRecommended` returns `200 OK` with list
- `GetRecommended` passes correct `userId` and `guizmoId` values to service
