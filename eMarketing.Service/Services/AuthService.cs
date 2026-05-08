using System.Data;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Services;

public interface IAuthService
{
    Task<KullaniciDto?> LoginAsync(string kullaniciAdi, string sifre, CancellationToken cancellationToken = default);
}

public sealed class AuthService : IAuthService
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuthService(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<KullaniciDto?> LoginAsync(string kullaniciAdi, string sifre, CancellationToken cancellationToken = default)
    {
        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new("sp_Kullanici_GirisYap", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@KullaniciAdi", SqlDbType.NVarChar, 100).Value = (kullaniciAdi ?? string.Empty).Trim();
        command.Parameters.Add("@Sifre", SqlDbType.NVarChar, 200).Value = (sifre ?? string.Empty).Trim();

        await connection.OpenAsync(cancellationToken);

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new KullaniciDto
        {
            KullaniciId = reader.GetInt("KullaniciId"),
            KullaniciAdi = reader.GetText("KullaniciAdi"),
            AdSoyad = reader.GetText("AdSoyad"),
            Rol = reader.GetText("Rol")
        };
    }
}
