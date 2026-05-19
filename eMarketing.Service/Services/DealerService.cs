using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;

namespace eMarketing.Service.Services;

public interface IDealerService
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

public sealed class DealerService : IDealerService
{
    private readonly IDealerRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreAuthorizationService _storeAuthorizationService;

    public DealerService(
        IDealerRepository repository,
        ICurrentUserService currentUserService,
        IStoreAuthorizationService storeAuthorizationService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _storeAuthorizationService = storeAuthorizationService;
    }

    public Task<IReadOnlyList<DealerDetailDto>> GetDealersAsync(string search, int status, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return _repository.GetDealersAsync(search, status, cancellationToken);
    }

    public Task<DealerDetailDto?> GetDealerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return _repository.GetDealerByIdAsync(id, cancellationToken);
    }

    public Task<int> CreateDealerAsync(DealerSaveRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return _repository.CreateDealerAsync(request, cancellationToken);
    }

    public Task UpdateDealerAsync(int id, DealerSaveRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return _repository.UpdateDealerAsync(id, request, cancellationToken);
    }

    public Task SetDealerStatusAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return _repository.SetDealerStatusAsync(id, active, cancellationToken);
    }

    public Task<IReadOnlyList<DealerStoreDto>> GetStoresAsync(int customerId, int status, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return _repository.GetStoresAsync(customerId, status, cancellationToken);
    }

    public async Task<DealerStoreDto?> GetStoreByIdAsync(int storeId, CancellationToken cancellationToken = default)
    {
        await _storeAuthorizationService.EnsureStoreAccessAsync(storeId, cancellationToken);
        return await _repository.GetStoreByIdAsync(storeId, cancellationToken);
    }

    public Task<int> CreateStoreAsync(int customerId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return _repository.CreateStoreAsync(customerId, request, cancellationToken);
    }

    public async Task UpdateStoreAsync(int storeId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default)
    {
        await _storeAuthorizationService.EnsureStoreAccessAsync(storeId, cancellationToken);
        await _repository.UpdateStoreAsync(storeId, request, cancellationToken);
    }

    public async Task SetStoreStatusAsync(int storeId, bool active, CancellationToken cancellationToken = default)
    {
        await _storeAuthorizationService.EnsureStoreAccessAsync(storeId, cancellationToken);
        await _repository.SetStoreStatusAsync(storeId, active, cancellationToken);
    }

    private void EnsureAdmin()
    {
        if (!_currentUserService.CurrentUser.IsAdmin)
            throw new UnauthorizedAccessException("Bu bayi yönetim işlemi için admin yetkisi gerekir.");
    }
}
