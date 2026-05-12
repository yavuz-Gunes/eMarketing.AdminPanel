using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/personel")]
[Authorize]
public sealed class PersonelController : ControllerBase
{
    private readonly IPersonnelService _personnelService;

    public PersonelController(IPersonnelService personnelService)
    {
        _personnelService = personnelService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewPersonnel")]
    public async Task<ActionResult<IReadOnlyList<PersonnelDto>>> Get([FromQuery] string arama = "", [FromQuery] bool sadeceAktif = false, [FromQuery] int? magazaId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _personnelService.GetPersonnelAsync(new PersonnelFilterRequest
        {
            Arama = arama,
            SadeceAktif = sadeceAktif,
            MagazaId = magazaId
        }, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "CanManagePersonnel")]
    public async Task<ActionResult<object>> Save([FromBody] PersonelSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _personnelService.SavePersonnelAsync(new CreatePersonnelRequest
        {
            KullaniciId = request.KullaniciId,
            KullaniciAdi = request.KullaniciAdi,
            Sifre = request.Sifre,
            AdSoyad = request.AdSoyad,
            Telefon = request.Telefon,
            Email = request.Email,
            ImageUrl = request.ImageUrl,
            Rol = request.Rol,
            AktifMi = request.AktifMi
        }, cancellationToken);

        return Ok(new { KullaniciId = id });
    }

    [HttpGet("{kullaniciId:int}/magazalar")]
    [Authorize(Policy = "CanViewPersonnel")]
    public async Task<ActionResult<IReadOnlyList<PersonnelStorePermissionDto>>> GetMagazalar(int kullaniciId, CancellationToken cancellationToken = default)
    {
        return Ok(await _personnelService.GetStoresAsync(kullaniciId, cancellationToken));
    }

    [HttpGet("{kullaniciId:int}/atanabilir-magazalar")]
    [Authorize(Policy = "CanManagePersonnel")]
    public async Task<ActionResult<IReadOnlyList<PersonnelStorePermissionDto>>> GetAtanabilirMagazalar(int kullaniciId, [FromQuery] string arama = "", CancellationToken cancellationToken = default)
    {
        return Ok(await _personnelService.GetAssignableStoresAsync(kullaniciId, arama, cancellationToken));
    }

    [HttpPost("{kullaniciId:int}/magazalar/{magazaId:int}")]
    [Authorize(Policy = "CanManagePersonnel")]
    public async Task<IActionResult> MagazaAta(int kullaniciId, int magazaId, [FromBody] AssignPersonnelStoreRequest? request, CancellationToken cancellationToken)
    {
        await _personnelService.AssignStoreAsync(kullaniciId, magazaId, request?.Gorev ?? "Personel", cancellationToken);
        return NoContent();
    }

    [HttpPatch("magaza-yetkileri/{kullaniciMagazaId:int}/gorev")]
    [Authorize(Policy = "CanManagePersonnel")]
    public async Task<IActionResult> MagazaGorevGuncelle(int kullaniciMagazaId, [FromBody] UpdatePersonnelStoreDutyRequest request, CancellationToken cancellationToken)
    {
        await _personnelService.UpdateStoreDutyAsync(kullaniciMagazaId, request.Gorev, cancellationToken);
        return NoContent();
    }

    [HttpDelete("magaza-yetkileri/{kullaniciMagazaId:int}")]
    [Authorize(Policy = "CanManagePersonnel")]
    public async Task<IActionResult> MagazaKaldir(int kullaniciMagazaId, CancellationToken cancellationToken)
    {
        await _personnelService.RemoveStoreAsync(kullaniciMagazaId, cancellationToken);
        return NoContent();
    }
}

public sealed class PersonelSaveRequest
{
    public int? KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}
