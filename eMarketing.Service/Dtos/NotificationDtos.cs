namespace eMarketing.Service.Dtos;

public sealed class NotificationDto
{
    public int BildirimId { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Mesaj { get; set; } = string.Empty;
    public string Tip { get; set; } = "Sistem";
    public string HedefUrl { get; set; } = string.Empty;
    public string HedefTipi { get; set; } = "Yok";
    public int? HedefId { get; set; }
    public int? KampanyaId { get; set; }
    public bool GlobalMi { get; set; }
    public bool AktifMi { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public DateTime? OlusturmaTarihi { get; set; }
    public bool OkunduMu { get; set; }
    public DateTime? OkunmaTarihi { get; set; }
    public string MagazaIds { get; set; } = string.Empty;
    public string MagazaAdlari { get; set; } = string.Empty;
}

public sealed class NotificationSaveRequest
{
    public string Baslik { get; set; } = string.Empty;
    public string Mesaj { get; set; } = string.Empty;
    public string Tip { get; set; } = "OzelMesaj";
    public string HedefUrl { get; set; } = string.Empty;
    public string HedefTipi { get; set; } = "Yok";
    public int? HedefId { get; set; }
    public bool GlobalMi { get; set; }
    public bool AktifMi { get; set; } = true;
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public IReadOnlyList<int> MagazaIds { get; set; } = Array.Empty<int>();
}

public sealed class NotificationCountDto
{
    public int OkunmamisSayisi { get; set; }
}
