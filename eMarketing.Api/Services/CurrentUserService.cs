using System.Security.Claims;
using eMarketing.Service.Security;

namespace eMarketing.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser CurrentUser
    {
        get
        {
            ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return new CurrentUser();

            string? userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            string role = principal.FindFirstValue("role")
                ?? principal.FindFirstValue(ClaimTypes.Role)
                ?? string.Empty;

            return new CurrentUser
            {
                IsAuthenticated = true,
                UserId = int.TryParse(userIdValue, out int userId) ? userId : null,
                Username = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                Role = NormalizeRole(role)
            };
        }
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
