using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bayi-operasyon")]
[Authorize]
public sealed class BayiOperasyonController : ControllerBase
{
    private readonly IDealerOperationService _dealerOperationService;

    public BayiOperasyonController(IDealerOperationService dealerOperationService)
    {
        _dealerOperationService = dealerOperationService;
    }

    [HttpGet("magazalar/{magazaId:int}/ekip")]
    public async Task<ActionResult<StoreTeamResponseDto>> GetTeam(int magazaId, CancellationToken cancellationToken)
    {
        return Ok(await _dealerOperationService.GetTeamAsync(magazaId, cancellationToken));
    }

    [HttpPost("magazalar/{magazaId:int}/personel")]
    public async Task<ActionResult<object>> CreatePersonnel(int magazaId, [FromBody] StorePersonnelCreateRequest request, CancellationToken cancellationToken)
    {
        int userId = await _dealerOperationService.CreatePersonnelAsync(magazaId, request, cancellationToken);
        return Ok(new { KullaniciId = userId });
    }

    [HttpPatch("magazalar/{magazaId:int}/personel/{kullaniciMagazaId:int}/gorev")]
    public async Task<IActionResult> UpdateDuty(int magazaId, int kullaniciMagazaId, [FromBody] StoreDutyUpdateRequest request, CancellationToken cancellationToken)
    {
        await _dealerOperationService.UpdateDutyAsync(magazaId, kullaniciMagazaId, request.Gorev, cancellationToken);
        return NoContent();
    }

    [HttpPatch("magazalar/{magazaId:int}/personel/{kullaniciId:int}/siparis-yetkisi")]
    public async Task<IActionResult> SetOrderAuthority(int magazaId, int kullaniciId, [FromBody] StoreOrderAuthorityUpdateRequest request, CancellationToken cancellationToken)
    {
        await _dealerOperationService.SetOrderAuthorityAsync(magazaId, kullaniciId, request.AktifMi, request.Notlar, cancellationToken);
        return NoContent();
    }

    [HttpDelete("magazalar/{magazaId:int}/personel/{kullaniciMagazaId:int}")]
    public async Task<IActionResult> RemovePersonnel(int magazaId, int kullaniciMagazaId, CancellationToken cancellationToken)
    {
        await _dealerOperationService.RemovePersonnelAsync(magazaId, kullaniciMagazaId, cancellationToken);
        return NoContent();
    }

    [HttpPatch("personel/{kullaniciId:int}/profil")]
    public async Task<IActionResult> UpdateProfile(int kullaniciId, [FromBody] StorePersonnelProfileUpdateRequest request, CancellationToken cancellationToken)
    {
        await _dealerOperationService.UpdateProfileAsync(kullaniciId, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("personel/{kullaniciId:int}/sifre")]
    public async Task<IActionResult> UpdatePassword(int kullaniciId, [FromBody] StorePersonnelPasswordUpdateRequest request, CancellationToken cancellationToken)
    {
        await _dealerOperationService.UpdatePasswordAsync(kullaniciId, request, cancellationToken);
        return NoContent();
    }
}
