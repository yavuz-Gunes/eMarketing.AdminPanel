namespace eMarketing.Service.Dtos;

using System.ComponentModel.DataAnnotations;

public class OrderDto
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

public class OrderCreateRequest
{
    [Required, StringLength(300)]
    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int? CustomerStoreId { get; set; }

    public string OrderType { get; set; } = "Bayi";
    public string OrderSource { get; set; } = "AdminPanel";
    public int? BayiYetkiliId { get; set; }
}

public sealed class OrderCreateItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; set; }
}

public sealed class OrderDetailItemDto
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public string KategoriAdi { get; set; } = string.Empty;
    public string GorselUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OrderDetailResponseDto
{
    public OrderDto Order { get; set; } = new();
    public IReadOnlyList<OrderDetailItemDto> Items { get; set; } = Array.Empty<OrderDetailItemDto>();
}

public sealed class CartOrderCreateRequest
{
    [Required, StringLength(300)]
    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? CustomerStoreId { get; set; }

    public string OrderType { get; set; } = "Bayi";
    public string OrderSource { get; set; } = "Web";
    public int? BayiYetkiliId { get; set; }
    public IReadOnlyList<OrderCreateItemRequest> Items { get; set; } = Array.Empty<OrderCreateItemRequest>();
}

public class OrderStatusUpdateRequest
{
    [Required, StringLength(50)]
    public string SiparisDurumu { get; set; } = string.Empty;
}

public sealed class OrderListItemDto : OrderDto;
public sealed class OrderDetailDto : OrderDto;
public sealed class CreateOrderRequest : OrderCreateRequest;
public sealed class UpdateOrderStatusRequest : OrderStatusUpdateRequest;

public sealed class OrderFilterRequest
{
    public int? MagazaId { get; set; }
    public bool TumMagazalar { get; set; } = true;
}

public sealed class OrderSummaryDto
{
    public int ToplamSiparis { get; set; }
    public int HazirlaniyorSayisi { get; set; }
    public int KargodaSayisi { get; set; }
    public int TeslimEdildiSayisi { get; set; }
    public int IptalSayisi { get; set; }
}
