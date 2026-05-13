using eMarketing.Service.Dtos;
using eMarketing.Web.State;

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
}
