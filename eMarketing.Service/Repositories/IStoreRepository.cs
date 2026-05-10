using eMarketing.Service.Dtos;

namespace eMarketing.Service.Repositories;

public interface IStoreRepository
{
    Task<IReadOnlyList<StoreDto>> GetStoresAsync(string search, bool onlyActive, int? userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<StoreDto?> GetStoreAsync(int storeId, int? userId, bool isAdmin, CancellationToken cancellationToken = default);
}
