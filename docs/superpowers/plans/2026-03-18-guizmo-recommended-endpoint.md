# Guizmo Recommended Endpoint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `GET /api/guizmos/{userId}/recommended?guizmoId={int?}` that calls a stubbed external API and returns up to 3 matching Guizmos with full category info.

**Architecture:** New `IExternalGuizmoApiClient` interface in Application layer with a stub implementation in Infrastructure that returns up to 3 random Guizmo IDs from the DB. `GuizmoService` depends on the interface, fetches matching entities, and maps them to DTOs. FluentValidation auto-validates the optional `guizmoId` query param including an async DB existence check.

**Tech Stack:** .NET 8, ASP.NET Core, EF Core (in-memory for tests), FluentValidation 11, Moq 4, FluentAssertions 6, xunit

---

## File Map

| Action | File | Purpose |
|---|---|---|
| Create | `src/GuizmoApi.Application/DTOs/GuizmoRecommendedQuery.cs` | `[FromQuery]` DTO holding optional `GuizmoId` |
| Create | `src/GuizmoApi.Application/Interfaces/IExternalGuizmoApiClient.cs` | Contract for external recommendations API |
| Modify | `src/GuizmoApi.Application/Interfaces/IGuizmoService.cs` | Add `GetRecommendedAsync` |
| Create | `src/GuizmoApi.Infrastructure/ExternalApi/StubExternalGuizmoApiClient.cs` | Stub returning up to 3 random IDs from DB |
| Modify | `src/GuizmoApi.Infrastructure/DependencyInjection.cs` | Register stub as `IExternalGuizmoApiClient` |
| Modify | `src/GuizmoApi.Application/Services/GuizmoService.cs` | Add constructor param + implement `GetRecommendedAsync` |
| Create | `src/GuizmoApi.Application/Validators/GuizmoRecommendedQueryValidator.cs` | Validate `GuizmoId > 0` and exists in DB |
| Modify | `src/GuizmoApi.Api/Controllers/GuizmosController.cs` | Add `GetRecommended` action |
| Modify | `tests/GuizmoApi.Application.Tests/GuizmoApi.Application.Tests.csproj` | Add Moq package reference |
| Modify | `tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs` | Add 3 tests for `GetRecommendedAsync` |
| Create | `tests/GuizmoApi.Application.Tests/Validators/GuizmoRecommendedQueryValidatorTests.cs` | 4 validator tests |
| Modify | `tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs` | Add 2 controller tests |

---

## Task 1: Create DTO, interface, and extend IGuizmoService

No tests for this task — these are pure contracts with no logic.

**Files:**
- Create: `src/GuizmoApi.Application/DTOs/GuizmoRecommendedQuery.cs`
- Create: `src/GuizmoApi.Application/Interfaces/IExternalGuizmoApiClient.cs`
- Modify: `src/GuizmoApi.Application/Interfaces/IGuizmoService.cs`

- [ ] **Step 1: Create `GuizmoRecommendedQuery` DTO**

```csharp
// src/GuizmoApi.Application/DTOs/GuizmoRecommendedQuery.cs
namespace GuizmoApi.Application.DTOs;

public record GuizmoRecommendedQuery(int? GuizmoId);
```

- [ ] **Step 2: Create `IExternalGuizmoApiClient` interface**

```csharp
// src/GuizmoApi.Application/Interfaces/IExternalGuizmoApiClient.cs
namespace GuizmoApi.Application.Interfaces;

public interface IExternalGuizmoApiClient
{
    Task<IEnumerable<int>> GetRecommendedIdsAsync(int userId, int? guizmoId, CancellationToken ct = default);
}
```

- [ ] **Step 3: Add `GetRecommendedAsync` to `IGuizmoService`**

Open `src/GuizmoApi.Application/Interfaces/IGuizmoService.cs` and add one line at the end of the interface body:

```csharp
Task<IEnumerable<GuizmoDto>> GetRecommendedAsync(int userId, int? guizmoId, CancellationToken ct = default);
```

Full file after change:
```csharp
using GuizmoApi.Application.DTOs;

namespace GuizmoApi.Application.Interfaces;

public interface IGuizmoService
{
    Task<IEnumerable<GuizmoDto>> GetAllAsync(CancellationToken ct = default);
    Task<GuizmoDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<GuizmoDto> CreateAsync(CreateGuizmoRequest request, CancellationToken ct = default);
    Task<GuizmoDto?> UpdateAsync(int id, UpdateGuizmoRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<PagedResult<GuizmoDto>> GetPagedAsync(GuizmoPagedQuery query, CancellationToken ct = default);
    Task<IEnumerable<GuizmoDto>> GetRecommendedAsync(int userId, int? guizmoId, CancellationToken ct = default);
}
```

- [ ] **Step 4: Verify it builds (no tests yet, just check compilation)**

```bash
dotnet build /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: build errors on `GuizmoService` (it no longer satisfies `IGuizmoService`) — that's fine, we fix it in Task 3.

> If the build error is only about `GuizmoService` not implementing `GetRecommendedAsync`, proceed. Any other error means a typo — fix it before continuing.

- [ ] **Step 5: Commit**

```bash
cd /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi
git add src/GuizmoApi.Application/DTOs/GuizmoRecommendedQuery.cs \
        src/GuizmoApi.Application/Interfaces/IExternalGuizmoApiClient.cs \
        src/GuizmoApi.Application/Interfaces/IGuizmoService.cs
git commit -m "feat: add GuizmoRecommendedQuery DTO, IExternalGuizmoApiClient, and extend IGuizmoService"
```

---

## Task 2: Implement stub and register in DI

**Files:**
- Create: `src/GuizmoApi.Infrastructure/ExternalApi/StubExternalGuizmoApiClient.cs`
- Modify: `src/GuizmoApi.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Create `StubExternalGuizmoApiClient`**

```csharp
// src/GuizmoApi.Infrastructure/ExternalApi/StubExternalGuizmoApiClient.cs
using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.Interfaces;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Infrastructure.ExternalApi;

public class StubExternalGuizmoApiClient : IExternalGuizmoApiClient
{
    private readonly AppDbContext _context;

    public StubExternalGuizmoApiClient(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<int>> GetRecommendedIdsAsync(int userId, int? guizmoId, CancellationToken ct = default)
    {
        var ids = await _context.Guizmos.Select(g => g.Id).ToListAsync(ct);
        return ids.OrderBy(_ => Random.Shared.Next()).Take(3);
    }
}
```

> `Random.Shared` is the thread-safe, zero-allocation singleton introduced in .NET 6. No `new Random()` needed.

- [ ] **Step 2: Register stub in `Infrastructure/DependencyInjection.cs`**

Open `src/GuizmoApi.Infrastructure/DependencyInjection.cs`. Add the `using` for the new namespace and register the stub inside `AddInfrastructure`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GuizmoApi.Infrastructure.Data;
using GuizmoApi.Infrastructure.ExternalApi;
using GuizmoApi.Application.Interfaces;

namespace GuizmoApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddTransient<IExternalGuizmoApiClient, StubExternalGuizmoApiClient>();

        return services;
    }
}
```

- [ ] **Step 3: Build to verify no compilation errors in Infrastructure**

```bash
dotnet build /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: still only the `GuizmoService` error about missing `GetRecommendedAsync`. Nothing new.

- [ ] **Step 4: Commit**

```bash
git add src/GuizmoApi.Infrastructure/ExternalApi/StubExternalGuizmoApiClient.cs \
        src/GuizmoApi.Infrastructure/DependencyInjection.cs
git commit -m "feat: add StubExternalGuizmoApiClient and register in DI"
```

---

## Task 3: Implement `GuizmoService.GetRecommendedAsync` (TDD)

The `Application.Tests` project does not currently have Moq — we add it first, then write failing tests, then implement.

**Files:**
- Modify: `tests/GuizmoApi.Application.Tests/GuizmoApi.Application.Tests.csproj`
- Modify: `tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs`
- Modify: `src/GuizmoApi.Application/Services/GuizmoService.cs`

- [ ] **Step 1: Add Moq to `GuizmoApi.Application.Tests`**

Open `tests/GuizmoApi.Application.Tests/GuizmoApi.Application.Tests.csproj` and add inside the existing `<ItemGroup>` with other `PackageReference` entries:

```xml
<PackageReference Include="Moq" Version="4.*" />
```

- [ ] **Step 2: Add the failing tests to `GuizmoServiceTests.cs`**

The file already has a `CreateContext` helper and an `AddCategoryAsync` helper — reuse them. Add these three test methods at the end of the class, before the closing `}`:

```csharp
[Fact]
public async Task GetRecommendedAsync_returns_full_dtos_for_ids_returned_by_client()
{
    await using var context = CreateContext(nameof(GetRecommendedAsync_returns_full_dtos_for_ids_returned_by_client));
    var category = await AddCategoryAsync(context);
    context.Guizmos.Add(new Guizmo { Name = "Widget1", Manufacturer = "Acme", Msrp = 1m, CategoryId = category.Id });
    context.Guizmos.Add(new Guizmo { Name = "Widget2", Manufacturer = "Acme", Msrp = 2m, CategoryId = category.Id });
    await context.SaveChangesAsync();
    var ids = context.Guizmos.Select(g => g.Id).ToList();

    var clientMock = new Mock<IExternalGuizmoApiClient>();
    clientMock.Setup(c => c.GetRecommendedIdsAsync(42, null, default)).ReturnsAsync(ids);

    var service = new GuizmoService(context, clientMock.Object);
    var result = (await service.GetRecommendedAsync(42, null)).ToList();

    result.Should().HaveCount(2);
    result.Should().Contain(g => g.Name == "Widget1");
    result.Should().Contain(g => g.Name == "Widget2");
    result.All(g => g.CategoryName == "Electronics").Should().BeTrue();
}

[Fact]
public async Task GetRecommendedAsync_excludes_ids_not_in_db()
{
    await using var context = CreateContext(nameof(GetRecommendedAsync_excludes_ids_not_in_db));
    var category = await AddCategoryAsync(context);
    context.Guizmos.Add(new Guizmo { Name = "Widget1", Manufacturer = "Acme", Msrp = 1m, CategoryId = category.Id });
    await context.SaveChangesAsync();
    var existingId = context.Guizmos.First().Id;

    var clientMock = new Mock<IExternalGuizmoApiClient>();
    clientMock.Setup(c => c.GetRecommendedIdsAsync(1, null, default))
        .ReturnsAsync(new[] { existingId, 99999 }); // 99999 does not exist

    var service = new GuizmoService(context, clientMock.Object);
    var result = (await service.GetRecommendedAsync(1, null)).ToList();

    result.Should().HaveCount(1);
    result[0].Id.Should().Be(existingId);
}

[Fact]
public async Task GetRecommendedAsync_returns_empty_list_when_client_returns_no_ids()
{
    await using var context = CreateContext(nameof(GetRecommendedAsync_returns_empty_list_when_client_returns_no_ids));
    var category = await AddCategoryAsync(context);

    var clientMock = new Mock<IExternalGuizmoApiClient>();
    clientMock.Setup(c => c.GetRecommendedIdsAsync(1, null, default))
        .ReturnsAsync(Enumerable.Empty<int>());

    var service = new GuizmoService(context, clientMock.Object);
    var result = await service.GetRecommendedAsync(1, null);

    result.Should().BeEmpty();
}
```

Also add `using Moq;` and `using GuizmoApi.Application.Interfaces;` to the top of the file alongside the existing usings.

- [ ] **Step 3: Attempt to build to confirm compilation fails (tests cannot run yet)**

```bash
dotnet build /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: compilation failure — `GuizmoService` constructor does not yet accept `IExternalGuizmoApiClient`. This is the expected "red" state before implementation.

- [ ] **Step 4: Update existing `GuizmoService` instantiations in `GuizmoServiceTests.cs`**

Adding `IExternalGuizmoApiClient` as a required constructor parameter will break every existing test that calls `new GuizmoService(context)`. Before touching `GuizmoService.cs`, update **all** occurrences in `tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs` from:

```csharp
var service = new GuizmoService(context);
```

to:

```csharp
var service = new GuizmoService(context, Mock.Of<IExternalGuizmoApiClient>());
```

There are **16 occurrences**. Use find-and-replace across the entire file — do not stop early. `Mock.Of<T>()` creates a no-behaviour mock in one expression — it is already available after adding `using Moq;` in Step 2.

> After this step the build will still fail because the two-arg constructor does not exist yet. That is expected — Step 5 adds it.

- [ ] **Step 5: Update `GuizmoService` — add constructor param and implement method**

Open `src/GuizmoApi.Application/Services/GuizmoService.cs`.

1. Add `_externalClient` field and update the constructor:

```csharp
private readonly AppDbContext _context;
private readonly IExternalGuizmoApiClient _externalClient;

public GuizmoService(AppDbContext context, IExternalGuizmoApiClient externalClient)
{
    _context = context;
    _externalClient = externalClient;
}
```

2. Add `GetRecommendedAsync` method before the private `ToDto` helper at the bottom of the class:

```csharp
public async Task<IEnumerable<GuizmoDto>> GetRecommendedAsync(int userId, int? guizmoId, CancellationToken ct = default)
{
    var ids = (await _externalClient.GetRecommendedIdsAsync(userId, guizmoId, ct)).ToList();

    var guizmos = await _context.Guizmos
        .Include(x => x.Category)
        .AsNoTracking()
        .Where(g => ids.Contains(g.Id))
        .ToListAsync(ct);

    return guizmos.Select(ToDto);
}
```

- [ ] **Step 6: Run the new tests to confirm they pass**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx \
  --filter "FullyQualifiedName~GetRecommendedAsync"
```

Expected: 3 tests PASS. If any fail, check that `ids.Contains(g.Id)` compiles (requires `ids` to be a `List<int>`, which the `.ToList()` call ensures).

- [ ] **Step 7: Run full test suite to confirm no regressions**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: all previously passing tests still pass.

- [ ] **Step 8: Commit**

```bash
git add tests/GuizmoApi.Application.Tests/GuizmoApi.Application.Tests.csproj \
        tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs \
        src/GuizmoApi.Application/Services/GuizmoService.cs
git commit -m "feat: implement GuizmoService.GetRecommendedAsync with tests"
```

---

## Task 4: Implement `GuizmoRecommendedQueryValidator` (TDD)

**Files:**
- Create: `tests/GuizmoApi.Application.Tests/Validators/GuizmoRecommendedQueryValidatorTests.cs`
- Create: `src/GuizmoApi.Application/Validators/GuizmoRecommendedQueryValidator.cs`

- [ ] **Step 1: Write the failing validator tests**

Create `tests/GuizmoApi.Application.Tests/Validators/GuizmoRecommendedQueryValidatorTests.cs`:

```csharp
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Validators;
using GuizmoApi.Domain.Entities;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Tests.Validators;

public class GuizmoRecommendedQueryValidatorTests
{
    private static AppDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    [Fact]
    public async Task Should_pass_when_guizmoId_is_null()
    {
        await using var context = CreateContext(nameof(Should_pass_when_guizmoId_is_null));
        var validator = new GuizmoRecommendedQueryValidator(context);

        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Should_pass_when_guizmoId_is_positive_and_exists_in_db()
    {
        await using var context = CreateContext(nameof(Should_pass_when_guizmoId_is_positive_and_exists_in_db));
        var category = new Category { Name = "Electronics" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        context.Guizmos.Add(new Guizmo { Name = "Widget", Manufacturer = "Acme", Msrp = 1m, CategoryId = category.Id });
        await context.SaveChangesAsync();
        var id = context.Guizmos.First().Id;

        var validator = new GuizmoRecommendedQueryValidator(context);
        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(id));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_fail_when_guizmoId_is_not_positive(int guizmoId)
    {
        await using var context = CreateContext($"{nameof(Should_fail_when_guizmoId_is_not_positive)}_{guizmoId}");
        var validator = new GuizmoRecommendedQueryValidator(context);

        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(guizmoId));

        result.ShouldHaveValidationErrorFor(x => x.GuizmoId);
    }

    [Fact]
    public async Task Should_fail_when_guizmoId_does_not_exist_in_db()
    {
        await using var context = CreateContext(nameof(Should_fail_when_guizmoId_does_not_exist_in_db));
        var validator = new GuizmoRecommendedQueryValidator(context);

        var result = await validator.TestValidateAsync(new GuizmoRecommendedQuery(99999));

        result.ShouldHaveValidationErrorFor(x => x.GuizmoId);
    }
}
```

- [ ] **Step 2: Attempt to build to confirm compilation fails**

```bash
dotnet build /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: compilation failure — `GuizmoRecommendedQueryValidator` does not exist yet. This is the expected "red" state.

- [ ] **Step 3: Create the validator**

```csharp
// src/GuizmoApi.Application/Validators/GuizmoRecommendedQueryValidator.cs
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Application.Validators;

public class GuizmoRecommendedQueryValidator : AbstractValidator<GuizmoRecommendedQuery>
{
    public GuizmoRecommendedQueryValidator(AppDbContext context)
    {
        When(x => x.GuizmoId.HasValue, () =>
        {
            RuleFor(x => x.GuizmoId!.Value)
                .GreaterThan(0)
                .WithMessage("GuizmoId must be greater than zero.");

            RuleFor(x => x.GuizmoId!.Value)
                .MustAsync(async (id, ct) => await context.Guizmos.AnyAsync(g => g.Id == id, ct))
                .WithMessage("GuizmoId does not exist.")
                .When(x => x.GuizmoId!.Value > 0);
        });
    }
}
```

> **`using Microsoft.EntityFrameworkCore;` is required** — `AnyAsync` is an EF Core extension method and will not resolve without it.

> **Why two separate `RuleFor` on the same property:** FluentValidation stops running rules on a property after the first failure by default. The `.When(x => x.GuizmoId!.Value > 0)` guard on the async rule ensures the DB check only runs if the value already passed the `> 0` check, avoiding a pointless DB query for invalid input.

> **Note on layering:** `GuizmoRecommendedQueryValidator` lives in `Application` but receives `AppDbContext` (from `Infrastructure`) via DI. This is a deliberate pragmatic decision that matches the existing `GuizmoService` pattern in this codebase — the `Application` project already references `Infrastructure`.

- [ ] **Step 4: Run the validator tests to confirm they pass**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx \
  --filter "FullyQualifiedName~GuizmoRecommendedQueryValidatorTests"
```

Expected: 5 test executions PASS — 3 `[Fact]` methods + 1 `[Theory]` with 2 `[InlineData]` rows = 5 total runs reported by xunit.

- [ ] **Step 5: Run full test suite**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/GuizmoApi.Application/Validators/GuizmoRecommendedQueryValidator.cs \
        tests/GuizmoApi.Application.Tests/Validators/GuizmoRecommendedQueryValidatorTests.cs
git commit -m "feat: add GuizmoRecommendedQueryValidator with async DB existence check"
```

---

## Task 5: Add controller action and controller tests (TDD)

**Files:**
- Modify: `tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs`
- Modify: `src/GuizmoApi.Api/Controllers/GuizmosController.cs`

- [ ] **Step 1: Write the failing controller tests**

Open `tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs` and add two test methods at the end of the class, before the closing `}`:

```csharp
[Fact]
public async Task GetRecommended_returns_200_with_list()
{
    var dtos = new List<GuizmoDto> { new(1, "Widget", "Acme", null, 9.99m, 1, "Electronics") };
    _serviceMock.Setup(s => s.GetRecommendedAsync(5, null, default)).ReturnsAsync(dtos);

    var result = await _controller.GetRecommended(5, new GuizmoRecommendedQuery(null), default);

    var ok = result.Result as OkObjectResult;
    ok.Should().NotBeNull();
    ok!.StatusCode.Should().Be(200);
    ok.Value.Should().BeEquivalentTo(dtos);
}

[Fact]
public async Task GetRecommended_passes_userId_and_guizmoId_to_service()
{
    _serviceMock.Setup(s => s.GetRecommendedAsync(7, 42, default))
        .ReturnsAsync(Enumerable.Empty<GuizmoDto>());

    await _controller.GetRecommended(7, new GuizmoRecommendedQuery(42), default);

    _serviceMock.Verify(s => s.GetRecommendedAsync(7, 42, default), Times.Once);
}
```

Also add `using GuizmoApi.Application.DTOs;` if `GuizmoRecommendedQuery` is not already imported (it's in the same namespace as the other DTOs, so it should be covered by the existing `using GuizmoApi.Application.DTOs;`).

- [ ] **Step 2: Attempt to build to confirm compilation fails**

```bash
dotnet build /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: compilation failure — `_controller.GetRecommended` does not exist yet. This is the expected "red" state.

- [ ] **Step 3: Add `GetRecommended` action to `GuizmosController`**

Open `src/GuizmoApi.Api/Controllers/GuizmosController.cs` and add this action after the `GetPaged` action and before `GetById`:

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

- [ ] **Step 4: Run the new controller tests to confirm they pass**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx \
  --filter "FullyQualifiedName~GetRecommended"
```

Expected: 2 tests PASS.

- [ ] **Step 5: Run full test suite to confirm no regressions**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.slnx
```

Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/GuizmoApi.Api/Controllers/GuizmosController.cs \
        tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs
git commit -m "feat: add GET /api/guizmos/{userId}/recommended controller action with tests"
```
