using eMarketing.Service.Dtos;

namespace eMarketing.Service.Repositories;

public interface IStockRepository
{
    Task<IReadOnlyList<StockItemDto>> GetStocksAsync(StockFilterRequest filter, int? userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<StockSummaryDto> GetSummaryAsync(int? storeId, bool allStores, int? userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(int storeId, int productId, int count, int? userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task UpdateMinimumAsync(int storeStockId, int minimumStock, CancellationToken cancellationToken = default);
    Task ProcessMovementAsync(StockOperationRequest request, CancellationToken cancellationToken = default);
}
