using System.Text.Json;
using eMarketing.Service.Dtos;
using Microsoft.AspNetCore.Http;

namespace eMarketing.Web.State;

public sealed class AuthSession
{
    public const string StorageKey = "emarketing.auth";
    public const string CookieName = "emarketing.auth";

    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public KullaniciDto? User { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && ExpiresAt > DateTime.UtcNow;
    public bool IsAdmin => IsAuthenticated && string.Equals(User?.Rol, "Admin", StringComparison.OrdinalIgnoreCase);
    public bool CanManageCampaigns =>
        IsAuthenticated &&
        (string.Equals(User?.Rol, "Admin", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(User?.Rol, "Yonetici", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(User?.Rol, "Yönetici", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(User?.Rol, "StoreManager", StringComparison.OrdinalIgnoreCase));

    public bool CanManageNotifications => CanManageCampaigns;
    public bool CanManageStock => CanManageCampaigns;
    public bool CanManageOrders => CanManageCampaigns;
    public bool CanManageProducts => CanManageCampaigns;
    public bool CanManagePersonnel => CanManageCampaigns;
    public bool CanManageDealers => CanManageCampaigns;

    public event Action? Changed;

    public AuthSession(IHttpContextAccessor httpContextAccessor)
    {
        RestoreFromCookie(httpContextAccessor.HttpContext?.Request.Cookies);
    }

    public void Set(LoginResponse response)
    {
        Token = response.Token;
        ExpiresAt = response.ExpiresAt;
        User = response.Kullanici;
        Changed?.Invoke();
    }

    public StoredAuthSession ToStoredSession()
    {
        return new StoredAuthSession
        {
            Token = Token,
            ExpiresAt = ExpiresAt,
            User = User
        };
    }

    public void Clear()
    {
        Token = string.Empty;
        ExpiresAt = default;
        User = null;
        Changed?.Invoke();
    }

    private void RestoreFromCookie(IRequestCookieCollection? cookies)
    {
        if (cookies == null || !cookies.TryGetValue(CookieName, out string? rawValue) || string.IsNullOrWhiteSpace(rawValue))
            return;

        try
        {
            string json = Uri.UnescapeDataString(rawValue);
            StoredAuthSession? stored = JsonSerializer.Deserialize<StoredAuthSession>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (stored == null || string.IsNullOrWhiteSpace(stored.Token) || stored.ExpiresAt <= DateTime.UtcNow)
                return;

            Token = stored.Token;
            ExpiresAt = stored.ExpiresAt;
            User = stored.User;
        }
        catch
        {
            Clear();
        }
    }
}

public sealed class StoredAuthSession
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public KullaniciDto? User { get; set; }
}
