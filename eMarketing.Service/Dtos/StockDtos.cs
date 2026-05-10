using System.ComponentModel.DataAnnotations;

namespace eMarketing.Service.Dtos;

public sealed class StockItemDto
{
    public int MagazaStokId { get; set; }
    public int MagazaId { get; set; }
    public int MusteriId { get; set; }
    public string MusteriAdi { get; set; } = string.Empty;
    public string MagazaAdi { get; set; } = string.Empty;
    public string Sehir { get; set; } = string.Empty;
    public string Ilce { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string SorumluKisi { get; set; } = string.Empty;
    public int UrunId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public int MerkezStok { get; set; }
    public string GorselUrl { get; set; } = string.Empty;
    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; } = string.Empty;
    public int BayiStok { get; set; }
    public int MinimumStok { get; set; }
    public string StokDurumu { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
    public DateTime? OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public DateTime? SonHareketTarihi { get; set; }
    public DateTime? SonGirisTarihi { get; set; }
    public DateTime? SonCikisTarihi { get; set; }
}

public sealed class StockMovementDto
{
    public int MagazaStokHareketId { get; set; }
    public int MagazaId { get; set; }
    public int UrunId { get; set; }
    public string HareketTipi { get; set; } = string.Empty;
    public string HareketYonu { get; set; } = string.Empty;
    public string HareketAciklama { get; set; } = string.Empty;
    public int Miktar { get; set; }
    public int OncekiStok { get; set; }
    public int SonrakiStok { get; set; }
    public int? KaynakSiparisId { get; set; }
    public string SiparisNo { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public DateTime? OlusturmaTarihi { get; set; }
}

public sealed class StockSummaryDto
{
    public int ToplamStokKarti { get; set; }
    public int StokluMagazaSayisi { get; set; }
    public int StokluUrunSayisi { get; set; }
    public int ToplamBayiStok { get; set; }
    public int KritikStokKarti { get; set; }
    public int TukenmisStokKarti { get; set; }
}

public sealed class StockOperationRequest
{
    [Range(1, int.MaxValue)]
    public int MagazaId { get; set; }

    [Range(1, int.MaxValue)]
    public int UrunId { get; set; }

    [Required, StringLength(50)]
    public string HareketTipi { get; set; } = "ManuelGiris";

    [Range(1, int.MaxValue)]
    public int Miktar { get; set; }

    [StringLength(500)]
    public string Aciklama { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int? MinimumStok { get; set; }
}

public sealed class StockFilterRequest
{
    public int? MagazaId { get; set; }
    public string Arama { get; set; } = string.Empty;
    public bool SadeceStokta { get; set; }
    public bool SadeceKritik { get; set; }
    public bool SadeceAktif { get; set; } = true;
}
