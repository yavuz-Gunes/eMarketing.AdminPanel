using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Security;

namespace eMarketing.Service.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(int? storeId, bool unreadOnly, int limit, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int? storeId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(int? storeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDto>> GetAdminNotificationsAsync(string search, string type, int status, int? storeId, CancellationToken cancellationToken = default);
    Task<int> CreateNotificationAsync(NotificationSaveRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(int notificationId, bool active, CancellationToken cancellationToken = default);
    Task DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken = default);
    Task CreateCampaignChangedNotificationAsync(int campaignId, CancellationToken cancellationToken = default);
    Task CreateCriticalStockNotificationsAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
    Task GenerateSystemNotificationsAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default);
}

public sealed class NotificationService : INotificationService
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Kampanya",
        "KritikStok",
        "OzelMesaj",
        "Sistem"
    };

    private readonly ISqlExecutor _sqlExecutor;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStoreAuthorizationService _storeAuthorizationService;

    public NotificationService(
        ISqlExecutor sqlExecutor,
        ICurrentUserService currentUserService,
        IStoreAuthorizationService storeAuthorizationService)
    {
        _sqlExecutor = sqlExecutor;
        _currentUserService = currentUserService;
        _storeAuthorizationService = storeAuthorizationService;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(int? storeId, bool unreadOnly, int limit, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        await GenerateSystemNotificationsCoreAsync(storeId, false, currentUser, cancellationToken);

        return await _sqlExecutor.QueryAsync("sp_Bildirim_Listele", new[]
        {
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@SadeceOkunmamis", SqlDbType.Bit, unreadOnly),
            SqlParameterFactory.Param("@Limit", SqlDbType.Int, Math.Clamp(limit, 1, 100))
        }, MapNotification, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        await GenerateSystemNotificationsCoreAsync(storeId, false, currentUser, cancellationToken);

        return await _sqlExecutor.ExecuteScalarIntAsync("sp_Bildirim_OkunmamisSayisi_Getir", new[]
        {
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId)
        }, cancellationToken);
    }

    public Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        return _sqlExecutor.ExecuteAsync("sp_Bildirim_Okundu_Isaretle", new[]
        {
            SqlParameterFactory.Param("@BildirimId", SqlDbType.Int, notificationId),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId)
        }, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        await _sqlExecutor.ExecuteAsync("sp_Bildirim_TumunuOkundu_Isaretle", new[]
        {
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId)
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetAdminNotificationsAsync(string search, string type, int status, int? storeId, CancellationToken cancellationToken = default)
    {
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        return await _sqlExecutor.QueryAsync("sp_Bildirim_Admin_Listele", new[]
        {
            SqlParameterFactory.TextParam("@Arama", 200, search),
            SqlParameterFactory.TextParam("@Tip", 30, type),
            SqlParameterFactory.Param("@Durum", SqlDbType.Int, status),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId)
        }, MapNotification, cancellationToken);
    }

    public async Task<int> CreateNotificationAsync(NotificationSaveRequest request, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        Validate(request);

        if (!currentUser.CanSeeAllStores)
        {
            foreach (int storeId in request.MagazaIds.Where(id => id > 0).Distinct())
                await _storeAuthorizationService.EnsureStoreAccessAsync(storeId, cancellationToken);
        }

        return await _sqlExecutor.ExecuteScalarIntAsync("sp_Bildirim_Ekle", BuildSaveParameters(request, currentUser.UserId), cancellationToken);
    }

    public Task SetStatusAsync(int notificationId, bool active, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_Bildirim_DurumGuncelle", new[]
        {
            SqlParameterFactory.Param("@BildirimId", SqlDbType.Int, notificationId),
            SqlParameterFactory.Param("@AktifMi", SqlDbType.Bit, active)
        }, cancellationToken);
    }

    public Task DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_Bildirim_Sil", new[]
        {
            SqlParameterFactory.Param("@BildirimId", SqlDbType.Int, notificationId)
        }, cancellationToken);
    }

    public Task CreateCampaignChangedNotificationAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        return _sqlExecutor.ExecuteAsync("sp_Bildirim_KampanyaDegisiklik_Uret", new[]
        {
            SqlParameterFactory.Param("@KampanyaId", SqlDbType.Int, campaignId),
            SqlParameterFactory.Param("@OlusturanKullaniciId", SqlDbType.Int, currentUser.UserId)
        }, cancellationToken);
    }

    public async Task CreateCriticalStockNotificationsAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        await _sqlExecutor.ExecuteAsync("sp_Bildirim_KritikStokDegisiklik_Uret", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@TumMagazalar", SqlDbType.Bit, allStores),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores)
        }, cancellationToken);
    }

    public async Task GenerateSystemNotificationsAsync(int? storeId, bool allStores, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = RequireUser();
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        await GenerateSystemNotificationsCoreAsync(storeId, allStores, currentUser, cancellationToken);
    }

    private async Task GenerateSystemNotificationsCoreAsync(int? storeId, bool allStores, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        await _sqlExecutor.ExecuteAsync("sp_Bildirim_GunlukKampanyaOzet_Uret", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@TumMagazalar", SqlDbType.Bit, allStores),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores)
        }, cancellationToken);

        await _sqlExecutor.ExecuteAsync("sp_Bildirim_KritikStokDegisiklik_Uret", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@TumMagazalar", SqlDbType.Bit, allStores),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores)
        }, cancellationToken);
    }

    private CurrentUser RequireUser()
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (!currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");

        return currentUser;
    }

    private static void Validate(NotificationSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Baslik))
            throw new ArgumentException("Bildirim başlığı zorunludur.", nameof(request.Baslik));

        if (!ValidTypes.Contains(request.Tip))
            throw new ArgumentException("Geçersiz bildirim tipi.", nameof(request.Tip));

        if (!IsValidTarget(request.HedefTipi, request.HedefId))
            throw new ArgumentException("Geçersiz bildirim hedefi.", nameof(request.HedefTipi));

        if (!request.GlobalMi && (request.MagazaIds == null || request.MagazaIds.All(id => id <= 0)))
            throw new ArgumentException("Global olmayan bildirim için en az bir mağaza seçilmelidir.", nameof(request.MagazaIds));

        if (request.BitisTarihi.HasValue && request.BaslangicTarihi.HasValue && request.BitisTarihi.Value < request.BaslangicTarihi.Value)
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden önce olamaz.", nameof(request.BitisTarihi));
    }

    private static Microsoft.Data.SqlClient.SqlParameter[] BuildSaveParameters(NotificationSaveRequest request, int? userId)
    {
        return
        [
            SqlParameterFactory.TextParam("@Baslik", 200, request.Baslik),
            SqlParameterFactory.NullableTextParam("@Mesaj", -1, request.Mesaj),
            SqlParameterFactory.TextParam("@Tip", 30, request.Tip),
            SqlParameterFactory.NullableTextParam("@HedefUrl", 500, BuildTargetUrl(request)),
            SqlParameterFactory.TextParam("@HedefTipi", 30, NormalizeTargetType(request.HedefTipi)),
            SqlParameterFactory.Param("@HedefId", SqlDbType.Int, request.HedefId),
            SqlParameterFactory.Param("@KampanyaId", SqlDbType.Int, null),
            SqlParameterFactory.NullableTextParam("@KaynakAnahtari", 200, null),
            SqlParameterFactory.Param("@GlobalMi", SqlDbType.Bit, request.GlobalMi),
            SqlParameterFactory.Param("@AktifMi", SqlDbType.Bit, request.AktifMi),
            SqlParameterFactory.Param("@BaslangicTarihi", SqlDbType.DateTime2, request.BaslangicTarihi),
            SqlParameterFactory.Param("@BitisTarihi", SqlDbType.DateTime2, request.BitisTarihi),
            SqlParameterFactory.Param("@OlusturanKullaniciId", SqlDbType.Int, userId),
            SqlParameterFactory.NullableTextParam("@MagazaIds", -1, JoinIds(request.MagazaIds))
        ];
    }

    private static NotificationDto MapNotification(Microsoft.Data.SqlClient.SqlDataReader reader)
    {
        return new NotificationDto
        {
            BildirimId = reader.GetInt("BildirimId"),
            Baslik = reader.GetText("Baslik"),
            Mesaj = reader.GetText("Mesaj"),
            Tip = reader.GetText("Tip"),
            HedefUrl = reader.GetText("HedefUrl"),
            HedefTipi = reader.HasColumn("HedefTipi") ? reader.GetText("HedefTipi") : "Yok",
            HedefId = reader.HasColumn("HedefId") ? reader.GetNullableInt("HedefId") : null,
            KampanyaId = reader.GetNullableInt("KampanyaId"),
            GlobalMi = reader.GetBool("GlobalMi"),
            AktifMi = reader.GetBool("AktifMi"),
            BaslangicTarihi = reader.GetNullableDate("BaslangicTarihi"),
            BitisTarihi = reader.GetNullableDate("BitisTarihi"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi"),
            OkunduMu = reader.GetBool("OkunduMu"),
            OkunmaTarihi = reader.GetNullableDate("OkunmaTarihi"),
            MagazaIds = reader.HasColumn("MagazaIds") ? reader.GetText("MagazaIds") : string.Empty,
            MagazaAdlari = reader.HasColumn("MagazaAdlari") ? reader.GetText("MagazaAdlari") : string.Empty
        };
    }

    private static string JoinIds(IReadOnlyList<int> ids)
    {
        return ids == null || ids.Count == 0
            ? string.Empty
            : string.Join(",", ids.Where(id => id > 0).Distinct());
    }

    private static bool IsValidTarget(string targetType, int? targetId)
    {
        return NormalizeTargetType(targetType) switch
        {
            "Yok" => true,
            "Sayfa" => true,
            "Urun" => targetId.HasValue && targetId.Value > 0,
            "Kategori" => targetId.HasValue && targetId.Value > 0,
            "Kampanya" => targetId.HasValue && targetId.Value > 0,
            _ => false
        };
    }

    private static string BuildTargetUrl(NotificationSaveRequest request)
    {
        return NormalizeTargetType(request.HedefTipi) switch
        {
            "Urun" => $"/products?urunId={request.HedefId}",
            "Kategori" => $"/products?kategoriId={request.HedefId}",
            "Kampanya" => $"/campaigns/{request.HedefId}",
            "Sayfa" => NormalizePageTarget(request.HedefUrl),
            _ => string.Empty
        };
    }

    private static string NormalizeTargetType(string? targetType)
    {
        return targetType switch
        {
            "Ürün" => "Urun",
            "Urun" => "Urun",
            "Kategori" => "Kategori",
            "Kampanya" => "Kampanya",
            "Sayfa" => "Sayfa",
            _ => "Yok"
        };
    }

    private static string NormalizePageTarget(string? targetUrl)
    {
        return targetUrl switch
        {
            "/products" => "/products",
            "/orders" => "/orders",
            "/admin/campaigns" => "/admin/campaigns",
            "/notifications" => "/notifications",
            _ => "/notifications"
        };
    }
}
