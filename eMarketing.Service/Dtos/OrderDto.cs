namespace eMarketing.Service.Dtos;

public sealed class OrderDto
{
    public int SiparisId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public int? CustomerStoreId { get; set; }
    public int? BayiYetkiliId { get; set; }
    public string MusteriAdi { get; set; } = string.Empty;
    public string MusteriEmail { get; set; } = string.Empty;
    public string MusteriTelefon { get; set; } = string.Empty;
    public string MagazaAdi { get; set; } = string.Empty;
    public string Sehir { get; set; } = string.Empty;
    public string Ilce { get; set; } = string.Empty;
    public string YetkiliAdi { get; set; } = string.Empty;
    public string YetkiliTelefon { get; set; } = string.Empty;
    public string YetkiliEmail { get; set; } = string.Empty;
    public string GorselUrl { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Adet { get; set; }
    public int UrunKalemi { get; set; }
    public int BayiStok { get; set; }
    public decimal ToplamTutar { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string OrderSource { get; set; } = string.Empty;
    public string SiparisDurumu { get; set; } = string.Empty;
    public DateTime? SiparisTarihi { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsArchived { get; set; }
}

public sealed class OrderCreateRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public int? CustomerStoreId { get; set; }
    public string OrderType { get; set; } = "Bayi";
    public string OrderSource { get; set; } = "AdminPanel";
    public int? BayiYetkiliId { get; set; }
}

public sealed class OrderStatusUpdateRequest
{
    public string SiparisDurumu { get; set; } = string.Empty;
}
