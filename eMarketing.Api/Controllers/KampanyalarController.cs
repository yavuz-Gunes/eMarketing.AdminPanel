using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/kampanyalar")]
[Authorize(Policy = "CanViewDashboard")]
public sealed class KampanyalarController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public KampanyalarController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet("aktif")]
    public async Task<ActionResult<IReadOnlyList<DashboardCampaignDto>>> GetAktifKampanyalar([FromQuery] int? magazaId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _campaignService.GetActiveCampaignsAsync(magazaId, cancellationToken));
    }
}
