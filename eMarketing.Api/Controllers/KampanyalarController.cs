using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/kampanyalar")]
[Authorize]
public sealed class KampanyalarController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public KampanyalarController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet("aktif")]
    [Authorize(Policy = "CanViewDashboard")]
    public async Task<ActionResult<IReadOnlyList<DashboardCampaignDto>>> GetAktifKampanyalar([FromQuery] int? magazaId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _campaignService.GetActiveCampaignsAsync(magazaId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewDashboard")]
    public async Task<ActionResult<CampaignDto>> GetDetay(int id, [FromQuery] int? magazaId = null, CancellationToken cancellationToken = default)
    {
        CampaignDto? campaign = await _campaignService.GetCampaignDetailAsync(id, magazaId, cancellationToken);
        return campaign == null ? NotFound("Kampanya bulunamadı.") : Ok(campaign);
    }

    [HttpGet]
    [Authorize(Policy = "CanManageCampaigns")]
    public async Task<ActionResult<IReadOnlyList<CampaignDto>>> GetAdmin([FromQuery] string arama = "", [FromQuery] int durum = -1, CancellationToken cancellationToken = default)
    {
        return Ok(await _campaignService.GetCampaignsAsync(arama, durum, cancellationToken));
    }

    [HttpGet("admin/{id:int}")]
    [Authorize(Policy = "CanManageCampaigns")]
    public async Task<ActionResult<CampaignDto>> GetAdminById(int id, CancellationToken cancellationToken = default)
    {
        CampaignDto? campaign = await _campaignService.GetCampaignByIdAsync(id, cancellationToken);
        return campaign == null ? NotFound("Kampanya bulunamadı.") : Ok(campaign);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageCampaigns")]
    public async Task<ActionResult<object>> Create([FromBody] CampaignSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _campaignService.CreateCampaignAsync(request, cancellationToken);
        return Ok(new { KampanyaId = id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageCampaigns")]
    public async Task<IActionResult> Update(int id, [FromBody] CampaignSaveRequest request, CancellationToken cancellationToken)
    {
        await _campaignService.UpdateCampaignAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/durum")]
    [Authorize(Policy = "CanManageCampaigns")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] CampaignStatusRequest request, CancellationToken cancellationToken)
    {
        await _campaignService.SetCampaignStatusAsync(id, request.AktifMi, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageCampaigns")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _campaignService.DeleteCampaignAsync(id, cancellationToken);
        return NoContent();
    }
}

public sealed class CampaignStatusRequest
{
    public bool AktifMi { get; set; }
}
