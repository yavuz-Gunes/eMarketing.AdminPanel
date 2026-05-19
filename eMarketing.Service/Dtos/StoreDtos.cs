namespace eMarketing.Service.Dtos;

public sealed class StoreDto
{
    public int MusteriId { get; set; }
    public int MagazaId { get; set; }
    public string MusteriAdi { get; set; } = string.Empty;
    public string MagazaAdi { get; set; } = string.Empty;
    public string Sehir { get; set; } = string.Empty;
    public string Ilce { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public int? SorumluKullaniciId { get; set; }
    public string SorumluKisi { get; set; } = string.Empty;
    public string MagazaMuduru { get; set; } = string.Empty;
    public string Supervisor { get; set; } = string.Empty;
    public int PersonelSayisi { get; set; }
    public int SiparisYetkilisiSayisi { get; set; }
    public string MusteriTipi { get; set; } = string.Empty;
    public bool MusteriAktifMi { get; set; }
    public bool MagazaAktifMi { get; set; }
    public int SiparisSayisi { get; set; }
    public decimal ToplamCiro { get; set; }
    public DateTime? SonSiparisTarihi { get; set; }
}

public sealed class DealerDto
{
    public int MusteriId { get; set; }
    public string MusteriAdi { get; set; } = string.Empty;
}

public sealed class StoreContextDto
{
    public int? KullaniciId { get; set; }
    public bool AdminMi { get; set; }
}
