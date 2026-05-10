using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bayi-stoklari")]
[Authorize]
public sealed class BayiStoklariController : ControllerBase
{
    private readonly ISqlDataService _dataService;

    public BayiStoklariController(ISqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> Get(
        [FromQuery] int? magazaId = null,
        [FromQuery] string arama = "",
        [FromQuery] bool sadeceStokta = false,
        [FromQuery] bool sadeceKritik = false,
        [FromQuery] bool sadeceAktif = true,
        [FromQuery] int? kullaniciId = null,
        [FromQuery] bool adminMi = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_MagazaStok_Listele", new[]
        {
            SqlDataService.Param("@MagazaId", SqlDbType.Int, magazaId),
            SqlDataService.TextParam("@Arama", 200, arama),
            SqlDataService.Param("@SadeceStokta", SqlDbType.Bit, sadeceStokta),
            SqlDataService.Param("@SadeceKritik", SqlDbType.Bit, sadeceKritik),
            SqlDataService.Param("@SadeceAktif", SqlDbType.Bit, sadeceAktif),
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken));
    }

    [HttpGet("ozet")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetOzet([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = true, [FromQuery] int? kullaniciId = null, [FromQuery] bool adminMi = false, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?>? row = await _dataService.QuerySingleAsync("sp_MagazaStok_Ozet_Getir", new[]
        {
            SqlDataService.Param("@MagazaId", SqlDbType.Int, magazaId),
            SqlDataService.Param("@TumMagazalar", SqlDbType.Bit, tumMagazalar),
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken);

        return Ok(row ?? new Dictionary<string, object?>());
    }

    [HttpGet("hareketler")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetHareketler([FromQuery] int magazaId, [FromQuery] int urunId, [FromQuery] int kayitSayisi = 25, [FromQuery] int? kullaniciId = null, [FromQuery] bool adminMi = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_MagazaStok_Hareket_Listele", new[]
        {
            SqlDataService.Param("@MagazaId", SqlDbType.Int, magazaId),
            SqlDataService.Param("@ProductId", SqlDbType.Int, urunId),
            SqlDataService.Param("@KayitSayisi", SqlDbType.Int, kayitSayisi <= 0 ? 25 : kayitSayisi),
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken));
    }

    [HttpPatch("{magazaStokId:int}/minimum")]
    public async Task<IActionResult> MinimumGuncelle(int magazaStokId, [FromBody] MinimumStokRequest request, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_MagazaStok_MinimumGuncelle", new[]
        {
            SqlDataService.Param("@MagazaStokId", SqlDbType.Int, magazaStokId),
            SqlDataService.Param("@MinimumStok", SqlDbType.Int, request.MinimumStok)
        }, cancellationToken);

        return NoContent();
    }

    [HttpPost("hareket")]
    public async Task<IActionResult> HareketIsle([FromBody] StokHareketRequest request, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_MagazaStok_Hareket_Isle", new[]
        {
            SqlDataService.Param("@MagazaId", SqlDbType.Int, request.MagazaId),
            SqlDataService.Param("@ProductId", SqlDbType.Int, request.UrunId),
            SqlDataService.NullableTextParam("@HareketTipi", 50, request.HareketTipi),
            SqlDataService.Param("@Miktar", SqlDbType.Int, request.Miktar),
            SqlDataService.Param("@KaynakSiparisId", SqlDbType.Int, null),
            SqlDataService.Param("@KaynakSiparisKalemId", SqlDbType.Int, null),
            SqlDataService.NullableTextParam("@Aciklama", 500, request.Aciklama),
            SqlDataService.Param("@MinimumStok", SqlDbType.Int, request.MinimumStok)
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
