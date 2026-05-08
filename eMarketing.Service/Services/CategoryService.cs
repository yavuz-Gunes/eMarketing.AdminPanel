using System.Data;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(string? search, int status, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<int> CreateCategoryAsync(string categoryName, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(int categoryId, string categoryName, bool isActive, CancellationToken cancellationToken = default);
    Task SetCategoryStatusAsync(int categoryId, bool isActive, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
}

public sealed class CategoryService : ICategoryService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CategoryService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(string? search, int status, CancellationToken cancellationToken = default)
    {
        var categories = new List<CategoryDto>();

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Kategori_Listele", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@Arama", SqlDbType.NVarChar, 150).Value = string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim();
        command.Parameters.Add("@Durum", SqlDbType.Int).Value = status;

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(new CategoryDto
            {
                KategoriId = reader.GetInt("KategoriId"),
                KategoriAdi = reader.GetText("KategoriAdi"),
                AktifMi = reader.GetBool("AktifMi"),
                OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi")
            });
        }

        return categories;
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Kategori_Getir", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@KategoriId", SqlDbType.Int).Value = categoryId;

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new CategoryDto
        {
            KategoriId = reader.GetInt("KategoriId"),
            KategoriAdi = reader.GetText("KategoriAdi"),
            AktifMi = reader.GetBool("AktifMi"),
            OlusturmaTarihi = reader.GetNullableDate("OlusturmaTarihi")
        };
    }

    public async Task<int> CreateCategoryAsync(string categoryName, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Kategori_Ekle", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@KategoriAdi", SqlDbType.NVarChar, 150).Value = categoryName.Trim();

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null ? 0 : Convert.ToInt32(result);
    }

    public async Task UpdateCategoryAsync(int categoryId, string categoryName, bool isActive, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Kategori_Guncelle", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@KategoriId", SqlDbType.Int).Value = categoryId;
        command.Parameters.Add("@KategoriAdi", SqlDbType.NVarChar, 150).Value = categoryName.Trim();
        command.Parameters.Add("@AktifMi", SqlDbType.Bit).Value = isActive;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetCategoryStatusAsync(int categoryId, bool isActive, CancellationToken cancellationToken = default)
    {
        CategoryDto? category = await GetCategoryByIdAsync(categoryId, cancellationToken);
        if (category == null)
            throw new InvalidOperationException("Kategori bulunamadı.");

        await UpdateCategoryAsync(categoryId, category.KategoriAdi, isActive, cancellationToken);
    }

    public async Task DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Kategori_Sil", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@KategoriId", SqlDbType.Int).Value = categoryId;

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
