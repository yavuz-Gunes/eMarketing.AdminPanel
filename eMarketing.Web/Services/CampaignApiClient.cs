using eMarketing.Service.Dtos;
using eMarketing.Web.State;
using System.Net;
using System.Net.Http.Json;

namespace eMarketing.Web.Services;

public sealed class CampaignApiClient : ApiClientBase
{
    public CampaignApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<DashboardCampaignDto>> GetActiveCampaignsAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"kampanyalar/aktif?magazaId={storeId.Value}"
            : "kampanyalar/aktif";

        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<DashboardCampaignDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<CampaignDto>> GetAdminCampaignsAsync(string search = "", int status = -1, CancellationToken cancellationToken = default)
    {
        string query = $"kampanyalar?arama={Uri.EscapeDataString(search)}&durum={status}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        ThrowIfUnauthorized(response);
        return await ReadRequiredAsync<IReadOnlyList<CampaignDto>>(response, cancellationToken);
    }

    public async Task<CampaignDto?> GetAdminCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync($"kampanyalar/admin/{campaignId}", cancellationToken);
        ThrowIfUnauthorized(response);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await ReadRequiredAsync<CampaignDto>(response, cancellationToken);
    }

    public async Task<int> CreateCampaignAsync(CampaignSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("kampanyalar", request, cancellationToken);
        ThrowIfUnauthorized(response);
        CampaignCreateResult result = await ReadRequiredAsync<CampaignCreateResult>(response, cancellationToken);
        return result.KampanyaId;
    }

    public async Task UpdateCampaignAsync(int campaignId, CampaignSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PutAsJsonAsync($"kampanyalar/{campaignId}", request, cancellationToken);
        ThrowIfUnauthorized(response);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetCampaignStatusAsync(int campaignId, bool active, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"kampanyalar/{campaignId}/durum", new { AktifMi = active }, cancellationToken);
        ThrowIfUnauthorized(response);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteCampaignAsync(int campaignId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().DeleteAsync($"kampanyalar/{campaignId}", cancellationToken);
        ThrowIfUnauthorized(response);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync("urunler?durum=1", cancellationToken);
        ThrowIfUnauthorized(response);
        return await ReadRequiredAsync<IReadOnlyList<ProductDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync("kategoriler?durum=1", cancellationToken);
        ThrowIfUnauthorized(response);
        return await ReadRequiredAsync<IReadOnlyList<CategoryDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DealerDetailDto>> GetDealersAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync("bayiler?durum=1", cancellationToken);
        ThrowIfUnauthorized(response);
        return await ReadRequiredAsync<IReadOnlyList<DealerDetailDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<StoreDto>> GetStoresAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync("magazalar/secim?sadeceAktif=true", cancellationToken);
        ThrowIfUnauthorized(response);
        return await ReadRequiredAsync<IReadOnlyList<StoreDto>>(response, cancellationToken);
    }

    public async Task<CampaignDto?> GetCampaignAsync(int campaignId, int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"kampanyalar/{campaignId}?magazaId={storeId.Value}"
            : $"kampanyalar/{campaignId}";

        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);

        ThrowIfUnauthorized(response);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await ReadRequiredAsync<CampaignDto>(response, cancellationToken);
    }

    private static void ThrowIfUnauthorized(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Oturum süresi doldu.");
    }

    private sealed class CampaignCreateResult
    {
        public int KampanyaId { get; set; }
    }
}
