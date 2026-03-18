namespace GuizmoApi.Domain.Interfaces;

public interface IExternalGuizmoApiClient
{
    Task<IEnumerable<int>> GetRecommendedIdsAsync(int userId, int? guizmoId, CancellationToken ct = default);
}
