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
    private readonly IPasswordService _passwordService;

    public AuthService(ISqlConnectionFactory connectionFactory, IPasswordService passwordService)
    {
        _connectionFactory = connectionFactory;
        _passwordService = passwordService;
    }

    public async Task<KullaniciDto?> LoginAsync(string kullaniciAdi, string sifre, CancellationToken cancellationToken = default)
    {
        KullaniciLoginRecord? loginRecord = await TryGetLoginRecordAsync(kullaniciAdi, cancellationToken);
        if (loginRecord != null)
        {
            if (!_passwordService.VerifyPassword(sifre, loginRecord.Sifre))
                return null;

            if (_passwordService.NeedsMigration(loginRecord.Sifre))
                await TryMigratePasswordAsync(loginRecord.KullaniciId, _passwordService.HashPassword(sifre.Trim()), cancellationToken);

            return new KullaniciDto
            {
                KullaniciId = loginRecord.KullaniciId,
                KullaniciAdi = loginRecord.KullaniciAdi,
                AdSoyad = loginRecord.AdSoyad,
                Rol = loginRecord.Rol
            };
        }

        // Compatibility fallback for databases where the auth security migration has not been applied yet.
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

    private async Task<KullaniciLoginRecord?> TryGetLoginRecordAsync(string kullaniciAdi, CancellationToken cancellationToken)
    {
        try
        {
            await using SqlConnection connection = _connectionFactory.CreateConnection();
            await using SqlCommand command = new("sp_Kullanici_LoginBilgisi_Getir", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@KullaniciAdi", SqlDbType.NVarChar, 100).Value = (kullaniciAdi ?? string.Empty).Trim();

            await connection.OpenAsync(cancellationToken);

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new KullaniciLoginRecord
            {
                KullaniciId = reader.GetInt("KullaniciId"),
                KullaniciAdi = reader.GetText("KullaniciAdi"),
                Sifre = reader.GetText("Sifre"),
                AdSoyad = reader.GetText("AdSoyad"),
                Rol = reader.GetText("Rol")
            };
        }
        catch (SqlException ex) when (ex.Number == 2812)
        {
            return null;
        }
    }

    private async Task TryMigratePasswordAsync(int kullaniciId, string passwordHash, CancellationToken cancellationToken)
    {
        try
        {
            await using SqlConnection connection = _connectionFactory.CreateConnection();
            await using SqlCommand command = new("sp_Kullanici_SifreHash_Guncelle", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = kullaniciId;
            command.Parameters.Add("@Sifre", SqlDbType.NVarChar, 100).Value = passwordHash;

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 2812)
        {
        }
    }

    private sealed class KullaniciLoginRecord
    {
        public int KullaniciId { get; init; }
        public string KullaniciAdi { get; init; } = string.Empty;
        public string Sifre { get; init; } = string.Empty;
        public string AdSoyad { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
    }
}
