using System.ComponentModel.DataAnnotations;

namespace eMarketing.Service.Dtos;

public sealed class BayiYetkiliDto
{
    public int BayiYetkiliId { get; set; }
    public int KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string RolGorunenAd { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int BayiId { get; set; }
    public string BayiAdi { get; set; } = string.Empty;
    public int MagazaId { get; set; }
    public string MagazaAdi { get; set; } = string.Empty;
    public string Sehir { get; set; } = string.Empty;
    public string Ilce { get; set; } = string.Empty;
    public string YetkiTipi { get; set; } = string.Empty;
    public string YetkiTipiGorunenAd { get; set; } = string.Empty;
    public string Notlar { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
    public DateTime? OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public int SiparisSayisi { get; set; }
    public DateTime? SonSiparisTarihi { get; set; }
}

public sealed class BayiYetkiliFilterRequest
{
    public string Arama { get; set; } = string.Empty;
    public int Durum { get; set; } = -1;
    public int? BayiId { get; set; }
    public int? MagazaId { get; set; }
}

public sealed class BayiYetkiliSaveRequest
{
    public int? BayiYetkiliId { get; set; }

    [Range(1, int.MaxValue)]
    public int KullaniciId { get; set; }

    [Range(1, int.MaxValue)]
    public int BayiId { get; set; }

    [Range(1, int.MaxValue)]
    public int MagazaId { get; set; }

    [Required, StringLength(50)]
    public string YetkiTipi { get; set; } = "SiparisYetkilisi";

    [StringLength(500)]
    public string Notlar { get; set; } = string.Empty;

    public bool AktifMi { get; set; } = true;
}

public sealed class BayiYetkiliStatusRequest
{
    public bool AktifMi { get; set; }
}

public sealed class SiparisYetkilisiDto
{
    public int BayiYetkiliId { get; set; }
    public int KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string RolGorunenAd { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string YetkiTipi { get; set; } = string.Empty;
    public string YetkiTipiGorunenAd { get; set; } = string.Empty;
}
