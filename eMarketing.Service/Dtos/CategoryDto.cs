namespace eMarketing.Service.Dtos;

public sealed class CategoryDto
{
    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
    public DateTime? OlusturmaTarihi { get; set; }
}

public sealed class CategorySaveRequest
{
    public string KategoriAdi { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}
