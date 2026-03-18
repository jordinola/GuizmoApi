# Guizmo Paginated List Endpoint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `GET /api/guizmos/paged` endpoint that returns guizmos paginated and sorted by category name.

**Architecture:** A new `GetPagedAsync` method is added to `IGuizmoService` and implemented in `GuizmoService` using EF Core `OrderBy`/`Skip`/`Take`. A FluentValidation validator enforces `PageNumber >= 1` and `PageSize >= 1` (consistent with existing validators). The controller exposes a new action at `GET /api/guizmos/paged` accepting query params and returning a generic `PagedResult<GuizmoDto>`. The existing `GET /api/guizmos` endpoint is left unchanged.

**Tech Stack:** .NET 8, ASP.NET Core, EF Core 8 (Npgsql), FluentValidation 11, FluentAssertions, xUnit, Moq, EF Core InMemory (tests)

> **Note:** `GuizmoService.ToDto` accesses `g.Category.Name` directly. This is safe because `GetPagedAsync` always calls `.Include(x => x.Category)` before projecting. Never call `ToDto` on a `Guizmo` entity without first loading the `Category` navigation property.

> **`SortOrder` design intent:** `SortOrder` is intentionally not validated — any value that is not `"desc"` (case-insensitive) is silently treated as ascending. This keeps the validator focused on structural constraints and allows callers to omit the parameter freely. "asc" and "desc" are the documented values; anything else defaults to ascending.

---

## File Map

| Action | File | Responsibility |
|--------|------|----------------|
| Create | `src/GuizmoApi.Application/DTOs/PagedResult.cs` | Generic pagination wrapper returned by the endpoint |
| Create | `src/GuizmoApi.Application/DTOs/GuizmoPagedQuery.cs` | Query parameters record for the paginated endpoint |
| Create | `src/GuizmoApi.Application/Validators/GuizmoPagedQueryValidator.cs` | Validates `PageNumber >= 1` and `PageSize >= 1` |
| Modify | `src/GuizmoApi.Application/Interfaces/IGuizmoService.cs` | Add `GetPagedAsync` method signature |
| Modify | `src/GuizmoApi.Application/Services/GuizmoService.cs` | Implement `GetPagedAsync` with EF Core ordering + paging |
| Modify | `src/GuizmoApi.Api/Controllers/GuizmosController.cs` | Add `GetPaged` action at `GET /api/guizmos/paged` |
| Modify | `tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs` | Integration tests for `GetPagedAsync` |
| Modify | `tests/GuizmoApi.Application.Tests/Validators/GuizmoPagedQueryValidatorTests.cs` | Unit tests for the validator |
| Modify | `tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs` | Unit tests for `GetPaged` controller action |

---

## Task 1: Create DTO types and validator

**Files:**
- Create: `src/GuizmoApi.Application/DTOs/PagedResult.cs`
- Create: `src/GuizmoApi.Application/DTOs/GuizmoPagedQuery.cs`
- Create: `src/GuizmoApi.Application/Validators/GuizmoPagedQueryValidator.cs`

- [ ] **Step 1: Create `PagedResult<T>`**

Create `src/GuizmoApi.Application/DTOs/PagedResult.cs`:

```csharp
namespace GuizmoApi.Application.DTOs;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
```

- [ ] **Step 2: Create `GuizmoPagedQuery`**

Create `src/GuizmoApi.Application/DTOs/GuizmoPagedQuery.cs`:

```csharp
namespace GuizmoApi.Application.DTOs;

public record GuizmoPagedQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SortOrder = "asc"
);
```

- [ ] **Step 3: Write failing validator tests**

Create `tests/GuizmoApi.Application.Tests/Validators/GuizmoPagedQueryValidatorTests.cs`:

```csharp
using FluentValidation.TestHelper;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Validators;

namespace GuizmoApi.Application.Tests.Validators;

public class GuizmoPagedQueryValidatorTests
{
    private readonly GuizmoPagedQueryValidator _validator = new();

    [Fact]
    public void Should_pass_when_all_fields_are_valid()
    {
        var result = _validator.TestValidate(new GuizmoPagedQuery(1, 10, "asc"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_fail_when_page_number_is_below_one(int pageNumber)
    {
        var result = _validator.TestValidate(new GuizmoPagedQuery(PageNumber: pageNumber));
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_fail_when_page_size_is_below_one(int pageSize)
    {
        var result = _validator.TestValidate(new GuizmoPagedQuery(PageSize: pageSize));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
```

- [ ] **Step 4: Run tests to verify they fail**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/tests/GuizmoApi.Application.Tests \
  --filter "GuizmoPagedQueryValidatorTests"
```

Expected: FAIL — `GuizmoPagedQueryValidator` does not exist yet.

- [ ] **Step 5: Create `GuizmoPagedQueryValidator`**

Create `src/GuizmoApi.Application/Validators/GuizmoPagedQueryValidator.cs`:

```csharp
using FluentValidation;
using GuizmoApi.Application.DTOs;

namespace GuizmoApi.Application.Validators;

public class GuizmoPagedQueryValidator : AbstractValidator<GuizmoPagedQuery>
{
    public GuizmoPagedQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}
```

- [ ] **Step 6: Run validator tests to verify they pass**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/tests/GuizmoApi.Application.Tests \
  --filter "GuizmoPagedQueryValidatorTests"
```

Expected: 5 tests PASS.

- [ ] **Step 7: Build to verify no compilation errors**

```bash
dotnet build /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.sln
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 8: Commit**

```bash
git add src/GuizmoApi.Application/DTOs/PagedResult.cs \
        src/GuizmoApi.Application/DTOs/GuizmoPagedQuery.cs \
        src/GuizmoApi.Application/Validators/GuizmoPagedQueryValidator.cs \
        tests/GuizmoApi.Application.Tests/Validators/GuizmoPagedQueryValidatorTests.cs
git commit -m "feat: add PagedResult, GuizmoPagedQuery DTOs and validator"
```

---

## Task 2: Extend `IGuizmoService` and add the interface to the contract

**Files:**
- Modify: `src/GuizmoApi.Application/Interfaces/IGuizmoService.cs`

- [ ] **Step 1: Add `GetPagedAsync` to the interface**

Edit `src/GuizmoApi.Application/Interfaces/IGuizmoService.cs`:

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
}
```

- [ ] **Step 2: Build to confirm expected compile error**

```bash
dotnet build /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.sln
```

Expected: FAIL with error: `'GuizmoService' does not implement interface member 'IGuizmoService.GetPagedAsync'`. This confirms the interface change is correct and the implementation is next.

- [ ] **Step 3: Commit the interface change**

```bash
git add src/GuizmoApi.Application/Interfaces/IGuizmoService.cs
git commit -m "feat: add GetPagedAsync to IGuizmoService"
```

---

## Task 3: Write failing service tests

**Files:**
- Modify: `tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs`

- [ ] **Step 1: Append failing tests for `GetPagedAsync`**

Append these test methods inside the `GuizmoServiceTests` class in `tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs`:

```csharp
[Fact]
public async Task GetPagedAsync_returns_correct_page_and_total_count()
{
    await using var context = CreateContext(nameof(GetPagedAsync_returns_correct_page_and_total_count));
    var cat = await AddCategoryAsync(context, "Electronics");
    for (var i = 1; i <= 15; i++)
        context.Guizmos.Add(new Guizmo { Name = $"Widget{i}", Manufacturer = "Acme", Msrp = i, CategoryId = cat.Id });
    await context.SaveChangesAsync();

    var service = new GuizmoService(context);
    var result = await service.GetPagedAsync(new GuizmoPagedQuery(PageNumber: 2, PageSize: 5));

    result.TotalCount.Should().Be(15);
    result.PageNumber.Should().Be(2);
    result.PageSize.Should().Be(5);
    result.TotalPages.Should().Be(3);
    result.Items.Should().HaveCount(5);
}

[Fact]
public async Task GetPagedAsync_sorts_by_category_name_ascending_by_default()
{
    await using var context = CreateContext(nameof(GetPagedAsync_sorts_by_category_name_ascending_by_default));
    var catZ = await AddCategoryAsync(context, "Zeta");
    var catA = await AddCategoryAsync(context, "Alpha");
    context.Guizmos.Add(new Guizmo { Name = "WidgetZ", Manufacturer = "Acme", Msrp = 1m, CategoryId = catZ.Id });
    context.Guizmos.Add(new Guizmo { Name = "WidgetA", Manufacturer = "Acme", Msrp = 1m, CategoryId = catA.Id });
    await context.SaveChangesAsync();

    var service = new GuizmoService(context);
    var result = await service.GetPagedAsync(new GuizmoPagedQuery());

    var items = result.Items.ToList();
    items[0].CategoryName.Should().Be("Alpha");
    items[1].CategoryName.Should().Be("Zeta");
}

[Fact]
public async Task GetPagedAsync_sorts_by_category_name_descending()
{
    await using var context = CreateContext(nameof(GetPagedAsync_sorts_by_category_name_descending));
    var catA = await AddCategoryAsync(context, "Alpha");
    var catZ = await AddCategoryAsync(context, "Zeta");
    context.Guizmos.Add(new Guizmo { Name = "WidgetA", Manufacturer = "Acme", Msrp = 1m, CategoryId = catA.Id });
    context.Guizmos.Add(new Guizmo { Name = "WidgetZ", Manufacturer = "Acme", Msrp = 1m, CategoryId = catZ.Id });
    await context.SaveChangesAsync();

    var service = new GuizmoService(context);
    var result = await service.GetPagedAsync(new GuizmoPagedQuery(SortOrder: "desc"));

    var items = result.Items.ToList();
    items[0].CategoryName.Should().Be("Zeta");
    items[1].CategoryName.Should().Be("Alpha");
}

[Fact]
public async Task GetPagedAsync_sort_order_is_case_insensitive()
{
    await using var context = CreateContext(nameof(GetPagedAsync_sort_order_is_case_insensitive));
    var catA = await AddCategoryAsync(context, "Alpha");
    var catZ = await AddCategoryAsync(context, "Zeta");
    context.Guizmos.Add(new Guizmo { Name = "WidgetA", Manufacturer = "Acme", Msrp = 1m, CategoryId = catA.Id });
    context.Guizmos.Add(new Guizmo { Name = "WidgetZ", Manufacturer = "Acme", Msrp = 1m, CategoryId = catZ.Id });
    await context.SaveChangesAsync();

    var service = new GuizmoService(context);
    var result = await service.GetPagedAsync(new GuizmoPagedQuery(SortOrder: "DESC"));

    var items = result.Items.ToList();
    items[0].CategoryName.Should().Be("Zeta");
    items[1].CategoryName.Should().Be("Alpha");
}

[Fact]
public async Task GetPagedAsync_last_page_returns_remaining_items()
{
    await using var context = CreateContext(nameof(GetPagedAsync_last_page_returns_remaining_items));
    var cat = await AddCategoryAsync(context, "Electronics");
    for (var i = 1; i <= 7; i++)
        context.Guizmos.Add(new Guizmo { Name = $"Widget{i}", Manufacturer = "Acme", Msrp = i, CategoryId = cat.Id });
    await context.SaveChangesAsync();

    var service = new GuizmoService(context);
    var result = await service.GetPagedAsync(new GuizmoPagedQuery(PageNumber: 2, PageSize: 5));

    result.TotalCount.Should().Be(7);
    result.TotalPages.Should().Be(2);
    result.Items.Should().HaveCount(2);
}

[Fact]
public async Task GetPagedAsync_returns_empty_result_when_no_guizmos_exist()
{
    await using var context = CreateContext(nameof(GetPagedAsync_returns_empty_result_when_no_guizmos_exist));

    var service = new GuizmoService(context);
    var result = await service.GetPagedAsync(new GuizmoPagedQuery());

    result.TotalCount.Should().Be(0);
    result.TotalPages.Should().Be(0);
    result.Items.Should().BeEmpty();
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/tests/GuizmoApi.Application.Tests \
  --filter "GetPagedAsync"
```

Expected: FAIL — build error because `GuizmoService` does not yet implement `GetPagedAsync`.

---

## Task 4: Implement `GetPagedAsync` in `GuizmoService`

**Files:**
- Modify: `src/GuizmoApi.Application/Services/GuizmoService.cs`

- [ ] **Step 1: Add `GetPagedAsync` implementation**

Add this method to `GuizmoService` (after `GetAllAsync`, before `GetByIdAsync`):

```csharp
public async Task<PagedResult<GuizmoDto>> GetPagedAsync(GuizmoPagedQuery query, CancellationToken ct = default)
{
    var q = _context.Guizmos.Include(x => x.Category).AsNoTracking();

    q = query.SortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase)
        ? q.OrderByDescending(x => x.Category!.Name)
        : q.OrderBy(x => x.Category!.Name);

    var totalCount = await q.CountAsync(ct);
    var items = await q
        .Skip((query.PageNumber - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync(ct);

    var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

    return new PagedResult<GuizmoDto>(
        items.Select(ToDto),
        totalCount,
        query.PageNumber,
        query.PageSize,
        totalPages
    );
}
```

- [ ] **Step 2: Run service tests to verify they pass**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/tests/GuizmoApi.Application.Tests \
  --filter "GetPagedAsync"
```

Expected: 6 tests PASS.

- [ ] **Step 3: Run all application tests to confirm no regressions**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/tests/GuizmoApi.Application.Tests
```

Expected: All tests PASS.

- [ ] **Step 4: Commit**

```bash
git add src/GuizmoApi.Application/Services/GuizmoService.cs \
        tests/GuizmoApi.Application.Tests/Services/GuizmoServiceTests.cs
git commit -m "feat: implement GetPagedAsync sorted by category"
```

---

## Task 5: Write failing controller tests and implement `GetPaged`

**Files:**
- Modify: `tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs`
- Modify: `src/GuizmoApi.Api/Controllers/GuizmosController.cs`

- [ ] **Step 1: Write failing controller tests**

Append these test methods inside `GuizmosControllerTests` in `tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs`:

```csharp
[Fact]
public async Task GetPaged_returns_200_with_paged_result()
{
    var pagedResult = new PagedResult<GuizmoDto>(
        Items: new List<GuizmoDto> { new(1, "Widget", "Acme", null, 9.99m, 1, "Electronics") },
        TotalCount: 1,
        PageNumber: 1,
        PageSize: 10,
        TotalPages: 1
    );
    _serviceMock.Setup(s => s.GetPagedAsync(It.IsAny<GuizmoPagedQuery>(), default))
        .ReturnsAsync(pagedResult);

    var result = await _controller.GetPaged(new GuizmoPagedQuery(), default);

    var ok = result.Result as OkObjectResult;
    ok.Should().NotBeNull();
    ok!.StatusCode.Should().Be(200);
    ok.Value.Should().BeEquivalentTo(pagedResult);
}

[Fact]
public async Task GetPaged_passes_query_params_to_service()
{
    var query = new GuizmoPagedQuery(PageNumber: 2, PageSize: 5, SortOrder: "desc");
    var pagedResult = new PagedResult<GuizmoDto>(
        Items: Enumerable.Empty<GuizmoDto>(),
        TotalCount: 0,
        PageNumber: 2,
        PageSize: 5,
        TotalPages: 0
    );
    _serviceMock.Setup(s => s.GetPagedAsync(query, default)).ReturnsAsync(pagedResult);

    var result = await _controller.GetPaged(query, default);

    _serviceMock.Verify(s => s.GetPagedAsync(query, default), Times.Once);
    result.Result.Should().BeOfType<OkObjectResult>();
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/tests/GuizmoApi.Api.Tests \
  --filter "GetPaged"
```

Expected: FAIL — `GetPaged` action does not exist yet on `GuizmosController`.

- [ ] **Step 3: Add `GetPaged` action to `GuizmosController`**

Add this action to `GuizmosController` (after `GetAll`, before `GetById`):

```csharp
/// <summary>Returns a paginated list of Guizmos sorted by category name.</summary>
[HttpGet("paged")]
[ProducesResponseType(typeof(PagedResult<GuizmoDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<PagedResult<GuizmoDto>>> GetPaged(
    [FromQuery] GuizmoPagedQuery query,
    CancellationToken ct)
{
    var result = await _service.GetPagedAsync(query, ct);
    return Ok(result);
}
```

- [ ] **Step 4: Run controller tests to verify they pass**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/tests/GuizmoApi.Api.Tests \
  --filter "GetPaged"
```

Expected: 2 tests PASS.

- [ ] **Step 5: Run all tests to confirm no regressions**

```bash
dotnet test /Users/james.ordinola/Documents/Repos/Actabl_Test/GuizmoApi/GuizmoApi.sln
```

Expected: All tests PASS.

- [ ] **Step 6: Commit**

```bash
git add src/GuizmoApi.Api/Controllers/GuizmosController.cs \
        tests/GuizmoApi.Api.Tests/Controllers/GuizmosControllerTests.cs
git commit -m "feat: add GET /api/guizmos/paged endpoint"
```
