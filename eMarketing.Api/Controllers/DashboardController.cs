using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "CanViewDashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("ozet")]
    public async Task<ActionResult<DashboardSummaryDto>> GetOzet([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _dashboardService.GetSummaryAsync(magazaId, tumMagazalar, cancellationToken));
    }

    [HttpGet("son-siparisler")]
    public async Task<ActionResult<IReadOnlyList<DashboardRecentOrderDto>>> GetSonSiparisler([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _dashboardService.GetRecentOrdersAsync(magazaId, tumMagazalar, cancellationToken));
    }

    [HttpGet("kritik-stok")]
    public async Task<ActionResult<IReadOnlyList<DashboardCriticalStockDto>>> GetKritikStok([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _dashboardService.GetCriticalStockAsync(magazaId, tumMagazalar, cancellationToken));
    }
}
