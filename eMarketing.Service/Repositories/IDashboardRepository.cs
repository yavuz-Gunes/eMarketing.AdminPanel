using eMarketing.Service.Dtos;

namespace eMarketing.Service.Repositories;

public interface IDashboardRepository
{
    Task<DashboardSummaryDto?> GetSummaryAsync(int? storeId, bool allStores, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardRecentOrderDto>> GetRecentOrdersAsync(int? storeId, bool allStores, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardCriticalStockDto>> GetCriticalStockAsync(int? storeId, bool allStores, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
}
