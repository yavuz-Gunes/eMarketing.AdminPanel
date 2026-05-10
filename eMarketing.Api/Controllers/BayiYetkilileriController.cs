using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bayi-yetkilileri")]
[Authorize]
public sealed class BayiYetkilileriController : ControllerBase
{
    private readonly ISqlDataService _dataService;

    public BayiYetkilileriController(ISqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> Get([FromQuery] string arama = "", [FromQuery] int durum = -1, [FromQuery] int? bayiId = null, [FromQuery] int? magazaId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_BayiYetkili_Listele", new[]
        {
            SqlDataService.TextParam("@Arama", 200, arama),
            SqlDataService.Param("@Durum", SqlDbType.Int, durum),
            SqlDataService.Param("@BayiId", SqlDbType.Int, bayiId),
            SqlDataService.Param("@MagazaId", SqlDbType.Int, magazaId)
        }, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetById(int id, CancellationToken cancellationToken)
    {
        Dictionary<string, object?>? row = await _dataService.QuerySingleAsync("sp_BayiYetkili_Getir", new[]
        {
            SqlDataService.Param("@BayiYetkiliId", SqlDbType.Int, id)
        }, cancellationToken);

        return row == null ? NotFound("Yetkili bulunamadı.") : Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Save([FromBody] BayiYetkiliSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _dataService.ExecuteScalarIntAsync("sp_BayiYetkili_Kaydet", request.ToParameters(), cancellationToken);
        return Ok(new { BayiYetkiliId = id });
    }

    [HttpPatch("{id:int}/durum")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] BayiYetkiliStatusRequest request, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_BayiYetkili_DurumGuncelle", new[]
        {
            SqlDataService.Param("@BayiYetkiliId", SqlDbType.Int, id),
            SqlDataService.Param("@AktifMi", SqlDbType.Bit, request.AktifMi)
        }, cancellationToken);

        return NoContent();
    }
}

public sealed class BayiYetkiliSaveRequest
{
    public int? BayiYetkiliId { get; set; }
    public int BayiId { get; set; }
    public int? MagazaId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Gorev { get; set; } = string.Empty;
    public string Notlar { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;

    public SqlParameter[] ToParameters()
    {
        return new[]
        {
            SqlDataService.Param("@BayiYetkiliId", SqlDbType.Int, BayiYetkiliId),
            SqlDataService.Param("@BayiId", SqlDbType.Int, BayiId),
            SqlDataService.Param("@MagazaId", SqlDbType.Int, MagazaId),
            SqlDataService.NullableTextParam("@AdSoyad", 200, AdSoyad),
            SqlDataService.NullableTextParam("@Telefon", 60, Telefon),
            SqlDataService.NullableTextParam("@Email", 400, Email),
            SqlDataService.NullableTextParam("@Gorev", 100, Gorev),
            SqlDataService.NullableTextParam("@Notlar", 500, Notlar),
            SqlDataService.Param("@AktifMi", SqlDbType.Bit, AktifMi)
        };
    }
}

public sealed class BayiYetkiliStatusRequest
{
    public bool AktifMi { get; set; }
}
