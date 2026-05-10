using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bayiler")]
[Authorize]
public sealed class BayilerController : ControllerBase
{
    private readonly ISqlDataService _dataService;

    public BayilerController(ISqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> Get([FromQuery] string arama = "", [FromQuery] int durum = -1, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_Musteri_Listele", new[]
        {
            SqlDataService.TextParam("@Arama", 200, arama),
            SqlDataService.Param("@Durum", SqlDbType.Int, durum)
        }, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetById(int id, CancellationToken cancellationToken)
    {
        Dictionary<string, object?>? row = await _dataService.QuerySingleAsync("sp_Musteri_Getir", new[]
        {
            SqlDataService.Param("@CustomerId", SqlDbType.Int, id)
        }, cancellationToken);

        return row == null ? NotFound("Bayi bulunamadı.") : Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] BayiSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _dataService.ExecuteScalarIntAsync("sp_Musteri_Ekle", request.ToInsertParameters(), cancellationToken);
        return Ok(new { CustomerId = id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BayiSaveRequest request, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_Musteri_Guncelle", request.ToUpdateParameters(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/durum")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] StatusRequest request, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_Musteri_DurumGuncelle", new[]
        {
            SqlDataService.Param("@CustomerId", SqlDbType.Int, id),
            SqlDataService.Param("@IsActive", SqlDbType.Bit, request.AktifMi)
        }, cancellationToken);

        return NoContent();
    }

    [HttpGet("{customerId:int}/magazalar")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetStores(int customerId, [FromQuery] int durum = -1, CancellationToken cancellationToken = default)
    {
        return Ok(await _dataService.QueryAsync("sp_MusteriMagaza_Listele", new[]
        {
            SqlDataService.Param("@CustomerId", SqlDbType.Int, customerId),
            SqlDataService.Param("@Durum", SqlDbType.Int, durum)
        }, cancellationToken));
    }

    [HttpPost("{customerId:int}/magazalar")]
    public async Task<ActionResult<object>> CreateStore(int customerId, [FromBody] BayiMagazaSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _dataService.ExecuteScalarIntAsync("sp_MusteriMagaza_Ekle", request.ToInsertParameters(customerId), cancellationToken);
        return Ok(new { CustomerStoreId = id });
    }
}

[ApiController]
[Route("api/bayi-magazalar")]
[Authorize]
public sealed class BayiMagazalarController : ControllerBase
{
    private readonly ISqlDataService _dataService;

    public BayiMagazalarController(ISqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetById(int id, CancellationToken cancellationToken)
    {
        Dictionary<string, object?>? row = await _dataService.QuerySingleAsync("sp_MusteriMagaza_Getir", new[]
        {
            SqlDataService.Param("@CustomerStoreId", SqlDbType.Int, id)
        }, cancellationToken);

        return row == null ? NotFound("Bayi mağazası bulunamadı.") : Ok(row);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BayiMagazaSaveRequest request, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_MusteriMagaza_Guncelle", request.ToUpdateParameters(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/durum")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] StatusRequest request, CancellationToken cancellationToken)
    {
        await _dataService.ExecuteAsync("sp_MusteriMagaza_DurumGuncelle", new[]
        {
            SqlDataService.Param("@CustomerStoreId", SqlDbType.Int, id),
            SqlDataService.Param("@IsActive", SqlDbType.Bit, request.AktifMi)
        }, cancellationToken);

        return NoContent();
    }
}

public sealed class BayiSaveRequest
{
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string AuthorizedPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CustomerType { get; set; } = "Toptan";
    public bool IsActive { get; set; } = true;

    public SqlParameter[] ToInsertParameters()
    {
        return CommonParameters().ToArray();
    }

    public SqlParameter[] ToUpdateParameters(int customerId)
    {
        var parameters = new List<SqlParameter>
        {
            SqlDataService.Param("@CustomerId", SqlDbType.Int, customerId)
        };
        parameters.AddRange(CommonParameters());
        return parameters.ToArray();
    }

    private IEnumerable<SqlParameter> CommonParameters()
    {
        yield return SqlDataService.TextParam("@FullName", 300, FullName);
        yield return SqlDataService.NullableTextParam("@CompanyName", 300, CompanyName);
        yield return SqlDataService.NullableTextParam("@AuthorizedPerson", 200, AuthorizedPerson);
        yield return SqlDataService.NullableTextParam("@Phone", 60, Phone);
        yield return SqlDataService.NullableTextParam("@Email", 400, Email);
        yield return SqlDataService.NullableTextParam("@TaxNumber", 50, TaxNumber);
        yield return SqlDataService.NullableTextParam("@TaxOffice", 100, TaxOffice);
        yield return SqlDataService.NullableTextParam("@Address", 500, Address);
        yield return SqlDataService.TextParam("@CustomerType", 50, CustomerType);
        yield return SqlDataService.Param("@IsActive", SqlDbType.Bit, IsActive);
    }
}

public sealed class BayiMagazaSaveRequest
{
    public string StoreName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public SqlParameter[] ToInsertParameters(int customerId)
    {
        var parameters = new List<SqlParameter>
        {
            SqlDataService.Param("@CustomerId", SqlDbType.Int, customerId)
        };
        parameters.AddRange(CommonParameters());
        return parameters.ToArray();
    }

    public SqlParameter[] ToUpdateParameters(int customerStoreId)
    {
        var parameters = new List<SqlParameter>
        {
            SqlDataService.Param("@CustomerStoreId", SqlDbType.Int, customerStoreId)
        };
        parameters.AddRange(CommonParameters());
        return parameters.ToArray();
    }

    private IEnumerable<SqlParameter> CommonParameters()
    {
        yield return SqlDataService.TextParam("@StoreName", 300, StoreName);
        yield return SqlDataService.NullableTextParam("@City", 100, City);
        yield return SqlDataService.NullableTextParam("@District", 100, District);
        yield return SqlDataService.NullableTextParam("@Address", 500, Address);
        yield return SqlDataService.NullableTextParam("@Phone", 60, Phone);
        yield return SqlDataService.NullableTextParam("@ResponsiblePerson", 200, ResponsiblePerson);
        yield return SqlDataService.Param("@IsActive", SqlDbType.Bit, IsActive);
    }
}

public sealed class StatusRequest
{
    public bool AktifMi { get; set; }
}
