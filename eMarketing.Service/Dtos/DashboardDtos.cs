namespace eMarketing.Service.Dtos;

public sealed class DashboardSummaryDto
{
    public int ToplamUrun { get; set; }
    public int AktifUrun { get; set; }
    public int KritikStok { get; set; }
    public int ToplamKategori { get; set; }
    public int AktifKategori { get; set; }
    public int ToplamSiparis { get; set; }
    public int HazirlaniyorSayisi { get; set; }
    public int KargodaSayisi { get; set; }
    public int TeslimEdildiSayisi { get; set; }
    public int IptalSayisi { get; set; }
    public int BekleyenOdemeSayisi { get; set; }
    public int ToplamMusteri { get; set; }
    public int AktifMagaza { get; set; }
    public int PersonelSayisi { get; set; }
    public decimal ToplamCiro { get; set; }
    public decimal BugunkuCiro { get; set; }
    public decimal AylikCiro { get; set; }
}

public sealed class DashboardRecentOrderDto
{
    public int SiparisId { get; set; }
    public string MusteriAdi { get; set; } = string.Empty;
    public string MagazaAdi { get; set; } = string.Empty;
    public string YetkiliAdi { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Adet { get; set; }
    public decimal ToplamTutar { get; set; }
    public string SiparisDurumu { get; set; } = string.Empty;
    public DateTime? SiparisTarihi { get; set; }
    public string OrderSource { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
}

public sealed class DashboardCriticalStockDto
{
    public int UrunId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public string KategoriAdi { get; set; } = string.Empty;
    public int Stok { get; set; }
    public decimal Fiyat { get; set; }
}

public sealed class DashboardCampaignDto
{
    public int KampanyaId { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string? GorselUrl { get; set; }
    public string CtaMetni { get; set; } = "Ürünlere Git";
    public string HedefUrl { get; set; } = "/products";
    public int Oncelik { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
}
