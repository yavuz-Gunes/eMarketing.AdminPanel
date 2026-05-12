using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bayi-yetkilileri")]
[Authorize]
public sealed class BayiYetkilileriController : ControllerBase
{
    private readonly IBayiYetkiliService _service;

    public BayiYetkilileriController(IBayiYetkiliService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewDealers")]
    public async Task<ActionResult<IReadOnlyList<BayiYetkiliDto>>> Get(
        [FromQuery] string arama = "",
        [FromQuery] int durum = -1,
        [FromQuery] int? bayiId = null,
        [FromQuery] int? magazaId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetAsync(new BayiYetkiliFilterRequest
        {
            Arama = arama,
            Durum = durum,
            BayiId = bayiId,
            MagazaId = magazaId
        }, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewDealers")]
    public async Task<ActionResult<BayiYetkiliDto>> GetById(int id, CancellationToken cancellationToken)
    {
        BayiYetkiliDto? row = await _service.GetByIdAsync(id, cancellationToken);
        return row == null ? NotFound("Yetkili bulunamadı.") : Ok(row);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<ActionResult<object>> Save([FromBody] BayiYetkiliSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _service.SaveAsync(request, cancellationToken);
        return Ok(new { BayiYetkiliId = id });
    }

    [HttpPatch("{id:int}/durum")]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] BayiYetkiliStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.SetStatusAsync(id, request.AktifMi, cancellationToken);
        return NoContent();
    }
}
