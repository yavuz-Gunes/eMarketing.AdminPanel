using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;

namespace eMarketing.Service.Repositories;

public sealed class StoreRepository : IStoreRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public StoreRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public Task<IReadOnlyList<StoreDto>> GetStoresAsync(string search, bool onlyActive, int? userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_Magaza_Secim_Listele", StoreParameters(search, onlyActive, userId, isAdmin), MapStore, cancellationToken);
    }

    public Task<StoreDto?> GetStoreAsync(int storeId, int? userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QuerySingleAsync("sp_Magaza_Secim_Getir", new[]
        {
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, userId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, MapStore, cancellationToken);
    }

    private static Microsoft.Data.SqlClient.SqlParameter[] StoreParameters(string search, bool onlyActive, int? userId, bool isAdmin)
    {
        return new[]
        {
            SqlParameterFactory.TextParam("@Arama", 200, search),
            SqlParameterFactory.Param("@SadeceAktif", SqlDbType.Bit, onlyActive),
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, userId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        };
    }

    private static StoreDto MapStore(Microsoft.Data.SqlClient.SqlDataReader reader)
    {
        return new StoreDto
        {
            MusteriId = reader.GetInt("MusteriId"),
            MagazaId = reader.GetInt("MagazaId"),
            MusteriAdi = reader.GetText("MusteriAdi"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            Sehir = reader.GetText("Sehir"),
            Ilce = reader.GetText("Ilce"),
            Telefon = reader.GetText("Telefon"),
            SorumluKullaniciId = reader.HasColumn("SorumluKullaniciId") ? reader.GetNullableInt("SorumluKullaniciId") : null,
            SorumluKisi = reader.GetText("SorumluKisi"),
            MagazaMuduru = reader.HasColumn("MagazaMuduru") ? reader.GetText("MagazaMuduru") : string.Empty,
            Supervisor = reader.HasColumn("Supervisor") ? reader.GetText("Supervisor") : string.Empty,
            PersonelSayisi = reader.HasColumn("PersonelSayisi") ? reader.GetInt("PersonelSayisi") : 0,
            SiparisYetkilisiSayisi = reader.HasColumn("SiparisYetkilisiSayisi") ? reader.GetInt("SiparisYetkilisiSayisi") : 0,
            MusteriTipi = reader.GetText("MusteriTipi"),
            MusteriAktifMi = reader.GetBool("MusteriAktifMi"),
            MagazaAktifMi = reader.GetBool("MagazaAktifMi"),
            SiparisSayisi = reader.GetInt("SiparisSayisi"),
            ToplamCiro = reader.GetDecimal("ToplamCiro"),
            SonSiparisTarihi = reader.GetNullableDate("SonSiparisTarihi")
        };
    }
}
