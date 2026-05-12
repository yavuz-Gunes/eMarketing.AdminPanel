using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bayiler")]
[Authorize]
public sealed class BayilerController : ControllerBase
{
    private readonly IDealerService _dealerService;

    public BayilerController(IDealerService dealerService)
    {
        _dealerService = dealerService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewDealers")]
    public async Task<ActionResult<IReadOnlyList<DealerDetailDto>>> Get([FromQuery] string arama = "", [FromQuery] int durum = -1, CancellationToken cancellationToken = default)
    {
        return Ok(await _dealerService.GetDealersAsync(arama, durum, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewDealers")]
    public async Task<ActionResult<DealerDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        DealerDetailDto? dealer = await _dealerService.GetDealerByIdAsync(id, cancellationToken);
        return dealer == null ? NotFound("Bayi bulunamadı.") : Ok(dealer);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<ActionResult<object>> Create([FromBody] DealerSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _dealerService.CreateDealerAsync(request, cancellationToken);
        return Ok(new { CustomerId = id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<IActionResult> Update(int id, [FromBody] DealerSaveRequest request, CancellationToken cancellationToken)
    {
        await _dealerService.UpdateDealerAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/durum")]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] DealerStatusRequest request, CancellationToken cancellationToken)
    {
        await _dealerService.SetDealerStatusAsync(id, request.AktifMi, cancellationToken);
        return NoContent();
    }

    [HttpGet("{customerId:int}/magazalar")]
    [Authorize(Policy = "CanViewDealers")]
    public async Task<ActionResult<IReadOnlyList<DealerStoreDto>>> GetStores(int customerId, [FromQuery] int durum = -1, CancellationToken cancellationToken = default)
    {
        return Ok(await _dealerService.GetStoresAsync(customerId, durum, cancellationToken));
    }

    [HttpPost("{customerId:int}/magazalar")]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<ActionResult<object>> CreateStore(int customerId, [FromBody] DealerStoreSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _dealerService.CreateStoreAsync(customerId, request, cancellationToken);
        return Ok(new { CustomerStoreId = id });
    }
}

[ApiController]
[Route("api/bayi-magazalar")]
[Authorize]
public sealed class BayiMagazalarController : ControllerBase
{
    private readonly IDealerService _dealerService;

    public BayiMagazalarController(IDealerService dealerService)
    {
        _dealerService = dealerService;
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewDealers")]
    public async Task<ActionResult<DealerStoreDto>> GetById(int id, CancellationToken cancellationToken)
    {
        DealerStoreDto? store = await _dealerService.GetStoreByIdAsync(id, cancellationToken);
        return store == null ? NotFound("Bayi mağazası bulunamadı.") : Ok(store);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<IActionResult> Update(int id, [FromBody] DealerStoreSaveRequest request, CancellationToken cancellationToken)
    {
        await _dealerService.UpdateStoreAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/durum")]
    [Authorize(Policy = "CanManageDealers")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] DealerStatusRequest request, CancellationToken cancellationToken)
    {
        await _dealerService.SetStoreStatusAsync(id, request.AktifMi, cancellationToken);
        return NoContent();
    }
}
