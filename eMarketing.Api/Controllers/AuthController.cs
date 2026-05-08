using eMarketing.Api.Services;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.KullaniciAdi) || string.IsNullOrWhiteSpace(request.Sifre))
            return BadRequest("Kullanıcı adı ve şifre zorunludur.");

        KullaniciDto? kullanici = await _authService.LoginAsync(request.KullaniciAdi, request.Sifre, cancellationToken);

        if (kullanici == null)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        return Ok(_jwtTokenService.CreateLoginResponse(kullanici));
    }
}
