namespace eMarketing.Service.Dtos;

public sealed class LoginRequest
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
}

public sealed class KullaniciDto
{
    public int KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public KullaniciDto Kullanici { get; set; } = new();
}

public sealed class CurrentUserDto
{
    public int? KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool AdminMi { get; set; }
    public bool TumMagazalariGorebilir { get; set; }
}
