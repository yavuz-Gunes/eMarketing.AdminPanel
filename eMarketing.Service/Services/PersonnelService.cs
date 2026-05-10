using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;
using Microsoft.Extensions.Logging;

namespace eMarketing.Service.Services;

public interface IPersonnelService
{
    Task<IReadOnlyList<PersonnelDto>> GetPersonnelAsync(PersonnelFilterRequest filter, CancellationToken cancellationToken = default);
    Task<int> SavePersonnelAsync(CreatePersonnelRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonnelStorePermissionDto>> GetStoresAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonnelStorePermissionDto>> GetAssignableStoresAsync(int userId, string search, CancellationToken cancellationToken = default);
    Task AssignStoreAsync(int userId, int storeId, CancellationToken cancellationToken = default);
    Task RemoveStoreAsync(int userStoreId, CancellationToken cancellationToken = default);
}

public sealed class PersonnelService : IPersonnelService
{
    private readonly IPersonnelRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<PersonnelService> _logger;

    public PersonnelService(IPersonnelRepository repository, ICurrentUserService currentUserService, IPasswordService passwordService, ILogger<PersonnelService> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _passwordService = passwordService;
        _logger = logger;
    }

    public Task<IReadOnlyList<PersonnelDto>> GetPersonnelAsync(PersonnelFilterRequest filter, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetPersonnelAsync(filter, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public async Task<int> SavePersonnelAsync(CreatePersonnelRequest request, CancellationToken cancellationToken = default)
    {
        var saveRequest = new CreatePersonnelRequest
        {
            KullaniciId = request.KullaniciId,
            KullaniciAdi = request.KullaniciAdi,
            Sifre = BuildPasswordForSave(request),
            AdSoyad = request.AdSoyad,
            Rol = request.Rol,
            AktifMi = request.AktifMi
        };

        int id = await _repository.SavePersonnelAsync(saveRequest, cancellationToken);
        _logger.LogInformation("Personnel saved. PersonnelId: {PersonnelId}, ActorUserId: {ActorUserId}", id, _currentUserService.CurrentUser.UserId);
        return id;
    }

    private string BuildPasswordForSave(CreatePersonnelRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Sifre))
            return _passwordService.HashPassword(request.Sifre.Trim());

        if (request.KullaniciId.HasValue && request.KullaniciId.Value > 0)
            return string.Empty;

        return _passwordService.HashPassword("1234");
    }

    public Task<IReadOnlyList<PersonnelStorePermissionDto>> GetStoresAsync(int userId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetStoresAsync(userId, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public Task<IReadOnlyList<PersonnelStorePermissionDto>> GetAssignableStoresAsync(int userId, string search, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        return _repository.GetAssignableStoresAsync(userId, search, currentUser.UserId, currentUser.CanSeeAllStores, cancellationToken);
    }

    public async Task AssignStoreAsync(int userId, int storeId, CancellationToken cancellationToken = default)
    {
        await _repository.AssignStoreAsync(userId, storeId, cancellationToken);
        _logger.LogInformation("Personnel store permission assigned. PersonnelId: {PersonnelId}, StoreId: {StoreId}, ActorUserId: {ActorUserId}", userId, storeId, _currentUserService.CurrentUser.UserId);
    }

    public async Task RemoveStoreAsync(int userStoreId, CancellationToken cancellationToken = default)
    {
        await _repository.RemoveStoreAsync(userStoreId, cancellationToken);
        _logger.LogInformation("Personnel store permission removed. UserStoreId: {UserStoreId}, ActorUserId: {ActorUserId}", userStoreId, _currentUserService.CurrentUser.UserId);
    }
}
