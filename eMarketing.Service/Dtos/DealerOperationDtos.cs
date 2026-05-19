using System.ComponentModel.DataAnnotations;

namespace eMarketing.Service.Dtos;

public sealed class StoreTeamMemberDto
{
    public int KullaniciId { get; set; }
    public int KullaniciMagazaId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string MagazaGorev { get; set; } = "Personel";
    public string MagazaGorevGorunenAd { get; set; } = "Personel";
    public bool SiparisYetkilisiMi { get; set; }
    public int? BayiYetkiliId { get; set; }
    public bool AktifMi { get; set; }
}

public sealed class StoreTeamContextDto
{
    public int MagazaId { get; set; }
    public string MagazaAdi { get; set; } = string.Empty;
    public string MusteriAdi { get; set; } = string.Empty;
    public string KullaniciGorev { get; set; } = "Personel";
    public bool SupervisorMu => string.Equals(KullaniciGorev, "Supervisor", StringComparison.OrdinalIgnoreCase);
    public bool MagazaMuduruMu => string.Equals(KullaniciGorev, "MagazaMuduru", StringComparison.OrdinalIgnoreCase);
    public bool SiparisYetkilisiMi { get; set; }
    public bool PersonelEkleyebilir => SupervisorMu;
    public bool GorevYonetebilir => SupervisorMu;
    public bool SiparisYetkisiYonetebilir => SupervisorMu || MagazaMuduruMu;
}

public sealed class StoreTeamResponseDto
{
    public StoreTeamContextDto Context { get; set; } = new();
    public IReadOnlyList<StoreTeamMemberDto> Members { get; set; } = Array.Empty<StoreTeamMemberDto>();
}

public sealed class StorePersonnelCreateRequest
{
    [Required, StringLength(100)]
    public string KullaniciAdi { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 4)]
    public string Sifre { get; set; } = "1234";

    [Required, StringLength(150)]
    public string AdSoyad { get; set; } = string.Empty;

    [EmailAddress, StringLength(400)]
    public string Email { get; set; } = string.Empty;

    [StringLength(60)]
    public string Telefon { get; set; } = string.Empty;

    [Required, RegularExpression("^(MagazaMuduru|Supervisor|Personel)$")]
    public string Gorev { get; set; } = "Personel";
}

public sealed class StoreDutyUpdateRequest
{
    [Required, RegularExpression("^(MagazaMuduru|Supervisor|Personel)$")]
    public string Gorev { get; set; } = "Personel";
}

public sealed class StoreOrderAuthorityUpdateRequest
{
    public bool AktifMi { get; set; }
    [StringLength(500)]
    public string Notlar { get; set; } = string.Empty;
}
