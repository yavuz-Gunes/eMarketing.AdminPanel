using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/magazalar")]
[Authorize]
public sealed class MagazalarController : ControllerBase
{
    private readonly IStoreService _storeService;

    public MagazalarController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet("secim")]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<ActionResult<IReadOnlyList<StoreDto>>> GetSecim([FromQuery] string arama = "", [FromQuery] bool sadeceAktif = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _storeService.GetStoresAsync(arama, sadeceAktif, cancellationToken));
    }

    [HttpGet("secim/{id:int}")]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<ActionResult<StoreDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        StoreDto? row = await _storeService.GetStoreAsync(id, cancellationToken);
        return row == null ? NotFound("Mağaza bulunamadı.") : Ok(row);
    }
}
