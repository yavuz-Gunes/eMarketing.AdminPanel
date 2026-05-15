using System.ComponentModel.DataAnnotations;

namespace eMarketing.Service.Dtos;

public sealed class CampaignDto
{
    public int KampanyaId { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string DetayMetni { get; set; } = string.Empty;
    public string KatilimSartlari { get; set; } = string.Empty;
    public string AdminNotlari { get; set; } = string.Empty;
    public string GorselUrl { get; set; } = string.Empty;
    public string CtaMetni { get; set; } = "Detaya Git";
    public string HedefUrl { get; set; } = "/products";
    public int? HedefUrunId { get; set; }
    public int? HedefKategoriId { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int Oncelik { get; set; }
    public bool AktifMi { get; set; }
    public bool GlobalMi { get; set; }
    public string Kapsam { get; set; } = "Global";
    public string BayiIds { get; set; } = string.Empty;
    public string BayiAdlari { get; set; } = string.Empty;
    public string MagazaIds { get; set; } = string.Empty;
    public string MagazaAdlari { get; set; } = string.Empty;
    public DateTime? OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
}

public sealed class CampaignSaveRequest
{
    [Required, StringLength(200)]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(500)]
    public string Aciklama { get; set; } = string.Empty;

    public string DetayMetni { get; set; } = string.Empty;
    public string KatilimSartlari { get; set; } = string.Empty;
    public string AdminNotlari { get; set; } = string.Empty;

    [StringLength(500)]
    public string GorselUrl { get; set; } = string.Empty;

    [StringLength(80)]
    public string CtaMetni { get; set; } = "Detaya Git";

    [StringLength(500)]
    public string HedefUrl { get; set; } = "/products";

    public int? HedefUrunId { get; set; }
    public int? HedefKategoriId { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }

    [Range(0, int.MaxValue)]
    public int Oncelik { get; set; } = 10;

    public bool AktifMi { get; set; } = true;
    public bool GlobalMi { get; set; } = true;
    public IReadOnlyList<int> BayiIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<int> MagazaIds { get; set; } = Array.Empty<int>();
}
