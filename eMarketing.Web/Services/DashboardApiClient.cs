using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class DashboardApiClient : ApiClientBase
{
    public DashboardApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"dashboard/ozet?magazaId={storeId.Value}&tumMagazalar=false"
            : "dashboard/ozet?tumMagazalar=false";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<DashboardSummaryDto>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardRecentOrderDto>> GetRecentOrdersAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"dashboard/son-siparisler?magazaId={storeId.Value}&tumMagazalar=false"
            : "dashboard/son-siparisler?tumMagazalar=false";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<DashboardRecentOrderDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardCriticalStockDto>> GetCriticalStockAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"dashboard/kritik-stok?magazaId={storeId.Value}&tumMagazalar=false"
            : "dashboard/kritik-stok?tumMagazalar=false";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<DashboardCriticalStockDto>>(response, cancellationToken);
    }
}
