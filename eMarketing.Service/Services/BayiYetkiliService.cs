using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;
using Microsoft.Extensions.Logging;

namespace eMarketing.Service.Services;

public interface IBayiYetkiliService
{
    Task<IReadOnlyList<BayiYetkiliDto>> GetAsync(BayiYetkiliFilterRequest filter, CancellationToken cancellationToken = default);
    Task<BayiYetkiliDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(BayiYetkiliSaveRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(int id, bool active, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SiparisYetkilisiDto>> GetOrderAuthoritiesAsync(int storeId, CancellationToken cancellationToken = default);
}

public sealed class BayiYetkiliService : IBayiYetkiliService
{
    private readonly IBayiYetkiliRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BayiYetkiliService> _logger;

    public BayiYetkiliService(IBayiYetkiliRepository repository, ICurrentUserService currentUserService, ILogger<BayiYetkiliService> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public Task<IReadOnlyList<BayiYetkiliDto>> GetAsync(BayiYetkiliFilterRequest filter, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetAsync(filter, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public Task<BayiYetkiliDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetByIdAsync(id, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public async Task<int> SaveAsync(BayiYetkiliSaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.KullaniciId <= 0)
            throw new ArgumentException("Yetkili kullanıcı seçimi zorunludur.");

        if (request.BayiId <= 0)
            throw new ArgumentException("Bayi seçimi zorunludur.");

        if (request.MagazaId <= 0)
            throw new ArgumentException("Mağaza seçimi zorunludur.");

        request.YetkiTipi = "SiparisYetkilisi";

        int id = await _repository.SaveAsync(request, cancellationToken);
        _logger.LogInformation("Dealer authority saved. Id: {BayiYetkiliId}, UserId: {KullaniciId}, StoreId: {MagazaId}, ActorUserId: {ActorUserId}",
            id, request.KullaniciId, request.MagazaId, _currentUserService.CurrentUser.UserId);
        return id;
    }

    public async Task SetStatusAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        await _repository.SetStatusAsync(id, active, cancellationToken);
        _logger.LogInformation("Dealer authority status updated. Id: {BayiYetkiliId}, Active: {AktifMi}, ActorUserId: {ActorUserId}",
            id, active, _currentUserService.CurrentUser.UserId);
    }

    public Task<IReadOnlyList<SiparisYetkilisiDto>> GetOrderAuthoritiesAsync(int storeId, CancellationToken cancellationToken = default)
    {
        if (storeId <= 0)
            throw new ArgumentException("Mağaza seçimi zorunludur.");

        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetOrderAuthoritiesAsync(storeId, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }
}
