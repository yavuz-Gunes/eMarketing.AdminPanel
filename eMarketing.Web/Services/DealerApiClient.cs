using System.Net.Http.Json;
using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class DealerApiClient : ApiClientBase
{
    public DealerApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<DealerDetailDto>> GetDealersAsync(string search = "", int status = -1, CancellationToken cancellationToken = default)
    {
        string query = $"bayiler?arama={Uri.EscapeDataString(search)}&durum={status}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<DealerDetailDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DealerStoreDto>> GetDealerStoresAsync(int customerId, int status = -1, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync($"bayiler/{customerId}/magazalar?durum={status}", cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<DealerStoreDto>>(response, cancellationToken);
    }

    public async Task<DealerStoreDto> GetStoreAsync(int storeId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync($"bayi-magazalar/{storeId}", cancellationToken);
        return await ReadRequiredAsync<DealerStoreDto>(response, cancellationToken);
    }

    public async Task<int> CreateDealerAsync(DealerSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("bayiler", request, cancellationToken);
        CreateDealerResponse result = await ReadRequiredAsync<CreateDealerResponse>(response, cancellationToken);
        return result.CustomerId;
    }

    public async Task UpdateDealerAsync(int customerId, DealerSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PutAsJsonAsync($"bayiler/{customerId}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetDealerStatusAsync(int customerId, bool active, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"bayiler/{customerId}/durum", new DealerStatusRequest
        {
            AktifMi = active
        }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<int> CreateStoreAsync(int customerId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync($"bayiler/{customerId}/magazalar", request, cancellationToken);
        CreateStoreResponse result = await ReadRequiredAsync<CreateStoreResponse>(response, cancellationToken);
        return result.CustomerStoreId;
    }

    public async Task UpdateStoreAsync(int storeId, DealerStoreSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PutAsJsonAsync($"bayi-magazalar/{storeId}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetStoreStatusAsync(int storeId, bool active, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"bayi-magazalar/{storeId}/durum", new DealerStatusRequest
        {
            AktifMi = active
        }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private sealed class CreateDealerResponse
    {
        public int CustomerId { get; set; }
    }

    private sealed class CreateStoreResponse
    {
        public int CustomerStoreId { get; set; }
    }
}
