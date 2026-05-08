using System.Data;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(string? search, int status, int categoryId, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<int> CreateProductAsync(ProductSaveRequest request, CancellationToken cancellationToken = default);
    Task UpdateProductAsync(int productId, ProductSaveRequest request, CancellationToken cancellationToken = default);
    Task SetProductStatusAsync(int productId, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default);
}

public sealed class ProductService : IProductService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ProductService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(string? search, int status, int categoryId, CancellationToken cancellationToken = default)
    {
        var products = new List<ProductDto>();

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Urun_Listele", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@Arama", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim();
        command.Parameters.Add("@Durum", SqlDbType.Int).Value = status;
        command.Parameters.Add("@KategoriId", SqlDbType.Int).Value = categoryId;

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new ProductDto
            {
                UrunId = reader.GetInt("UrunId"),
                KategoriId = reader.GetInt("KategoriId"),
                KategoriAdi = reader.GetText("KategoriAdi"),
                UrunAdi = reader.GetText("UrunAdi"),
                Aciklama = reader.GetText("Aciklama"),
                Fiyat = reader.GetDecimal("Fiyat"),
                Stok = reader.GetInt("Stok"),
                GorselUrl = reader.GetText("GorselUrl"),
                AktifMi = reader.GetBool("AktifMi"),
                StokDurumu = reader.GetText("StokDurumu"),
                OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi")
            });
        }

        return products;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Urun_Getir", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@UrunId", SqlDbType.Int).Value = productId;

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapProduct(reader);
    }

    public async Task<int> CreateProductAsync(ProductSaveRequest request, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Urun_Ekle", connection);
        ConfigureSaveCommand(command, request);

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null ? 0 : Convert.ToInt32(result);
    }

    public async Task UpdateProductAsync(int productId, ProductSaveRequest request, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Urun_Guncelle", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@UrunId", SqlDbType.Int).Value = productId;
        AddSaveParameters(command, request);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetProductStatusAsync(int productId, bool isActive, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Urun_DurumGuncelle", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@UrunId", SqlDbType.Int).Value = productId;
        command.Parameters.Add("@AktifMi", SqlDbType.Bit).Value = isActive;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Urun_Sil", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@UrunId", SqlDbType.Int).Value = productId;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void ConfigureSaveCommand(SqlCommand command, ProductSaveRequest request)
    {
        command.CommandType = CommandType.StoredProcedure;
        AddSaveParameters(command, request);
    }

    private static void AddSaveParameters(SqlCommand command, ProductSaveRequest request)
    {
        command.Parameters.Add("@KategoriId", SqlDbType.Int).Value = request.KategoriId;
        command.Parameters.Add("@UrunAdi", SqlDbType.NVarChar, 200).Value = request.UrunAdi.Trim();
        command.Parameters.Add("@Aciklama", SqlDbType.NVarChar).Value = string.IsNullOrWhiteSpace(request.Aciklama) ? DBNull.Value : request.Aciklama.Trim();

        SqlParameter price = command.Parameters.Add("@Fiyat", SqlDbType.Decimal);
        price.Precision = 18;
        price.Scale = 2;
        price.Value = request.Fiyat;

        command.Parameters.Add("@Stok", SqlDbType.Int).Value = request.Stok;
        command.Parameters.Add("@GorselUrl", SqlDbType.NVarChar, 500).Value = string.IsNullOrWhiteSpace(request.GorselUrl) ? DBNull.Value : request.GorselUrl.Trim();
        command.Parameters.Add("@AktifMi", SqlDbType.Bit).Value = request.AktifMi;
    }

    private static ProductDto MapProduct(SqlDataReader reader)
    {
        return new ProductDto
        {
            UrunId = reader.GetInt("UrunId"),
            KategoriId = reader.GetInt("KategoriId"),
            KategoriAdi = reader.GetText("KategoriAdi"),
            UrunAdi = reader.GetText("UrunAdi"),
            Aciklama = reader.GetText("Aciklama"),
            Fiyat = reader.GetDecimal("Fiyat"),
            Stok = reader.GetInt("Stok"),
            GorselUrl = reader.GetText("GorselUrl"),
            AktifMi = reader.GetBool("AktifMi"),
            StokDurumu = reader.GetText("StokDurumu"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi")
        };
    }
}
