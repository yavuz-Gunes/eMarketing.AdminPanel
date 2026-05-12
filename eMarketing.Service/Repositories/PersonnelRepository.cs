using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;

namespace eMarketing.Service.Repositories;

public sealed class PersonnelRepository : IPersonnelRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public PersonnelRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public Task<IReadOnlyList<PersonnelDto>> GetPersonnelAsync(PersonnelFilterRequest filter, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_Kullanici_Listele", new[]
        {
            SqlParameterFactory.TextParam("@Arama", 200, filter.Arama),
            SqlParameterFactory.Param("@SadeceAktif", SqlDbType.Bit, filter.SadeceAktif),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, filter.MagazaId),
            SqlParameterFactory.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, viewerUserId),
            SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin)
        }, reader => new PersonnelDto
        {
            KullaniciId = reader.GetInt("KullaniciId"),
            KullaniciAdi = reader.GetText("KullaniciAdi"),
            AdSoyad = reader.GetText("AdSoyad"),
            Rol = reader.GetText("Rol"),
            Telefon = reader.GetText("Telefon"),
            Email = reader.GetText("Email"),
            ImageUrl = reader.GetText("ImageUrl"),
            AktifMi = reader.GetBool("AktifMi"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi"),
            MagazaSayisi = reader.GetInt("MagazaSayisi"),
            AktifMagazaGorev = reader.HasColumn("AktifMagazaGorev") ? reader.GetText("AktifMagazaGorev") : string.Empty,
            AktifMagazaGorevGorunenAd = reader.HasColumn("AktifMagazaGorevGorunenAd") ? reader.GetText("AktifMagazaGorevGorunenAd") : string.Empty,
            SiparisYetkilisiMi = reader.HasColumn("SiparisYetkilisiMi") && reader.GetBool("SiparisYetkilisiMi"),
            SiparisYetkiliMagazaSayisi = reader.HasColumn("SiparisYetkiliMagazaSayisi") ? reader.GetInt("SiparisYetkiliMagazaSayisi") : 0
        }, cancellationToken);
    }

    public Task<int> SavePersonnelAsync(CreatePersonnelRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteScalarIntAsync("sp_Kullanici_Kaydet", new[]
        {
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, request.KullaniciId),
            SqlParameterFactory.NullableTextParam("@KullaniciAdi", 100, request.KullaniciAdi),
            SqlParameterFactory.NullableTextParam("@Sifre", 100, request.Sifre),
            SqlParameterFactory.NullableTextParam("@AdSoyad", 150, request.AdSoyad),
            SqlParameterFactory.NullableTextParam("@Rol", 50, request.Rol),
            SqlParameterFactory.NullableTextParam("@Telefon", 60, request.Telefon),
            SqlParameterFactory.NullableTextParam("@Email", 400, request.Email),
            SqlParameterFactory.NullableTextParam("@ImageUrl", 500, request.ImageUrl),
            SqlParameterFactory.Param("@AktifMi", SqlDbType.Bit, request.AktifMi)
        }, cancellationToken);
    }

    public Task<IReadOnlyList<PersonnelStorePermissionDto>> GetStoresAsync(int userId, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_KullaniciMagaza_Listele", ViewerStoreParameters(userId, null, viewerUserId, isAdmin), MapStorePermission, cancellationToken);
    }

    public Task<IReadOnlyList<PersonnelStorePermissionDto>> GetAssignableStoresAsync(int userId, string search, int? viewerUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_KullaniciMagaza_AtanmamisMagaza_Listele", ViewerStoreParameters(userId, search, viewerUserId, isAdmin), MapStorePermission, cancellationToken);
    }

    public Task AssignStoreAsync(int userId, int storeId, string duty, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_KullaniciMagaza_Ata", new[]
        {
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, userId),
            SqlParameterFactory.Param("@MagazaId", SqlDbType.Int, storeId),
            SqlParameterFactory.NullableTextParam("@Gorev", 30, duty)
        }, cancellationToken);
    }

    public Task UpdateStoreDutyAsync(int userStoreId, string duty, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_KullaniciMagaza_GorevGuncelle", new[]
        {
            SqlParameterFactory.Param("@KullaniciMagazaId", SqlDbType.Int, userStoreId),
            SqlParameterFactory.NullableTextParam("@Gorev", 30, duty)
        }, cancellationToken);
    }

    public Task RemoveStoreAsync(int userStoreId, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_KullaniciMagaza_Kaldir", new[]
        {
            SqlParameterFactory.Param("@KullaniciMagazaId", SqlDbType.Int, userStoreId)
        }, cancellationToken);
    }

    private static IReadOnlyList<Microsoft.Data.SqlClient.SqlParameter> ViewerStoreParameters(int userId, string? search, int? viewerUserId, bool isAdmin)
    {
        var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>
        {
            SqlParameterFactory.Param("@KullaniciId", SqlDbType.Int, userId)
        };

        if (search != null)
            parameters.Add(SqlParameterFactory.TextParam("@Arama", 200, search));

        parameters.Add(SqlParameterFactory.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, viewerUserId));
        parameters.Add(SqlParameterFactory.Param("@AdminMi", SqlDbType.Bit, isAdmin));
        return parameters;
    }

    private static PersonnelStorePermissionDto MapStorePermission(Microsoft.Data.SqlClient.SqlDataReader reader)
    {
        return new PersonnelStorePermissionDto
        {
            KullaniciMagazaId = reader.HasColumn("KullaniciMagazaId") ? reader.GetNullableInt("KullaniciMagazaId") : null,
            KullaniciId = reader.HasColumn("KullaniciId") ? reader.GetNullableInt("KullaniciId") : null,
            MagazaId = reader.GetInt("MagazaId"),
            Gorev = reader.HasColumn("Gorev") ? reader.GetText("Gorev") : "Personel",
            GorevGorunenAd = reader.HasColumn("GorevGorunenAd") ? reader.GetText("GorevGorunenAd") : "Personel",
            MusteriId = reader.HasColumn("MusteriId") ? reader.GetInt("MusteriId") : 0,
            MusteriAdi = reader.GetText("MusteriAdi"),
            MagazaAdi = reader.GetText("MagazaAdi"),
            Sehir = reader.GetText("Sehir"),
            Ilce = reader.GetText("Ilce"),
            Telefon = reader.HasColumn("Telefon") ? reader.GetText("Telefon") : string.Empty,
            SorumluKisi = reader.HasColumn("SorumluKisi") ? reader.GetText("SorumluKisi") : string.Empty,
            SiparisSayisi = reader.HasColumn("SiparisSayisi") ? reader.GetInt("SiparisSayisi") : 0,
            ToplamCiro = reader.HasColumn("ToplamCiro") ? reader.GetDecimal("ToplamCiro") : 0M,
            BayiYetkiliId = reader.HasColumn("BayiYetkiliId") ? reader.GetNullableInt("BayiYetkiliId") : null,
            SiparisYetkilisiMi = reader.HasColumn("SiparisYetkilisiMi") && reader.GetBool("SiparisYetkilisiMi"),
            AktifMi = !reader.HasColumn("AktifMi") || reader.GetBool("AktifMi"),
            OlusturmaTarihi = reader.HasColumn("OlusturmaTarihi") ? reader.GetNullableDate("OlusturmaTarihi") : null
        };
    }
}
