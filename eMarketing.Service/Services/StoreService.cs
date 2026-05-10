using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;

namespace eMarketing.Service.Services;

public interface IStoreService
{
    Task<IReadOnlyList<StoreDto>> GetStoresAsync(string search, bool onlyActive, CancellationToken cancellationToken = default);
    Task<StoreDto?> GetStoreAsync(int storeId, CancellationToken cancellationToken = default);
}

public sealed class StoreService : IStoreService
{
    private readonly IStoreRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public StoreService(IStoreRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public Task<IReadOnlyList<StoreDto>> GetStoresAsync(string search, bool onlyActive, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetStoresAsync(search, onlyActive, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public Task<StoreDto?> GetStoreAsync(int storeId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetStoreAsync(storeId, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }
}
