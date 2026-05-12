using System.Data;
using eMarketing.Service.Connection;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Security;

public interface IStoreAuthorizationService
{
    Task EnsureStoreAccessAsync(int storeId, CancellationToken cancellationToken = default);
    Task EnsureCanCreateOrderAsync(int storeId, int? dealerAuthorityId, CancellationToken cancellationToken = default);
}

public sealed class StoreAuthorizationService : IStoreAuthorizationService
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public StoreAuthorizationService(ISqlConnectionFactory connectionFactory, ICurrentUserService currentUserService)
    {
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task EnsureStoreAccessAsync(int storeId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (currentUser.CanSeeAllStores)
            return;

        if (!currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(
            "SELECT COUNT(1) FROM dbo.KullaniciMagazalari WHERE KullaniciId = @KullaniciId AND MagazaId = @MagazaId AND AktifMi = 1",
            connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.Value;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null || Convert.ToInt32(result) <= 0)
            throw new UnauthorizedAccessException("Bu mağaza için işlem yetkiniz yok.");
    }

    public async Task EnsureCanCreateOrderAsync(int storeId, int? dealerAuthorityId, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (!dealerAuthorityId.HasValue || dealerAuthorityId.Value <= 0)
            throw new UnauthorizedAccessException("Sipariş yetkilisi seçimi zorunludur.");

        if (!currentUser.UserId.HasValue && !currentUser.CanSeeAllStores)
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await using SqlCommand command = new(@"
SELECT COUNT(1)
FROM dbo.BayiYetkilileri byk
INNER JOIN dbo.KullaniciMagazalari km
    ON km.KullaniciId = byk.KullaniciId
   AND km.MagazaId = byk.MagazaId
   AND km.AktifMi = 1
WHERE byk.BayiYetkiliId = @BayiYetkiliId
  AND byk.MagazaId = @MagazaId
  AND byk.AktifMi = 1
  AND byk.YetkiTipi = N'SiparisYetkilisi'
  AND (@AdminMi = 1 OR byk.KullaniciId = @KullaniciId);", connection);

        command.CommandType = CommandType.Text;
        command.Parameters.Add("@BayiYetkiliId", SqlDbType.Int).Value = dealerAuthorityId.Value;
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.HasValue ? currentUser.UserId.Value : DBNull.Value;
        command.Parameters.Add("@AdminMi", SqlDbType.Bit).Value = currentUser.CanSeeAllStores;

        await connection.OpenAsync(cancellationToken);
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        if (result == null || Convert.ToInt32(result) <= 0)
            throw new UnauthorizedAccessException("Bu mağaza için geçerli bir sipariş yetkiniz yok.");
    }
}
