using eMarketing.Service.Dtos;

namespace eMarketing.Service.Repositories;

public interface IDealerRepository
{
    Task<IReadOnlyList<DealerDetailDto>> GetDealersAsync(string search, int status, CancellationToken cancellationToken = default);
    Task<DealerDetailDto?> GetDealerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateDealerAsync(DealerSaveRequest request, CancellationToken cancellationToken = default);
    Task UpdateDealerAsync(int id, DealerSaveRequest request, CancellationToken cancellationToken = default);
    Task SetDealerStatusAsync(int id, bool active, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DealerStoreDto>> GetStoresAsync(int customerId, int status, CancellationToken cancellationToken = default);
    Task<DealerStoreDto?> GetStoreByIdAsync(int storeId, CancellationToken cancellationToken = default);
    Task<int> CreateStoreAsync(int customerId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default);
    Task UpdateStoreAsync(int storeId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default);
    Task SetStoreStatusAsync(int storeId, bool active, CancellationToken cancellationToken = default);
}
