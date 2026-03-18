namespace GuizmoApi.Application.Interfaces;

public interface IExternalGuizmoApiClient
{
    Task<IEnumerable<int>> GetRecommendedIdsAsync(int userId, int? guizmoId, CancellationToken ct = default);
}
