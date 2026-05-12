using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class StoreApiClient : ApiClientBase
{
    public StoreApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<StoreDto>> GetStoresAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync("magazalar/secim?sadeceAktif=true", cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<StoreDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<SiparisYetkilisiDto>> GetOrderAuthoritiesAsync(int storeId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync($"magazalar/{storeId}/siparis-yetkilileri", cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<SiparisYetkilisiDto>>(response, cancellationToken);
    }
}
