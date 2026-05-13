using eMarketing.Service.Dtos;
using eMarketing.Service.Security;

namespace eMarketing.Service.Services;

public interface ICampaignService
{
    Task<IReadOnlyList<DashboardCampaignDto>> GetActiveCampaignsAsync(int? storeId, CancellationToken cancellationToken = default);
}

public sealed class CampaignService : ICampaignService
{
    private readonly IStoreAuthorizationService _storeAuthorizationService;

    public CampaignService(IStoreAuthorizationService storeAuthorizationService)
    {
        _storeAuthorizationService = storeAuthorizationService;
    }

    public async Task<IReadOnlyList<DashboardCampaignDto>> GetActiveCampaignsAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        if (storeId.HasValue)
            await _storeAuthorizationService.EnsureStoreAccessAsync(storeId.Value, cancellationToken);

        return Array.Empty<DashboardCampaignDto>();
    }
}
