using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;

namespace eMarketing.Service.Repositories;

public sealed class BayiYetkiliRepository : IBayiYetkiliRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public BayiYetkiliRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public Task<IReadOnlyList<BayiYetkiliDto>> GetAsync(BayiYetkiliFilterRequest filter, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_BayiYetkili_Listele", new[]
        {
            SqlParameterFactory.TextParam("@Arama", 200, filter.Arama),
            SqlParameterFactory.Param("@Durum", SqlDbType.Int, filter.Durum),
            SqlParameterFactory.Param("@BayiId", SqlDbType.Int, filter.BayiId),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, filter.MagazaId),
            SqlParameterFactory.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, viewerUserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, MapBayiYetkili, cancellationToken);
    }

    public Task<BayiYetkiliDto?> GetByIdAsync(int id, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QuerySingleAsync("sp_BayiYetkili_Getir", new[]
        {
            SqlParameterFactory.Param("@BayiYetkiliId", SqlDbType.Int, id),
            SqlParameterFactory.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, viewerUserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, MapBayiYetkili, cancellationToken);
    }

    public Task<int> SaveAsync(BayiYetkiliSaveRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteScalarIntAsync("sp_BayiYetkili_Kaydet", new[]
        {
            SqlParameterFactory.Param("@BayiYetkiliId", SqlDbType.Int, request.BayiYetkiliId),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, request.KullaniciId),
            SqlParameterFactory.Param("@BayiId", SqlDbType.Int, request.BayiId),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, request.MagazaId),
            SqlParameterFactory.NullableTextParam("@YetkiTipi", 50, request.YetkiTipi),
            SqlParameterFactory.NullableTextParam("@Notlar", 500, request.Notlar),
            SqlParameterFactory.Param("@AktifMi", SqlDbType.Bit, request.AktifMi)
        }, cancellationToken);
    }

    public Task SetStatusAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_BayiYetkili_DurumGuncelle", new[]
        {
            SqlParameterFactory.Param("@BayiYetkiliId", SqlDbType.Int, id),
            SqlParameterFactory.Param("@AktifMi", SqlDbType.Bit, active)
        }, cancellationToken);
    }

    public Task<IReadOnlyList<SiparisYetkilisiDto>> GetOrderAuthoritiesAsync(int storeId, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_Magaza_SiparisYetkilileri_Listele", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, viewerUserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, reader => new SiparisYetkilisiDto
        {
            BayiYetkiliId = reader.GetInt("BayiYetkiliId"),
            KullaniciId = reader.GetInt("KullaniciId"),
            KullaniciAdi = reader.GetText("KullaniciAdi"),
            AdSoyad = reader.GetText("AdSoyad"),
            Rol = reader.GetText("Rol"),
            RolGorunenAd = reader.GetText("RolGorunenAd"),
            Telefon = reader.GetText("Telefon"),
            Email = reader.GetText("Email"),
            ImageUrl = reader.GetText("ImageUrl"),
            YetkiTipi = reader.GetText("YetkiTipi"),
            YetkiTipiGorunenAd = reader.GetText("YetkiTipiGorunenAd")
        }, cancellationToken);
    }

    private static BayiYetkiliDto MapBayiYetkili(Microsoft.Data.SqlClient.SqlDataReader reader)
    {
        return new BayiYetkiliDto
        {
            BayiYetkiliId = reader.GetInt("BayiYetkiliId"),
            KullaniciId = reader.GetInt("KullaniciId"),
            KullaniciAdi = reader.GetText("KullaniciAdi"),
            AdSoyad = reader.GetText("AdSoyad"),
            Rol = reader.GetText("Rol"),
            RolGorunenAd = reader.GetText("RolGorunenAd"),
            Telefon = reader.GetText("Telefon"),
            Email = reader.GetText("Email"),
            ImageUrl = reader.GetText("ImageUrl"),
            BayiId = reader.GetInt("BayiId"),
            BayiAdi = reader.GetText("BayiAdi"),
            MagazaId = reader.GetInt("MagazaId"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            Sehir = reader.GetText("Sehir"),
            Ilce = reader.GetText("Ilce"),
            YetkiTipi = reader.GetText("YetkiTipi"),
            YetkiTipiGorunenAd = reader.GetText("YetkiTipiGorunenAd"),
            Notlar = reader.GetText("Notlar"),
            AktifMi = reader.GetBool("AktifMi"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi"),
            GuncellemeTarihi = reader.GetNullableDate("GuncellemeTarihi"),
            SiparisSayisi = reader.GetInt("SiparisSayisi"),
            SonSiparisTarihi = reader.GetNullableDate("SonSiparisTarihi")
        };
    }
}
