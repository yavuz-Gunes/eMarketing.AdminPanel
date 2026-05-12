using System.Net.Http.Json;
using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class TeamApiClient : ApiClientBase
{
    public TeamApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<StoreTeamResponseDto> GetTeamAsync(int storeId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync($"bayi-operasyon/magazalar/{storeId}/ekip", cancellationToken);
        return await ReadRequiredAsync<StoreTeamResponseDto>(response, cancellationToken);
    }

    public async Task CreatePersonnelAsync(int storeId, StorePersonnelCreateRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync($"bayi-operasyon/magazalar/{storeId}/personel", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateDutyAsync(int storeId, int userStoreId, string duty, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"bayi-operasyon/magazalar/{storeId}/personel/{userStoreId}/gorev", new StoreDutyUpdateRequest
        {
            Gorev = duty
        }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetOrderAuthorityAsync(int storeId, int userId, bool active, string notes, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"bayi-operasyon/magazalar/{storeId}/personel/{userId}/siparis-yetkisi", new StoreOrderAuthorityUpdateRequest
        {
            AktifMi = active,
            Notlar = notes
        }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task RemovePersonnelAsync(int storeId, int userStoreId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().DeleteAsync($"bayi-operasyon/magazalar/{storeId}/personel/{userStoreId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }
}
