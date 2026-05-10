using eMarketing.Api.Services;
using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.KullaniciAdi) || string.IsNullOrWhiteSpace(request.Sifre))
            return BadRequest("Kullanıcı adı ve şifre zorunludur.");

        KullaniciDto? kullanici = await _authService.LoginAsync(request.KullaniciAdi, request.Sifre, cancellationToken);

        if (kullanici == null)
        {
            _logger.LogWarning("Login failed for username {Username}", request.KullaniciAdi);
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }

        _logger.LogInformation("Login succeeded for user {UserId} ({Username})", kullanici.KullaniciId, kullanici.KullaniciAdi);
        return Ok(_jwtTokenService.CreateLoginResponse(kullanici));
    }
}
