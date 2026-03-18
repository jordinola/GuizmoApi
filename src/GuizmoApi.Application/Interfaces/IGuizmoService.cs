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
