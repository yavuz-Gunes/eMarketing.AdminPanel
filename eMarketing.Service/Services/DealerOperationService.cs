using System.Data;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using eMarketing.Service.Repositories;
using eMarketing.Service.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace eMarketing.Service.Services;

public interface IDealerOperationService
{
    Task<StoreTeamResponseDto> GetTeamAsync(int storeId, CancellationToken cancellationToken = default);
    Task<int> CreatePersonnelAsync(int storeId, StorePersonnelCreateRequest request, CancellationToken cancellationToken = default);
    Task UpdateDutyAsync(int storeId, int userStoreId, string duty, CancellationToken cancellationToken = default);
    Task SetOrderAuthorityAsync(int storeId, int userId, bool active, string notes, CancellationToken cancellationToken = default);
    Task RemovePersonnelAsync(int storeId, int userStoreId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(int userId, StorePersonnelProfileUpdateRequest request, CancellationToken cancellationToken = default);
    Task UpdatePasswordAsync(int userId, StorePersonnelPasswordUpdateRequest request, CancellationToken cancellationToken = default);
}

public sealed class DealerOperationService : IDealerOperationService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IPersonnelService _personnelService;
    private readonly IBayiYetkiliRepository _authorityRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<DealerOperationService> _logger;

    public DealerOperationService(
        ISqlConnectionFactory connectionFactory,
        IPersonnelService personnelService,
        IBayiYetkiliRepository authorityRepository,
        ICurrentUserService currentUserService,
        IPasswordService passwordService,
        ILogger<DealerOperationService> logger)
    {
        _connectionFactory = connectionFactory;
        _personnelService = personnelService;
        _authorityRepository = authorityRepository;
        _currentUserService = currentUserService;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<StoreTeamResponseDto> GetTeamAsync(int storeId, CancellationToken cancellationToken = default)
    {
        StoreTeamContextDto context = await GetContextAsync(storeId, cancellationToken);
        IReadOnlyList<StoreTeamMemberDto> members = await GetMembersAsync(storeId, cancellationToken);

        return new StoreTeamResponseDto
        {
            Context = context,
            Members = members
        };
    }

    public async Task<int> CreatePersonnelAsync(int storeId, StorePersonnelCreateRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureSupervisorAsync(storeId, cancellationToken);

        string duty = NormalizeDuty(request.Gorev);
        int userId = await _personnelService.SavePersonnelAsync(new CreatePersonnelRequest
        {
            KullaniciAdi = request.KullaniciAdi,
            Sifre = request.Sifre,
            AdSoyad = request.AdSoyad,
            Telefon = request.Telefon,
            Email = request.Email,
            Rol = AppRoles.Personel,
            AktifMi = true
        }, cancellationToken);

        await _personnelService.AssignStoreAsync(userId, storeId, duty, cancellationToken);
        _logger.LogInformation("Dealer personnel created. UserId: {UserId}, StoreId: {StoreId}, Duty: {Duty}, ActorUserId: {ActorUserId}", userId, storeId, duty, _currentUserService.CurrentUser.UserId);
        return userId;
    }

    public async Task UpdateDutyAsync(int storeId, int userStoreId, string duty, CancellationToken cancellationToken = default)
    {
        await EnsureSupervisorAsync(storeId, cancellationToken);
        StoreMembership membership = await GetMembershipAsync(storeId, userStoreId, cancellationToken);
        EnsureNotSelf(membership.UserId, "Kendi mağaza görevinizi değiştiremezsiniz.");
        EnsureStorePermissionTargetAllowed(membership);
        string normalizedDuty = NormalizeDuty(duty);

        if (string.Equals(membership.Duty, "Supervisor", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalizedDuty, "Supervisor", StringComparison.OrdinalIgnoreCase))
            await EnsureNotLastSupervisorAsync(storeId, membership.UserId, cancellationToken);

        await _personnelService.UpdateStoreDutyAsync(userStoreId, normalizedDuty, cancellationToken);
        _logger.LogInformation("Dealer personnel duty updated. UserStoreId: {UserStoreId}, StoreId: {StoreId}, Duty: {Duty}, ActorUserId: {ActorUserId}", userStoreId, storeId, normalizedDuty, _currentUserService.CurrentUser.UserId);
    }

    public async Task SetOrderAuthorityAsync(int storeId, int userId, bool active, string notes, CancellationToken cancellationToken = default)
    {
        StoreTeamContextDto context = await GetContextAsync(storeId, cancellationToken);
        if (!context.SiparisYetkisiYonetebilir)
            throw new UnauthorizedAccessException("Sipariş yetkisi yönetmek için mağaza müdürü veya supervisor olmalısınız.");

        StoreMembership membership = await GetMembershipByUserAsync(storeId, userId, cancellationToken);
        EnsureNotSelf(membership.UserId, "Kendi sipariş yetkinizi değiştiremezsiniz.");
        EnsureStorePermissionTargetAllowed(membership);
        int customerId = await GetCustomerIdAsync(storeId, cancellationToken);
        int? authorityId = await GetOrderAuthorityIdAsync(storeId, membership.UserId, cancellationToken);

        if (authorityId.HasValue)
        {
            await _authorityRepository.SetStatusAsync(authorityId.Value, active, cancellationToken);
        }
        else if (active)
        {
            await _authorityRepository.SaveAsync(new BayiYetkiliSaveRequest
            {
                KullaniciId = membership.UserId,
                BayiId = customerId,
                MagazaId = storeId,
                YetkiTipi = "SiparisYetkilisi",
                Notlar = notes,
                AktifMi = true
            }, cancellationToken);
        }

        _logger.LogInformation("Dealer order authority changed. UserId: {UserId}, StoreId: {StoreId}, Active: {Active}, ActorUserId: {ActorUserId}", userId, storeId, active, _currentUserService.CurrentUser.UserId);
    }

    public async Task RemovePersonnelAsync(int storeId, int userStoreId, CancellationToken cancellationToken = default)
    {
        await EnsureSupervisorAsync(storeId, cancellationToken);
        StoreMembership membership = await GetMembershipAsync(storeId, userStoreId, cancellationToken);
        EnsureNotSelf(membership.UserId, "Kendi mağaza bağlantınızı kaldıramazsınız.");
        EnsureStorePermissionTargetAllowed(membership);
        if (string.Equals(membership.Duty, "Supervisor", StringComparison.OrdinalIgnoreCase))
            await EnsureNotLastSupervisorAsync(storeId, membership.UserId, cancellationToken);

        int? authorityId = await GetOrderAuthorityIdAsync(storeId, membership.UserId, cancellationToken);
        if (authorityId.HasValue)
            await _authorityRepository.SetStatusAsync(authorityId.Value, false, cancellationToken);

        await _personnelService.RemoveStoreAsync(userStoreId, cancellationToken);
        _logger.LogInformation("Dealer personnel removed from store. UserStoreId: {UserStoreId}, StoreId: {StoreId}, ActorUserId: {ActorUserId}", userStoreId, storeId, _currentUserService.CurrentUser.UserId);
    }

    public async Task UpdateProfileAsync(int userId, StorePersonnelProfileUpdateRequest request, CancellationToken cancellationToken = default)
    {
        EnsureCanManageAccount(userId);
        if (string.IsNullOrWhiteSpace(request.AdSoyad))
            throw new ArgumentException("Ad soyad zorunludur.");

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
UPDATE dbo.Kullanicilar
SET
    AdSoyad = @AdSoyad,
    Telefon = NULLIF(LTRIM(RTRIM(@Telefon)), N''),
    Email = NULLIF(LTRIM(RTRIM(@Email)), N'')
WHERE KullaniciId = @KullaniciId;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@AdSoyad", SqlDbType.NVarChar, 150).Value = request.AdSoyad.Trim();
        command.Parameters.Add("@Telefon", SqlDbType.NVarChar, 60).Value = (object?)request.Telefon?.Trim() ?? DBNull.Value;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 400).Value = (object?)request.Email?.Trim() ?? DBNull.Value;

        await connection.OpenAsync(cancellationToken);
        int affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
            throw new InvalidOperationException("Personel bulunamadı.");

        _logger.LogInformation("Dealer personnel profile updated. UserId: {UserId}, ActorUserId: {ActorUserId}", userId, _currentUserService.CurrentUser.UserId);
    }

    public async Task UpdatePasswordAsync(int userId, StorePersonnelPasswordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        EnsureCanManageAccount(userId);
        if (string.IsNullOrWhiteSpace(request.YeniSifre) || request.YeniSifre.Trim().Length < 4)
            throw new ArgumentException("Yeni şifre en az 4 karakter olmalıdır.");

        string passwordHash = _passwordService.HashPassword(request.YeniSifre.Trim());
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
UPDATE dbo.Kullanicilar
SET Sifre = @Sifre
WHERE KullaniciId = @KullaniciId;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@Sifre", SqlDbType.NVarChar, 100).Value = passwordHash;

        await connection.OpenAsync(cancellationToken);
        int affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
            throw new InvalidOperationException("Personel bulunamadı.");

        _logger.LogInformation("Dealer personnel password updated. UserId: {UserId}, ActorUserId: {ActorUserId}", userId, _currentUserService.CurrentUser.UserId);
    }

    private async Task EnsureSupervisorAsync(int storeId, CancellationToken cancellationToken)
    {
        StoreTeamContextDto context = await GetContextAsync(storeId, cancellationToken);
        if (!context.SupervisorMu)
            throw new UnauthorizedAccessException("Bu işlem için aktif mağazada supervisor olmalısınız.");
    }

    private void EnsureCanManageAccount(int targetUserId)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (!currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");

        if (currentUser.IsAdmin || currentUser.UserId.Value == targetUserId)
            return;

        throw new UnauthorizedAccessException("Bu personelin profil veya şifre bilgilerini güncelleme yetkiniz yok.");
    }

    private void EnsureNotSelf(int targetUserId, string message)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (currentUser.UserId.HasValue
            && currentUser.UserId.Value == targetUserId)
            throw new UnauthorizedAccessException(message);
    }

    private void EnsureStorePermissionTargetAllowed(StoreMembership membership)
    {
        if (IsSystemAdmin(membership.Role, membership.UserName))
            throw new UnauthorizedAccessException("Admin hesabının mağaza yetkileri bu ekrandan değiştirilemez.");
    }

    private async Task<StoreTeamContextDto> GetContextAsync(int storeId, CancellationToken cancellationToken)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (!currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");

        if (currentUser.CanSeeAllStores)
            return await GetAdminContextAsync(storeId, cancellationToken);

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT TOP 1
    cs.CustomerStoreId AS MagazaId,
    cs.StoreName AS MagazaAdi,
    COALESCE(NULLIF(c.CompanyName, N''), c.FullName) AS MusteriAdi,
    km.Gorev,
    CAST(CASE WHEN byk.BayiYetkiliId IS NULL THEN 0 ELSE 1 END AS bit) AS SiparisYetkilisiMi
FROM dbo.KullaniciMagazalari km
INNER JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = km.MagazaId
INNER JOIN dbo.Customers c ON c.CustomerId = cs.CustomerId
LEFT JOIN dbo.BayiYetkilileri byk
    ON byk.KullaniciId = km.KullaniciId
   AND byk.MagazaId = km.MagazaId
   AND byk.YetkiTipi = N'SiparisYetkilisi'
   AND byk.AktifMi = 1
WHERE km.KullaniciId = @KullaniciId
  AND km.MagazaId = @MagazaId
  AND km.AktifMi = 1
  AND cs.IsActive = 1
  AND c.IsActive = 1;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.Value;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new UnauthorizedAccessException("Bu mağaza için işlem yetkiniz yok.");

        return new StoreTeamContextDto
        {
            MagazaId = reader.GetInt("MagazaId"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            MusteriAdi = reader.GetText("MusteriAdi"),
            KullaniciGorev = reader.GetText("Gorev"),
            SiparisYetkilisiMi = reader.GetBool("SiparisYetkilisiMi")
        };
    }

    private async Task<StoreTeamContextDto> GetAdminContextAsync(int storeId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT TOP 1
    cs.CustomerStoreId AS MagazaId,
    cs.StoreName AS MagazaAdi,
    COALESCE(NULLIF(c.CompanyName, N''), c.FullName) AS MusteriAdi
FROM dbo.CustomerStores cs
INNER JOIN dbo.Customers c ON c.CustomerId = cs.CustomerId
WHERE cs.CustomerStoreId = @MagazaId
  AND cs.IsActive = 1
  AND c.IsActive = 1;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new UnauthorizedAccessException("Aktif bayi bulunamadı.");

        return new StoreTeamContextDto
        {
            MagazaId = reader.GetInt("MagazaId"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            MusteriAdi = reader.GetText("MusteriAdi"),
            KullaniciGorev = "Supervisor",
            SiparisYetkilisiMi = true
        };
    }

    private async Task<IReadOnlyList<StoreTeamMemberDto>> GetMembersAsync(int storeId, CancellationToken cancellationToken)
    {
        var members = new List<StoreTeamMemberDto>();

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT
    k.KullaniciId,
    km.KullaniciMagazaId,
    k.KullaniciAdi,
    k.AdSoyad,
    ISNULL(k.Email, N'') AS Email,
    ISNULL(k.Telefon, N'') AS Telefon,
    ISNULL(k.ImageUrl, N'') AS ImageUrl,
    ISNULL(k.Rol, N'') AS Rol,
    km.Gorev,
    CASE km.Gorev
        WHEN N'Supervisor' THEN N'Supervisor'
        WHEN N'MagazaMuduru' THEN N'Mağaza Müdürü'
        ELSE N'Personel'
    END AS GorevGorunenAd,
    CAST(CASE WHEN byk.BayiYetkiliId IS NULL THEN 0 ELSE 1 END AS bit) AS SiparisYetkilisiMi,
    byk.BayiYetkiliId,
    km.AktifMi
FROM dbo.KullaniciMagazalari km
INNER JOIN dbo.Kullanicilar k ON k.KullaniciId = km.KullaniciId
LEFT JOIN dbo.BayiYetkilileri byk
    ON byk.KullaniciId = km.KullaniciId
   AND byk.MagazaId = km.MagazaId
   AND byk.YetkiTipi = N'SiparisYetkilisi'
   AND byk.AktifMi = 1
WHERE km.MagazaId = @MagazaId
ORDER BY
    CASE km.Gorev WHEN N'Supervisor' THEN 1 WHEN N'MagazaMuduru' THEN 2 ELSE 3 END,
    k.AdSoyad;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            members.Add(new StoreTeamMemberDto
            {
                KullaniciId = reader.GetInt("KullaniciId"),
                KullaniciMagazaId = reader.GetInt("KullaniciMagazaId"),
                KullaniciAdi = reader.GetText("KullaniciAdi"),
                AdSoyad = reader.GetText("AdSoyad"),
                Email = reader.GetText("Email"),
                Telefon = reader.GetText("Telefon"),
                ImageUrl = reader.GetText("ImageUrl"),
                Rol = reader.GetText("Rol"),
                MagazaGorev = reader.GetText("Gorev"),
                MagazaGorevGorunenAd = reader.GetText("GorevGorunenAd"),
                SiparisYetkilisiMi = reader.GetBool("SiparisYetkilisiMi"),
                BayiYetkiliId = reader.GetNullableInt("BayiYetkiliId"),
                AktifMi = reader.GetBool("AktifMi")
            });
        }

        return members;
    }

    private async Task<StoreMembership> GetMembershipAsync(int storeId, int userStoreId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT km.KullaniciMagazaId, km.KullaniciId, km.MagazaId, km.Gorev, k.KullaniciAdi, ISNULL(k.Rol, N'') AS Rol
FROM dbo.KullaniciMagazalari km
INNER JOIN dbo.Kullanicilar k ON k.KullaniciId = km.KullaniciId
WHERE km.KullaniciMagazaId = @KullaniciMagazaId
  AND km.MagazaId = @MagazaId
  AND km.AktifMi = 1;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@KullaniciMagazaId", SqlDbType.Int).Value = userStoreId;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Personel aktif mağazada bulunamadı.");

        return new StoreMembership(reader.GetInt("KullaniciMagazaId"), reader.GetInt("KullaniciId"), reader.GetInt("MagazaId"), reader.GetText("Gorev"), reader.GetText("KullaniciAdi"), reader.GetText("Rol"));
    }

    private async Task<StoreMembership> GetMembershipByUserAsync(int storeId, int userId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT km.KullaniciMagazaId, km.KullaniciId, km.MagazaId, km.Gorev, k.KullaniciAdi, ISNULL(k.Rol, N'') AS Rol
FROM dbo.KullaniciMagazalari km
INNER JOIN dbo.Kullanicilar k ON k.KullaniciId = km.KullaniciId
WHERE km.KullaniciId = @KullaniciId
  AND km.MagazaId = @MagazaId
  AND km.AktifMi = 1;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Personel aktif mağazada bulunamadı.");

        return new StoreMembership(reader.GetInt("KullaniciMagazaId"), reader.GetInt("KullaniciId"), reader.GetInt("MagazaId"), reader.GetText("Gorev"), reader.GetText("KullaniciAdi"), reader.GetText("Rol"));
    }

    private async Task EnsureNotLastSupervisorAsync(int storeId, int userId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT COUNT(1)
FROM dbo.KullaniciMagazalari
WHERE MagazaId = @MagazaId
  AND Gorev = N'Supervisor'
  AND AktifMi = 1
  AND KullaniciId <> @KullaniciId;", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = userId;

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null || Convert.ToInt32(result) <= 0)
            throw new InvalidOperationException("Mağazada tek aktif supervisor varken bu supervisor'ın rolü düşürülemez veya mağaza bağlantısı kaldırılamaz.");
    }

    private async Task<int> GetCustomerIdAsync(int storeId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("SELECT CustomerId FROM dbo.CustomerStores WHERE CustomerStoreId = @MagazaId;", connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null || result == DBNull.Value)
            throw new InvalidOperationException("Aktif mağaza bulunamadı.");

        return Convert.ToInt32(result);
    }

    private async Task<int?> GetOrderAuthorityIdAsync(int storeId, int userId, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT TOP 1 BayiYetkiliId
FROM dbo.BayiYetkilileri
WHERE KullaniciId = @KullaniciId
  AND MagazaId = @MagazaId
  AND YetkiTipi = N'SiparisYetkilisi';", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    private static string NormalizeDuty(string duty)
    {
        return duty is "MagazaMuduru" or "Supervisor" or "Personel" ? duty : "Personel";
    }

    private static bool IsSystemAdmin(string role, string userName)
    {
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(userName, "admin", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record StoreMembership(int UserStoreId, int UserId, int StoreId, string Duty, string UserName, string Role);
}
