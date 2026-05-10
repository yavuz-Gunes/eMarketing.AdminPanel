using eMarketing.Service.Dtos;
using eMarketing.Service.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public MeController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public ActionResult<CurrentUserDto> Get()
    {
        CurrentUser user = _currentUserService.CurrentUser;
        return Ok(new CurrentUserDto
        {
            KullaniciId = user.UserId,
            KullaniciAdi = user.Username,
            Rol = user.Role,
            AdminMi = user.IsAdmin,
            TumMagazalariGorebilir = user.CanSeeAllStores
        });
    }
}
