using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;

namespace eMarketing.Service.Repositories;

public sealed class StockRepository : IStockRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public StockRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public Task<IReadOnlyList<StockItemDto>> GetStocksAsync(StockFilterRequest filter, int? userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_MagazaStok_Listele", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, filter.MagazaId),
            SqlParameterFactory.TextParam("@Arama", 200, filter.Arama),
            SqlParameterFactory.Param("@SadeceStokta", SqlDbType.Bit, filter.SadeceStokta),
            SqlParameterFactory.Param("@SadeceKritik", SqlDbType.Bit, filter.SadeceKritik),
            SqlParameterFactory.Param("@SadeceAktif", SqlDbType.Bit, filter.SadeceAktif),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, userId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, reader => new StockItemDto
        {
            MagazaStokId = reader.GetInt("MagazaStokId"),
            MagazaId = reader.GetInt("MagazaId"),
            MusteriId = reader.GetInt("MusteriId"),
            MusteriAdi = reader.GetText("MusteriAdi"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            Sehir = reader.GetText("Sehir"),
            Ilce = reader.GetText("Ilce"),
            Telefon = reader.GetText("Telefon"),
            SorumluKisi = reader.GetText("SorumluKisi"),
            UrunId = reader.GetInt("UrunId"),
            UrunAdi = reader.GetText("UrunAdi"),
            Aciklama = reader.GetText("Aciklama"),
            Fiyat = reader.GetDecimal("Fiyat"),
            MerkezStok = reader.GetInt("MerkezStok"),
            GorselUrl = reader.GetText("GorselUrl"),
            KategoriId = reader.GetInt("KategoriId"),
            KategoriAdi = reader.GetText("KategoriAdi"),
            BayiStok = reader.GetInt("BayiStok"),
            MinimumStok = reader.GetInt("MinimumStok"),
            StokDurumu = reader.GetText("StokDurumu"),
            AktifMi = reader.GetBool("AktifMi"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi"),
            GuncellemeTarihi = reader.GetNullableDate("GuncellemeTarihi"),
            SonHareketTarihi = reader.GetNullableDate("SonHareketTarihi"),
            SonGirisTarihi = reader.GetNullableDate("SonGirisTarihi"),
            SonCikisTarihi = reader.GetNullableDate("SonCikisTarihi")
        }, cancellationToken);
    }

    public async Task<StockSummaryDto> GetSummaryAsync(int? storeId, bool allStores, int? userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        StockSummaryDto? summary = await _sqlExecutor.QuerySingleAsync("sp_MagazaStok_Ozet_Getir", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@TumMagazalar", SqlDbType.Bit, allStores),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, userId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, reader => new StockSummaryDto
        {
            ToplamStokKarti = reader.GetInt("ToplamStokKarti"),
            StokluMagazaSayisi = reader.GetInt("StokluMagazaSayisi"),
            StokluUrunSayisi = reader.GetInt("StokluUrunSayisi"),
            ToplamBayiStok = reader.GetInt("ToplamBayiStok"),
            KritikStokKarti = reader.GetInt("KritikStokKarti"),
            TukenmisStokKarti = reader.GetInt("TukenmisStokKarti")
        }, cancellationToken);

        return summary ?? new StockSummaryDto();
    }

    public Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(int storeId, int productId, int count, int? userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_MagazaStok_Hareket_Listele", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@ProductId", SqlDbType.Int, productId),
            SqlParameterFactory.Param("@KayitSayisi", SqlDbType.Int, count <= 0 ? 25 : count),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, userId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, reader => new StockMovementDto
        {
            MagazaStokHareketId = reader.GetInt("MagazaStokHareketId"),
            MagazaId = reader.GetInt("MagazaId"),
            UrunId = reader.GetInt("UrunId"),
            HareketTipi = reader.GetText("HareketTipi"),
            HareketYonu = reader.GetText("HareketYonu"),
            HareketAciklama = reader.GetText("HareketAciklama"),
            Miktar = reader.GetInt("Miktar"),
            OncekiStok = reader.GetInt("OncekiStok"),
            SonrakiStok = reader.GetInt("SonrakiStok"),
            KaynakSiparisId = reader.GetNullableInt("KaynakSiparisId"),
            SiparisNo = reader.GetText("SiparisNo"),
            Aciklama = reader.GetText("Aciklama"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi")
        }, cancellationToken);
    }

    public Task UpdateMinimumAsync(int storeStockId, int minimumStock, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_MagazaStok_MinimumGuncelle", new[]
        {
            SqlParameterFactory.Param("@MagazaStokId", SqlDbType.Int, storeStockId),
            SqlParameterFactory.Param("@MinimumStok", SqlDbType.Int, minimumStock)
        }, cancellationToken);
    }

    public Task ProcessMovementAsync(StockOperationRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_MagazaStok_Hareket_Isle", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, request.MagazaId),
            SqlParameterFactory.Param("@ProductId", SqlDbType.Int, request.UrunId),
            SqlParameterFactory.NullableTextParam("@HareketTipi", 50, request.HareketTipi),
            SqlParameterFactory.Param("@Miktar", SqlDbType.Int, request.Miktar),
            SqlParameterFactory.Param("@KaynakSiparisId", SqlDbType.Int, null),
            SqlParameterFactory.Param("@KaynakSiparisKalemId", SqlDbType.Int, null),
            SqlParameterFactory.NullableTextParam("@Aciklama", 500, request.Aciklama),
            SqlParameterFactory.Param("@MinimumStok", SqlDbType.Int, request.MinimumStok)
        }, cancellationToken);
    }

    public Task ProcessCentralStockAsync(CentralStockOperationRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_MerkezStok_Artir", new[]
        {
            SqlParameterFactory.Param("@ProductId", SqlDbType.Int, request.UrunId),
            SqlParameterFactory.Param("@Miktar", SqlDbType.Int, request.Miktar),
            SqlParameterFactory.NullableTextParam("@Aciklama", 500, request.Aciklama)
        }, cancellationToken);
    }
}
