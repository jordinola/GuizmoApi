using Microsoft.EntityFrameworkCore;
using GuizmoApi.Domain.Interfaces;
using GuizmoApi.Infrastructure.Data;

namespace GuizmoApi.Infrastructure.ExternalApi;

public class StubExternalGuizmoApiClient(AppDbContext context) : IExternalGuizmoApiClient
{
    public async Task<IEnumerable<int>> GetRecommendedIdsAsync(int userId, int? guizmoId, CancellationToken ct = default)
    {
        // Stub: userId and guizmoId are intentionally ignored; the real API will use them
        var ids = await context.Guizmos.Select(g => g.Id).ToListAsync(ct);
        return ids.OrderBy(_ => Random.Shared.Next()).Take(3);
    }
}
