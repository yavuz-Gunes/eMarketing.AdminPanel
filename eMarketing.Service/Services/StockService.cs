using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;
using Microsoft.Extensions.Logging;

namespace eMarketing.Service.Services;

public interface IStockService
{
    Task<IReadOnlyList<StockItemDto>> GetStocksAsync(StockFilterRequest filter, CancellationToken cancellationToken = default);
    Task<StockSummaryDto> GetSummaryAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(int storeId, int productId, int count, CancellationToken cancellationToken = default);
    Task UpdateMinimumAsync(int storeStockId, int minimumStock, CancellationToken cancellationToken = default);
    Task ProcessMovementAsync(StockOperationRequest request, CancellationToken cancellationToken = default);
    Task ProcessCentralStockAsync(CentralStockOperationRequest request, CancellationToken cancellationToken = default);
}

public sealed class StockService : IStockService
{
    private readonly IStockRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<StockService> _logger;

    public StockService(IStockRepository repository, ICurrentUserService currentUserService, ILogger<StockService> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public Task<IReadOnlyList<StockItemDto>> GetStocksAsync(StockFilterRequest filter, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetStocksAsync(filter, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public Task<StockSummaryDto> GetSummaryAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetSummaryAsync(storeId, currentUser.CanSeeAllStores && allStores, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(int storeId, int productId, int count, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetMovementsAsync(storeId, productId, count, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public Task UpdateMinimumAsync(int storeStockId, int minimumStock, CancellationToken cancellationToken = default)
    {
        if (minimumStock < 0)
            throw new ArgumentException("Minimum stok negatif olamaz.", nameof(minimumStock));

        _logger.LogInformation("Stock minimum updated. StoreStockId: {StoreStockId}, MinimumStock: {MinimumStock}, ActorUserId: {ActorUserId}", storeStockId, minimumStock, _currentUserService.CurrentUser.UserId);
        return _repository.UpdateMinimumAsync(storeStockId, minimumStock, cancellationToken);
    }

    public Task ProcessMovementAsync(StockOperationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Manual stock movement requested. StoreId: {StoreId}, ProductId: {ProductId}, Type: {MovementType}, Quantity: {Quantity}, ActorUserId: {ActorUserId}", request.MagazaId, request.UrunId, request.HareketTipi, request.Miktar, _currentUserService.CurrentUser.UserId);
        return _repository.ProcessMovementAsync(request, cancellationToken);
    }

    public Task ProcessCentralStockAsync(CentralStockOperationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.UrunId <= 0)
            throw new ArgumentException("Ürün seçimi zorunludur.", nameof(request.UrunId));

        if (request.Miktar <= 0)
            throw new ArgumentException("Merkez stok miktarı sıfırdan büyük olmalıdır.", nameof(request.Miktar));

        _logger.LogInformation("Central stock increase requested. ProductId: {ProductId}, Quantity: {Quantity}, ActorUserId: {ActorUserId}", request.UrunId, request.Miktar, _currentUserService.CurrentUser.UserId);
        return _repository.ProcessCentralStockAsync(request, cancellationToken);
    }
}
