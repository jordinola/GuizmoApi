using Microsoft.AspNetCore.Mvc;
using GuizmoApi.Application.DTOs;
using GuizmoApi.Application.Interfaces;

namespace GuizmoApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuizmosController : ControllerBase
{
    private readonly IGuizmoService _service;

    public GuizmosController(IGuizmoService service)
    {
        _service = service;
    }

    /// <summary>Returns all Guizmos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GuizmoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GuizmoDto>>> GetAll(CancellationToken ct)
    {
        var guizmos = await _service.GetAllAsync(ct);
        return Ok(guizmos);
    }

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

    /// <summary>Returns a single Guizmo by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GuizmoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuizmoDto>> GetById(int id, CancellationToken ct)
    {
        var guizmo = await _service.GetByIdAsync(id, ct);
        return guizmo is null ? NotFound() : Ok(guizmo);
    }

    /// <summary>Creates a new Guizmo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(GuizmoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GuizmoDto>> Create(
        [FromBody] CreateGuizmoRequest request,
        CancellationToken ct)
    {
        var guizmo = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = guizmo.Id }, guizmo);
    }

    /// <summary>Updates an existing Guizmo.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(GuizmoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GuizmoDto>> Update(
        int id,
        [FromBody] UpdateGuizmoRequest request,
        CancellationToken ct)
    {
        var guizmo = await _service.UpdateAsync(id, request, ct);
        return guizmo is null ? NotFound() : Ok(guizmo);
    }

    /// <summary>Deletes a Guizmo by id.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
