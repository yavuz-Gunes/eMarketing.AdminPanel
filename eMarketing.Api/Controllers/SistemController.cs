using eMarketing.Service.Connection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/sistem")]
public sealed class SistemController : ControllerBase
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SistemController(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    [HttpGet("durum")]
    public async Task<ActionResult<object>> GetDurum(CancellationToken cancellationToken)
    {
        try
        {
            await using SqlConnection connection = _connectionFactory.CreateConnection();
            await using SqlCommand command = new("SELECT 1", connection);

            await connection.OpenAsync(cancellationToken);
            await command.ExecuteScalarAsync(cancellationToken);

            return Ok(new
            {
                ApiDurumu = "Calisiyor",
                VeritabaniDurumu = "Baglandi",
                Saat = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                ApiDurumu = "Calisiyor",
                VeritabaniDurumu = "Baglanamadi",
                Hata = ex.Message,
                Saat = DateTime.Now
            });
        }
    }
}
