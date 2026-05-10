using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/magazalar")]
[Authorize]
public sealed class MagazalarController : ControllerBase
{
    private readonly ISqlDataService _dataService;

    public MagazalarController(ISqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet("secim")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetSecim([FromQuery] string arama = "", [FromQuery] bool sadeceAktif = true, [FromQuery] int? kullaniciId = null, [FromQuery] bool adminMi = false, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_Magaza_Secim_Listele", new[]
        {
            SqlDataService.TextParam("@Arama", 200, arama),
            SqlDataService.Param("@SadeceAktif", SqlDbType.Bit, sadeceAktif),
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken));
    }

    [HttpGet("secim/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetById(int id, [FromQuery] int? kullaniciId = null, [FromQuery] bool adminMi = false, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?>? row = await _dataService.QuerySingleAsync("sp_Magaza_Secim_Getir", new[]
        {
            SqlDataService.Param("@MagazaId", SqlDbType.Int, id),
            SqlDataService.Param("@KullaniciId", SqlDbType.Int, kullaniciId),
            SqlDataService.Param("@AdminMi", SqlDbType.Bit, adminMi)
        }, cancellationToken);

        return row == null ? NotFound("Mağaza bulunamadı.") : Ok(row);
    }
}
