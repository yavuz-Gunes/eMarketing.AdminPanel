using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Security;

namespace eMarketing.Service.Services;

public interface ICampaignService
{
    Task<IReadOnlyList<DashboardCampaignDto>> GetActiveCampaignsAsync(int? storeId, CancellationToken cancellationToken = default);
    Task<CampaignDto?> GetCampaignDetailAsync(int campaignId, int? storeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CampaignDto>> GetCampaignsAsync(string search, int status, CancellationToken cancellationToken = default);
    Task<CampaignDto?> GetCampaignByIdAsync(int campaignId, CancellationToken cancellationToken = default);
    Task<int> CreateCampaignAsync(CampaignSaveRequest request, CancellationToken cancellationToken = default);
    Task UpdateCampaignAsync(int campaignId, CampaignSaveRequest request, CancellationToken cancellationToken = default);
    Task SetCampaignStatusAsync(int campaignId, bool active, CancellationToken cancellationToken = default);
    Task DeleteCampaignAsync(int campaignId, CancellationToken cancellationToken = default);
}

public sealed class CampaignService : ICampaignService
{
    private readonly IStoreAuthorizationService _storeAuthorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISqlExecutor _sqlExecutor;
    private readonly INotificationService _notificationService;

    public CampaignService(
        IStoreAuthorizationService storeAuthorizationService,
        ICurrentUserService currentUserService,
        ISqlExecutor sqlExecutor,
        INotificationService notificationService)
    {
        _storeAuthorizationService = storeAuthorizationService;
        _currentUserService = currentUserService;
        _sqlExecutor = sqlExecutor;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<DashboardCampaignDto>> GetActiveCampaignsAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        CurrentUser currentUser = _currentUserService.CurrentUser;

        return await _sqlExecutor.QueryAsync("sp_Kampanya_Aktif_Listele", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores)
        }, reader => new DashboardCampaignDto
        {
            KampanyaId = reader.GetInt("KampanyaId"),
            Baslik = reader.GetText("Baslik"),
            Aciklama = reader.GetText("Aciklama"),
            GorselUrl = reader.GetText("GorselUrl"),
            CtaMetni = reader.GetText("CtaMetni"),
            HedefUrl = reader.GetText("HedefUrl"),
            Oncelik = reader.GetInt("Oncelik"),
            BaslangicTarihi = reader.GetNullableDate("BaslangicTarihi"),
            BitisTarihi = reader.GetNullableDate("BitisTarihi")
        }, cancellationToken);
    }

    public async Task<CampaignDto?> GetCampaignDetailAsync(int campaignId, int? storeId, CancellationToken cancellationToken = default)
    {
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        CurrentUser currentUser = _currentUserService.CurrentUser;

        return await _sqlExecutor.QuerySingleAsync("sp_Kampanya_Detay_Getir", new[]
        {
            SqlParameterFactory.Param("@KampanyaId", SqlDbType.Int, campaignId),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, currentUser.UserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, currentUser.CanSeeAllStores)
        }, MapCampaign, cancellationToken);
    }

    public Task<IReadOnlyList<CampaignDto>> GetCampaignsAsync(string search, int status, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_Kampanya_Admin_Listele", new[]
        {
            SqlParameterFactory.TextParam("@Arama", 200, search),
            SqlParameterFactory.Param("@Durum", SqlDbType.Int, status)
        }, MapCampaign, cancellationToken);
    }

    public Task<CampaignDto?> GetCampaignByIdAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QuerySingleAsync("sp_Kampanya_Admin_Getir", new[]
        {
            SqlParameterFactory.Param("@KampanyaId", SqlDbType.Int, campaignId)
        }, MapCampaign, cancellationToken);
    }

    public async Task<int> CreateCampaignAsync(CampaignSaveRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        int campaignId = await _sqlExecutor.ExecuteScalarIntAsync("sp_Kampanya_Ekle", CampaignParameters(request), cancellationToken);
        await _notificationService.CreateCampaignChangedNotificationAsync(campaignId, cancellationToken);
        return campaignId;
    }

    public async Task UpdateCampaignAsync(int campaignId, CampaignSaveRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        await _sqlExecutor.ExecuteAsync("sp_Kampanya_Guncelle", WithId(campaignId, CampaignParameters(request)), cancellationToken);
        await _notificationService.CreateCampaignChangedNotificationAsync(campaignId, cancellationToken);
    }

    public async Task SetCampaignStatusAsync(int campaignId, bool active, CancellationToken cancellationToken = default)
    {
        await _sqlExecutor.ExecuteAsync("sp_Kampanya_DurumGuncelle", new[]
        {
            SqlParameterFactory.Param("@KampanyaId", SqlDbType.Int, campaignId),
            SqlParameterFactory.Param("@AktifMi", SqlDbType.Bit, active)
        }, cancellationToken);

        if (active)
            await _notificationService.CreateCampaignChangedNotificationAsync(campaignId, cancellationToken);
    }

    public Task DeleteCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_Kampanya_Sil", new[]
        {
            SqlParameterFactory.Param("@KampanyaId", SqlDbType.Int, campaignId)
        }, cancellationToken);
    }

    private static CampaignDto MapCampaign(Microsoft.Data.SqlClient.SqlDataReader reader)
    {
        return new CampaignDto
        {
            KampanyaId = reader.GetInt("KampanyaId"),
            Baslik = reader.GetText("Baslik"),
            Aciklama = reader.GetText("Aciklama"),
            DetayMetni = reader.GetText("DetayMetni"),
            KatilimSartlari = reader.GetText("KatilimSartlari"),
            AdminNotlari = reader.GetText("AdminNotlari"),
            GorselUrl = reader.GetText("GorselUrl"),
            CtaMetni = reader.GetText("CtaMetni"),
            HedefUrl = reader.GetText("HedefUrl"),
            HedefUrunId = reader.GetNullableInt("HedefUrunId"),
            HedefKategoriId = reader.GetNullableInt("HedefKategoriId"),
            BaslangicTarihi = reader.GetNullableDate("BaslangicTarihi"),
            BitisTarihi = reader.GetNullableDate("BitisTarihi"),
            Oncelik = reader.GetInt("Oncelik"),
            AktifMi = reader.GetBool("AktifMi"),
            GlobalMi = reader.GetBool("GlobalMi"),
            Kapsam = reader.GetText("Kapsam"),
            BayiIds = reader.GetText("BayiIds"),
            BayiAdlari = reader.GetText("BayiAdlari"),
            MagazaIds = reader.GetText("MagazaIds"),
            MagazaAdlari = reader.GetText("MagazaAdlari"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi"),
            GuncellemeTarihi = reader.GetNullableDate("GuncellemeTarihi")
        };
    }

    private static void Validate(CampaignSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Baslik))
            throw new ArgumentException("Kampanya başlığı zorunludur.", nameof(request.Baslik));

        if (request.BitisTarihi.HasValue && request.BaslangicTarihi.HasValue && request.BitisTarihi.Value < request.BaslangicTarihi.Value)
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden önce olamaz.", nameof(request.BitisTarihi));
    }

    private static Microsoft.Data.SqlClient.SqlParameter[] CampaignParameters(CampaignSaveRequest request)
    {
        return
        [
            SqlParameterFactory.TextParam("@Baslik", 200, request.Baslik),
            SqlParameterFactory.NullableTextParam("@Aciklama", 500, request.Aciklama),
            SqlParameterFactory.NullableTextParam("@DetayMetni", -1, request.DetayMetni),
            SqlParameterFactory.NullableTextParam("@KatilimSartlari", -1, request.KatilimSartlari),
            SqlParameterFactory.NullableTextParam("@AdminNotlari", -1, request.AdminNotlari),
            SqlParameterFactory.NullableTextParam("@GorselUrl", 500, request.GorselUrl),
            SqlParameterFactory.NullableTextParam("@CtaMetni", 80, request.CtaMetni),
            SqlParameterFactory.NullableTextParam("@HedefUrl", 500, request.HedefUrl),
            SqlParameterFactory.Param("@HedefUrunId", SqlDbType.Int, request.HedefUrunId),
            SqlParameterFactory.Param("@HedefKategoriId", SqlDbType.Int, request.HedefKategoriId),
            SqlParameterFactory.Param("@BaslangicTarihi", SqlDbType.DateTime2, request.BaslangicTarihi),
            SqlParameterFactory.Param("@BitisTarihi", SqlDbType.DateTime2, request.BitisTarihi),
            SqlParameterFactory.Param("@Oncelik", SqlDbType.Int, request.Oncelik),
            SqlParameterFactory.Param("@AktifMi", SqlDbType.Bit, request.AktifMi),
            SqlParameterFactory.Param("@GlobalMi", SqlDbType.Bit, request.GlobalMi),
            SqlParameterFactory.NullableTextParam("@BayiIds", -1, JoinIds(request.BayiIds)),
            SqlParameterFactory.NullableTextParam("@MagazaIds", -1, JoinIds(request.MagazaIds))
        ];
    }

    private static Microsoft.Data.SqlClient.SqlParameter[] WithId(int campaignId, Microsoft.Data.SqlClient.SqlParameter[] parameters)
    {
        return [SqlParameterFactory.Param("@KampanyaId", SqlDbType.Int, campaignId), .. parameters];
    }

    private static string JoinIds(IReadOnlyList<int> ids)
    {
        return ids == null || ids.Count == 0
            ? string.Empty
            : string.Join(",", ids.Where(id => id > 0).Distinct());
    }
}
