using System.ComponentModel.DataAnnotations;

namespace eMarketing.Service.Dtos;

public sealed class DealerDetailDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string AuthorizedPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int StoreCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public sealed class DealerStoreDto
{
    public int CustomerStoreId { get; set; }
    public int CustomerId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public int? SorumluKullaniciId { get; set; }
    public string SorumluKisi { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class DealerSaveRequest
{
    [Required, StringLength(300)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(300)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string AuthorizedPerson { get; set; } = string.Empty;

    [StringLength(60)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(400)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string TaxNumber { get; set; } = string.Empty;

    [StringLength(100)]
    public string TaxOffice { get; set; } = string.Empty;

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(50)]
    public string CustomerType { get; set; } = "Toptan";

    public bool IsActive { get; set; } = true;
}

public sealed class DealerStoreSaveRequest
{
    [Required, StringLength(300)]
    public string StoreName { get; set; } = string.Empty;

    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(60)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string ResponsiblePerson { get; set; } = string.Empty;

    public int? SorumluKullaniciId { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class DealerStatusRequest
{
    public bool AktifMi { get; set; }
}
