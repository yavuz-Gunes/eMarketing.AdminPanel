using eMarketing.Service.Dtos;

namespace eMarketing.Service.Repositories;

public interface IOrderRepository
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
    Task<OrderSummaryDto> GetSummaryAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
    Task<int> CreateOrderAsync(OrderCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int orderId, string status, CancellationToken cancellationToken = default);
    Task CancelAsync(int orderId, CancellationToken cancellationToken = default);
}
