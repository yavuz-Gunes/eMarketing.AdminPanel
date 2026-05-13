using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bayi-stoklari")]
[Authorize]
public sealed class BayiStoklariController : ControllerBase
{
    private readonly IStockService _stockService;

    public BayiStoklariController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewStocks")]
    public async Task<ActionResult<IReadOnlyList<StockItemDto>>> Get(
        [FromQuery] int? magazaId = null,
        [FromQuery] string arama = "",
        [FromQuery] bool sadeceStokta = false,
        [FromQuery] bool sadeceKritik = false,
        [FromQuery] bool sadeceAktif = true,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _stockService.GetStocksAsync(new StockFilterRequest
        {
            MagazaId = magazaId,
            Arama = arama,
            SadeceStokta = sadeceStokta,
            SadeceKritik = sadeceKritik,
            SadeceAktif = sadeceAktif
        }, cancellationToken));
    }

    [HttpGet("ozet")]
    [Authorize(Policy = "CanViewStocks")]
    public async Task<ActionResult<StockSummaryDto>> GetOzet([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _stockService.GetSummaryAsync(magazaId, tumMagazalar, cancellationToken));
    }

    [HttpGet("hareketler")]
    [Authorize(Policy = "CanViewStocks")]
    public async Task<ActionResult<IReadOnlyList<StockMovementDto>>> GetHareketler([FromQuery] int magazaId, [FromQuery] int urunId, [FromQuery] int kayitSayisi = 25, CancellationToken cancellationToken = default)
    {
        return Ok(await _stockService.GetMovementsAsync(magazaId, urunId, kayitSayisi, cancellationToken));
    }

    [HttpPatch("{magazaStokId:int}/minimum")]
    [Authorize(Policy = "CanManageStock")]
    public async Task<IActionResult> MinimumGuncelle(int magazaStokId, [FromBody] MinimumStokRequest request, CancellationToken cancellationToken)
    {
        await _stockService.UpdateMinimumAsync(magazaStokId, request.MinimumStok, cancellationToken);
        return NoContent();
    }

    [HttpPost("hareket")]
    [Authorize(Policy = "CanManageStock")]
    public async Task<IActionResult> HareketIsle([FromBody] StokHareketRequest request, CancellationToken cancellationToken)
    {
        await _stockService.ProcessMovementAsync(new StockOperationRequest
        {
            MagazaId = request.MagazaId,
            UrunId = request.UrunId,
            HareketTipi = request.HareketTipi,
            Miktar = request.Miktar,
            Aciklama = request.Aciklama,
            MinimumStok = request.MinimumStok
        }, cancellationToken);

        return NoContent();
    }

    [HttpPost("merkez-stok/artir")]
    [Authorize(Policy = "CanManageCentralStock")]
    public async Task<IActionResult> MerkezStokArtir([FromBody] MerkezStokArtirRequest request, CancellationToken cancellationToken)
    {
        await _stockService.ProcessCentralStockAsync(new CentralStockOperationRequest
        {
            UrunId = request.UrunId,
            Miktar = request.Miktar,
            Aciklama = request.Aciklama
        }, cancellationToken);

        return NoContent();
    }
}

public sealed class MinimumStokRequest
{
    public int MinimumStok { get; set; }
}

public sealed class StokHareketRequest
{
    public int MagazaId { get; set; }
    public int UrunId { get; set; }
    public string HareketTipi { get; set; } = "ManuelGiris";
    public int Miktar { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public int? MinimumStok { get; set; }
}

public sealed class MerkezStokArtirRequest
{
    public int UrunId { get; set; }
    public int Miktar { get; set; }
    public string Aciklama { get; set; } = string.Empty;
}
