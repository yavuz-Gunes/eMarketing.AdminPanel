using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public DashboardRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public Task<DashboardSummaryDto?> GetSummaryAsync(int? storeId, bool allStores, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QuerySingleAsync("sp_Dashboard_Ozet_Getir", DashboardParameters(storeId, allStores, viewerUserId, isAdmin), reader => new DashboardSummaryDto
        {
            ToplamUrun = reader.GetInt("ToplamUrun"),
            AktifUrun = reader.GetInt("AktifUrun"),
            KritikStok = reader.GetInt("KritikStok"),
            ToplamKategori = reader.GetInt("ToplamKategori"),
            AktifKategori = reader.GetInt("AktifKategori"),
            ToplamSiparis = reader.GetInt("ToplamSiparis"),
            HazirlaniyorSayisi = reader.GetInt("HazirlaniyorSayisi"),
            KargodaSayisi = reader.GetInt("KargodaSayisi"),
            TeslimEdildiSayisi = reader.GetInt("TeslimEdildiSayisi"),
            IptalSayisi = reader.GetInt("IptalSayisi"),
            BekleyenOdemeSayisi = reader.GetInt("BekleyenOdemeSayisi"),
            ToplamMusteri = reader.GetInt("ToplamMusteri"),
            AktifMagaza = reader.GetInt("AktifMagaza"),
            PersonelSayisi = reader.GetInt("PersonelSayisi"),
            ToplamCiro = reader.GetDecimal("ToplamCiro"),
            BugunkuCiro = reader.GetDecimal("BugunkuCiro"),
            AylikCiro = reader.GetDecimal("AylikCiro")
        }, cancellationToken);
    }

    public Task<IReadOnlyList<DashboardRecentOrderDto>> GetRecentOrdersAsync(int? storeId, bool allStores, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_Dashboard_SonSiparisler_Getir", DashboardParameters(storeId, allStores, viewerUserId, isAdmin), reader => new DashboardRecentOrderDto
        {
            SiparisId = reader.GetInt("SiparisId"),
            MusteriAdi = reader.GetText("MusteriAdi"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            YetkiliAdi = reader.GetText("YetkiliAdi"),
            UrunAdi = reader.GetText("UrunAdi"),
            Adet = reader.GetInt("Adet"),
            ToplamTutar = reader.GetDecimal("ToplamTutar"),
            SiparisDurumu = reader.GetText("SiparisDurumu"),
            SiparisTarihi = reader.GetNullableDate("SiparisTarihi"),
            OrderSource = reader.GetText("OrderSource"),
            OrderType = reader.GetText("OrderType")
        }, cancellationToken);
    }

    public Task<IReadOnlyList<DashboardCriticalStockDto>> GetCriticalStockAsync(int? storeId, bool allStores, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_Dashboard_KritikStok_Getir", DashboardParameters(storeId, allStores, viewerUserId, isAdmin), reader => new DashboardCriticalStockDto
        {
            UrunId = reader.GetInt("UrunId"),
            UrunAdi = reader.GetText("UrunAdi"),
            KategoriAdi = reader.GetText("KategoriAdi"),
            Stok = reader.GetInt("Stok"),
            Fiyat = reader.GetDecimal("Fiyat")
        }, cancellationToken);
    }

    private static SqlParameter[] DashboardParameters(int? storeId, bool allStores, int? viewerUserId, bool isAdmin)
    {
        return new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@TumMagazalar", SqlDbType.Bit, allStores),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, viewerUserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        };
    }
}
