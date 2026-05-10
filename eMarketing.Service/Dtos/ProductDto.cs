namespace eMarketing.Service.Dtos;

using System.ComponentModel.DataAnnotations;

public class ProductDto
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

public class ProductSaveRequest
{
    [Range(1, int.MaxValue)]
    public int KategoriId { get; set; }

    [Required, StringLength(200)]
    public string UrunAdi { get; set; } = string.Empty;

    public string Aciklama { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Fiyat { get; set; }

    [Range(0, int.MaxValue)]
    public int Stok { get; set; }

    public string GorselUrl { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}

public sealed class ProductListItemDto : ProductDto;
public sealed class CreateProductRequest : ProductSaveRequest;
public sealed class UpdateProductRequest : ProductSaveRequest;

public sealed class ProductFilterRequest
{
    public string Arama { get; set; } = string.Empty;
    public int Durum { get; set; } = 1;
    public int KategoriId { get; set; }
}
