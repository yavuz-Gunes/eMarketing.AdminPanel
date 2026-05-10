using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly ISqlDataService _dataService;

    public DashboardController(ISqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet("ozet")]
    public async Task<ActionResult<object>> GetOzet([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = true, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object?>? row = await _dataService.QuerySingleAsync(
            "sp_Dashboard_Ozet_Getir",
            MagazaParams(magazaId, tumMagazalar),
            cancellationToken);

        return Ok(row ?? new Dictionary<string, object?>());
    }

    [HttpGet("son-siparisler")]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetSonSiparisler([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_Dashboard_SonSiparisler_Getir", MagazaParams(magazaId, tumMagazalar), cancellationToken));
    }

    [HttpGet("kritik-stok")]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetKritikStok([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = true, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_Dashboard_KritikStok_Getir", MagazaParams(magazaId, tumMagazalar), cancellationToken));
    }

    private static SqlParameter[] MagazaParams(int? magazaId, bool tumMagazalar)
    {
        return new[]
        {
            SqlDataService.Param("@MagazaId", SqlDbType.Int, magazaId),
            SqlDataService.Param("@TumMagazalar", SqlDbType.Bit, tumMagazalar)
        };
    }
}
