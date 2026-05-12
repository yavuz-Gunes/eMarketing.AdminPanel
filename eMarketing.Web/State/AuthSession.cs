using eMarketing.Service.Dtos;

namespace eMarketing.Web.State;

public sealed class AuthSession
{
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public KullaniciDto? User { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && ExpiresAt > DateTime.UtcNow;

    public event Action? Changed;

    public void Set(LoginResponse response)
    {
        Token = response.Token;
        ExpiresAt = response.ExpiresAt;
        User = response.Kullanici;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Token = string.Empty;
        ExpiresAt = default;
        User = null;
        Changed?.Invoke();
    }
}
