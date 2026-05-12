using System.ComponentModel.DataAnnotations;

namespace eMarketing.Service.Dtos;

public sealed class PersonnelDto
{
    public int KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
    public DateTime? OlusturmaTarihi { get; set; }
    public int MagazaSayisi { get; set; }
    public string AktifMagazaGorev { get; set; } = string.Empty;
    public string AktifMagazaGorevGorunenAd { get; set; } = string.Empty;
    public bool SiparisYetkilisiMi { get; set; }
    public int SiparisYetkiliMagazaSayisi { get; set; }
}

public sealed class PersonnelListItemDto
{
    public int KullaniciId { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
    public int MagazaSayisi { get; set; }
    public string AktifMagazaGorev { get; set; } = string.Empty;
    public string AktifMagazaGorevGorunenAd { get; set; } = string.Empty;
    public bool SiparisYetkilisiMi { get; set; }
    public int SiparisYetkiliMagazaSayisi { get; set; }
}

public sealed class PersonnelStorePermissionDto
{
    public int? KullaniciMagazaId { get; set; }
    public int? KullaniciId { get; set; }
    public int MagazaId { get; set; }
    public string Gorev { get; set; } = "Personel";
    public string GorevGorunenAd { get; set; } = "Personel";
    public int MusteriId { get; set; }
    public string MusteriAdi { get; set; } = string.Empty;
    public string MagazaAdi { get; set; } = string.Empty;
    public string Sehir { get; set; } = string.Empty;
    public string Ilce { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string SorumluKisi { get; set; } = string.Empty;
    public int SiparisSayisi { get; set; }
    public decimal ToplamCiro { get; set; }
    public int? BayiYetkiliId { get; set; }
    public bool SiparisYetkilisiMi { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime? OlusturmaTarihi { get; set; }
}

public class CreatePersonnelRequest
{
    public int? KullaniciId { get; set; }

    [Required, StringLength(100)]
    public string KullaniciAdi { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 4)]
    public string Sifre { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string AdSoyad { get; set; } = string.Empty;

    [StringLength(60)]
    public string Telefon { get; set; } = string.Empty;

    [EmailAddress, StringLength(400)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required, RegularExpression("^(Admin|Personel|Yonetici|MagazaYetkilisi|StoreManager|SalesPerson)$")]
    public string Rol { get; set; } = string.Empty;

    public bool AktifMi { get; set; } = true;
}

public sealed class UpdatePersonnelRequest : CreatePersonnelRequest;

public sealed class PersonnelFilterRequest
{
    public string Arama { get; set; } = string.Empty;
    public bool SadeceAktif { get; set; }
    public int? MagazaId { get; set; }
}

public sealed class AssignPersonnelStoreRequest
{
    [Required, RegularExpression("^(MagazaMuduru|Supervisor|Personel)$")]
    public string Gorev { get; set; } = "Personel";
}

public sealed class UpdatePersonnelStoreDutyRequest
{
    [Required, RegularExpression("^(MagazaMuduru|Supervisor|Personel)$")]
    public string Gorev { get; set; } = "Personel";
}
