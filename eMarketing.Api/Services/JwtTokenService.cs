using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using eMarketing.Service.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace eMarketing.Api.Services;

public interface IJwtTokenService
{
    LoginResponse CreateLoginResponse(KullaniciDto kullanici);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LoginResponse CreateLoginResponse(KullaniciDto kullanici)
    {
        int expireMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out int value)
            ? value
            : 480;

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);
        string normalizedRole = NormalizeRole(kullanici.Rol);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, kullanici.KullaniciId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
            new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
            new Claim(ClaimTypes.GivenName, kullanici.AdSoyad),
            new Claim(ClaimTypes.Role, kullanici.Rol),
            new Claim(ClaimTypes.Role, normalizedRole),
            new Claim("role", normalizedRole)
        };

        string key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key ayarı bulunamadı.");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt,
            Kullanici = kullanici
        };
    }

    private static string NormalizeRole(string role)
    {
        return role switch
        {
            "StoreManager" => "Yonetici",
            "SalesPerson" => "MagazaYetkilisi",
            _ => role
        };
    }
}
