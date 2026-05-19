using System.Data;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Repositories;

public sealed class DealerRepository : IDealerRepository
{
    private readonly ISqlExecutor _sqlExecutor;

    public DealerRepository(ISqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public Task<IReadOnlyList<DealerDetailDto>> GetDealersAsync(string search, int status, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_Musteri_Listele", new[]
        {
            SqlParameterFactory.TextParam("@Arama", 200, search),
            SqlParameterFactory.Param("@Durum", SqlDbType.Int, status)
        }, MapDealer, cancellationToken);
    }

    public Task<DealerDetailDto?> GetDealerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QuerySingleAsync("sp_Musteri_Getir", new[]
        {
            SqlParameterFactory.Param("@CustomerId", SqlDbType.Int, id)
        }, MapDealer, cancellationToken);
    }

    public Task<int> CreateDealerAsync(DealerSaveRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteScalarIntAsync("sp_Musteri_Ekle", DealerParameters(request), cancellationToken);
    }

    public Task UpdateDealerAsync(int id, DealerSaveRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_Musteri_Guncelle", WithId("@CustomerId", id, DealerParameters(request)), cancellationToken);
    }

    public Task SetDealerStatusAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_Musteri_DurumGuncelle", new[]
        {
            SqlParameterFactory.Param("@CustomerId", SqlDbType.Int, id),
            SqlParameterFactory.Param("@IsActive", SqlDbType.Bit, active)
        }, cancellationToken);
    }

    public Task<IReadOnlyList<DealerStoreDto>> GetStoresAsync(int customerId, int status, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QueryAsync("sp_MusteriMagaza_Listele", new[]
        {
            SqlParameterFactory.Param("@CustomerId", SqlDbType.Int, customerId),
            SqlParameterFactory.Param("@Durum", SqlDbType.Int, status)
        }, MapStore, cancellationToken);
    }

    public Task<DealerStoreDto?> GetStoreByIdAsync(int storeId, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.QuerySingleAsync("sp_MusteriMagaza_Getir", new[]
        {
            SqlParameterFactory.Param("@CustomerStoreId", SqlDbType.Int, storeId)
        }, MapStore, cancellationToken);
    }

    public Task<int> CreateStoreAsync(int customerId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteScalarIntAsync("sp_MusteriMagaza_Ekle", WithId("@CustomerId", customerId, StoreParameters(request)), cancellationToken);
    }

    public Task UpdateStoreAsync(int storeId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_MusteriMagaza_Guncelle", WithId("@CustomerStoreId", storeId, StoreParameters(request)), cancellationToken);
    }

    public Task SetStoreStatusAsync(int storeId, bool active, CancellationToken cancellationToken = default)
    {
        return _sqlExecutor.ExecuteAsync("sp_MusteriMagaza_DurumGuncelle", new[]
        {
            SqlParameterFactory.Param("@CustomerStoreId", SqlDbType.Int, storeId),
            SqlParameterFactory.Param("@IsActive", SqlDbType.Bit, active)
        }, cancellationToken);
    }

    private static SqlParameter[] DealerParameters(DealerSaveRequest request)
    {
        return new[]
        {
            SqlParameterFactory.TextParam("@FullName", 300, request.FullName),
            SqlParameterFactory.NullableTextParam("@CompanyName", 300, request.CompanyName),
            SqlParameterFactory.NullableTextParam("@AuthorizedPerson", 200, request.AuthorizedPerson),
            SqlParameterFactory.NullableTextParam("@Phone", 60, request.Phone),
            SqlParameterFactory.NullableTextParam("@Email", 400, request.Email),
            SqlParameterFactory.NullableTextParam("@TaxNumber", 50, request.TaxNumber),
            SqlParameterFactory.NullableTextParam("@TaxOffice", 100, request.TaxOffice),
            SqlParameterFactory.NullableTextParam("@Address", 500, request.Address),
            SqlParameterFactory.TextParam("@CustomerType", 50, request.CustomerType),
            SqlParameterFactory.Param("@IsActive", SqlDbType.Bit, request.IsActive)
        };
    }

    private static SqlParameter[] StoreParameters(DealerStoreSaveRequest request)
    {
        return new[]
        {
            SqlParameterFactory.TextParam("@StoreName", 300, request.StoreName),
            SqlParameterFactory.NullableTextParam("@City", 100, request.City),
            SqlParameterFactory.NullableTextParam("@District", 100, request.District),
            SqlParameterFactory.NullableTextParam("@Address", 500, request.Address),
            SqlParameterFactory.NullableTextParam("@Phone", 60, request.Phone),
            SqlParameterFactory.NullableTextParam("@ResponsiblePerson", 200, request.ResponsiblePerson),
            SqlParameterFactory.Param("@SorumluKullaniciId", SqlDbType.Int, request.SorumluKullaniciId),
            SqlParameterFactory.Param("@IsActive", SqlDbType.Bit, request.IsActive)
        };
    }

    private static SqlParameter[] WithId(string name, int id, IReadOnlyCollection<SqlParameter> parameters)
    {
        var result = new List<SqlParameter> { SqlParameterFactory.Param(name, SqlDbType.Int, id) };
        result.AddRange(parameters);
        return result.ToArray();
    }

    private static DealerDetailDto MapDealer(SqlDataReader reader)
    {
        return new DealerDetailDto
        {
            CustomerId = reader.GetInt("CustomerId"),
            FullName = reader.GetText("FullName"),
            CompanyName = reader.GetText("CompanyName"),
            AuthorizedPerson = reader.GetText("AuthorizedPerson"),
            Phone = reader.GetText("Phone"),
            Email = reader.GetText("Email"),
            TaxNumber = reader.GetText("TaxNumber"),
            TaxOffice = reader.GetText("TaxOffice"),
            Address = reader.GetText("Address"),
            CustomerType = reader.GetText("CustomerType"),
            IsActive = reader.GetBool("IsActive"),
            CreatedAt = reader.GetNullableDate("CreatedAt"),
            StoreCount = reader.HasColumn("StoreCount") ? reader.GetInt("StoreCount") : 0,
            OrderCount = reader.HasColumn("OrderCount") ? reader.GetInt("OrderCount") : 0,
            TotalRevenue = reader.HasColumn("TotalRevenue") ? reader.GetDecimal("TotalRevenue") : 0M
        };
    }

    private static DealerStoreDto MapStore(SqlDataReader reader)
    {
        return new DealerStoreDto
        {
            CustomerStoreId = reader.GetInt("CustomerStoreId"),
            CustomerId = reader.GetInt("CustomerId"),
            StoreName = reader.GetText("StoreName"),
            City = reader.GetText("City"),
            District = reader.GetText("District"),
            Address = reader.GetText("Address"),
            Phone = reader.GetText("Phone"),
            ResponsiblePerson = reader.GetText("ResponsiblePerson"),
            SorumluKullaniciId = reader.HasColumn("SorumluKullaniciId") ? reader.GetNullableInt("SorumluKullaniciId") : null,
            SorumluKisi = reader.HasColumn("SorumluKisi") ? reader.GetText("SorumluKisi") : string.Empty,
            IsActive = reader.GetBool("IsActive"),
            CreatedAt = reader.GetNullableDate("CreatedAt")
        };
    }
}
