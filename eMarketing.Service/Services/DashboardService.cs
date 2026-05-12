using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;

namespace eMarketing.Service.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardRecentOrderDto>> GetRecentOrdersAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DashboardCriticalStockDto>> GetCriticalStockAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
}

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(IDashboardRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return await _repository.GetSummaryAsync(storeId, allStores, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken)
            ?? new DashboardSummaryDto();
    }

    public Task<IReadOnlyList<DashboardRecentOrderDto>> GetRecentOrdersAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetRecentOrdersAsync(storeId, allStores, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public Task<IReadOnlyList<DashboardCriticalStockDto>> GetCriticalStockAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetCriticalStockAsync(storeId, allStores, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }
}
