using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/personel")]
[Authorize]
public sealed class PersonelController : ControllerBase
{
    private readonly ISqlDataService _dataService;

    public PersonelController(ISqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> Get([FromQuery] string arama = "", [FromQuery] bool sadeceAktif = false, [FromQuery] int? goruntuleyenKullaniciId = null, [FromQuery] bool adminMi = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_Kullanici_Listele", new[]
        {
            SqlDataService.TextParam("@Arama", 200, arama),
            SqlDataService.Param("@SadeceAktif", SqlDbType.Bit, sadeceAktif),
            SqlDataService.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, goruntuleyenKullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<object>> Save([FromBody] PersonelSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _dataService.ExecuteScalarIntAsync("sp_Kullanici_Kaydet", new[]
        {
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, request.KullaniciId),
            SqlDataService.NullableTextParam("@KullaniciAdi", 100, request.KullaniciAdi),
            SqlDataService.NullableTextParam("@Sifre", 100, request.Sifre),
            SqlDataService.NullableTextParam("@AdSoyad", 150, request.AdSoyad),
            SqlDataService.NullableTextParam("@Rol", 50, request.Rol),
            SqlDataService.Param("@AktifMi", SqlDbType.Bit, request.AktifMi)
        }, cancellationToken);

        return Ok(new { KullaniciId = id });
    }

    [HttpGet("{kullaniciId:int}/magazalar")]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetMagazalar(int kullaniciId, [FromQuery] int? goruntuleyenKullaniciId = null, [FromQuery] bool adminMi = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_KullaniciMagaza_Listele", new[]
        {
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, goruntuleyenKullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken));
    }

    [HttpGet("{kullaniciId:int}/atanabilir-magazalar")]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetAtanabilirMagazalar(int kullaniciId, [FromQuery] string arama = "", [FromQuery] int? goruntuleyenKullaniciId = null, [FromQuery] bool adminMi = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_KullaniciMagaza_AtanmamisMagaza_Listele", new[]
        {
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.TextParam("@Arama", 200, arama),
            SqlDataService.Param("@GoruntuleyenKullaniciId", SqlDbType.Int, goruntuleyenKullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken));
    }

    [HttpPost("{kullaniciId:int}/magazalar/{magazaId:int}")]
    public async Task<IActionResult> MagazaAta(int kullaniciId, int magazaId, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_KullaniciMagaza_Ata", new[]
        {
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.Param("@MagazaId", SqlDbType.Int, magazaId)
        }, cancellationToken);

        return NoContent();
    }

    [HttpDelete("magaza-yetkileri/{kullaniciMagazaId:int}")]
    public async Task<IActionResult> MagazaKaldir(int kullaniciMagazaId, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_KullaniciMagaza_Kaldir", new[]
        {
            SqlDataService.Param("@KullaniciMagazaId", SqlDbType.Int, kullaniciMagazaId)
        }, cancellationToken);

        return NoContent();
    }
}

public sealed class PersonelSaveRequest
{
    public int? KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}
