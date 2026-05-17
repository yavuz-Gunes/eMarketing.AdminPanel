using System.Net.Http.Json;
using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class NotificationApiClient : ApiClientBase
{
    public NotificationApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(int? storeId, bool unreadOnly = false, int limit = 50, CancellationToken cancellationToken = default)
    {
        string query = $"bildirimler?sadeceOkunmamis={unreadOnly.ToString().ToLowerInvariant()}&limit={limit}";
        if (storeId.HasValue)
            query += $"&magazaId={storeId.Value}";

        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<NotificationDto>>(response, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"bildirimler/okunmamis-sayisi?magazaId={storeId.Value}"
            : "bildirimler/okunmamis-sayisi";

        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        NotificationCountDto result = await ReadRequiredAsync<NotificationCountDto>(response, cancellationToken);
        return result.OkunmamisSayisi;
    }

    public async Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsync($"bildirimler/{notificationId}/okundu", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"bildirimler/tumunu-okundu?magazaId={storeId.Value}"
            : "bildirimler/tumunu-okundu";

        HttpResponseMessage response = await CreateClient().PostAsync(query, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task GenerateSystemNotificationsAsync(int? storeId = null, bool allStores = true, CancellationToken cancellationToken = default)
    {
        string query = $"bildirimler/sistem-kontrol?tumMagazalar={allStores.ToString().ToLowerInvariant()}";
        if (storeId.HasValue)
            query += $"&magazaId={storeId.Value}";

        HttpResponseMessage response = await CreateClient().PostAsync(query, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetAdminNotificationsAsync(string search = "", string type = "", int status = -1, int? storeId = null, CancellationToken cancellationToken = default)
    {
        string query = $"bildirimler/admin?arama={Uri.EscapeDataString(search)}&tip={Uri.EscapeDataString(type)}&durum={status}";
        if (storeId.HasValue)
            query += $"&magazaId={storeId.Value}";

        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<NotificationDto>>(response, cancellationToken);
    }

    public async Task<int> CreateNotificationAsync(NotificationSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("bildirimler/admin", request, cancellationToken);
        NotificationCreateResult result = await ReadRequiredAsync<NotificationCreateResult>(response, cancellationToken);
        return result.BildirimId;
    }

    public async Task SetStatusAsync(int notificationId, bool active, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"bildirimler/admin/{notificationId}/durum", new { AktifMi = active }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().DeleteAsync($"bildirimler/admin/{notificationId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private sealed class NotificationCreateResult
    {
        public int BildirimId { get; set; }
    }
}
