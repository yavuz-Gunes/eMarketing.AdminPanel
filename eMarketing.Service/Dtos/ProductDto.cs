namespace eMarketing.Service.Dtos;

public sealed class ProductDto
{
    public int UrunId { get; set; }
    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public int Stok { get; set; }
    public string GorselUrl { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
    public string StokDurumu { get; set; } = string.Empty;
    public DateTime? OlusturmaTarihi { get; set; }
}

public sealed class ProductSaveRequest
{
    public int KategoriId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public int Stok { get; set; }
    public string GorselUrl { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}
