using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/raporlar")]
[Authorize(Policy = "CanViewReports")]
public sealed class RaporlarController : ControllerBase
{
    private readonly IReportService _reportService;

    public RaporlarController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("portal")]
    public async Task<ActionResult<PortalReportDto>> GetPortalReport(
        [FromQuery] int? magazaId = null,
        [FromQuery] bool tumMagazalar = false,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _reportService.GetPortalReportAsync(magazaId, tumMagazalar, startDate, endDate, cancellationToken));
    }

    [HttpGet("portal/pdf")]
    public async Task<IActionResult> ExportPortalReportPdf(
        [FromQuery] int? magazaId = null,
        [FromQuery] bool tumMagazalar = false,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        ReportExportResultDto export = await _reportService.ExportPortalReportPdfAsync(magazaId, tumMagazalar, startDate, endDate, cancellationToken);
        return File(export.Content, export.ContentType, export.FileName);
    }
}
