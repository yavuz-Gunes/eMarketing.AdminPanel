namespace eMarketing.Service.Dtos;

public sealed class ReportSummaryDto
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public IReadOnlyDictionary<string, object?> Values { get; set; } = new Dictionary<string, object?>();
}

public sealed class ReportRequest
{
    public string ReportType { get; set; } = "summary";
    public int? MagazaId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public sealed class ReportExportResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

public sealed class PortalReportDto
{
    public string Scope { get; set; } = "Personel";
    public string Title { get; set; } = "Raporlar";
    public string StoreName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReportMetricDto[] Metrics { get; set; } = [];
    public ReportPersonPerformanceDto[] Personnel { get; set; } = [];
    public ReportProductPerformanceDto[] Products { get; set; } = [];
    public ReportCategoryPerformanceDto[] Categories { get; set; } = [];
    public ReportStockRiskDto[] StockRisks { get; set; } = [];
    public ReportOrderRowDto[] RecentOrders { get; set; } = [];
}

public sealed class ReportMetricDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string DisplayValue { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Tone { get; set; } = "slate";
}

public sealed class ReportPersonPerformanceDto
{
    public int KullaniciId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string MagazaAdi { get; set; } = string.Empty;
    public int SiparisSayisi { get; set; }
    public int UrunAdedi { get; set; }
    public decimal ToplamTutar { get; set; }
}

public sealed class ReportProductPerformanceDto
{
    public int UrunId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public string KategoriAdi { get; set; } = string.Empty;
    public int SatisAdedi { get; set; }
    public int SiparisSayisi { get; set; }
    public decimal ToplamTutar { get; set; }
    public int BayiStok { get; set; }
    public bool KritikMi { get; set; }
    public bool PasifMi { get; set; }
}

public sealed class ReportCategoryPerformanceDto
{
    public string KategoriAdi { get; set; } = string.Empty;
    public int SatisAdedi { get; set; }
    public int SiparisSayisi { get; set; }
    public decimal ToplamTutar { get; set; }
}

public sealed class ReportStockRiskDto
{
    public int UrunId { get; set; }
    public string UrunAdi { get; set; } = string.Empty;
    public string KategoriAdi { get; set; } = string.Empty;
    public int BayiStok { get; set; }
    public int MerkezStok { get; set; }
    public int MinimumStok { get; set; }
    public string Durum { get; set; } = string.Empty;
    public bool AktifMi { get; set; }
}

public sealed class ReportOrderRowDto
{
    public int SiparisId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string MagazaAdi { get; set; } = string.Empty;
    public string YetkiliAdi { get; set; } = string.Empty;
    public string UrunAdi { get; set; } = string.Empty;
    public int Adet { get; set; }
    public decimal ToplamTutar { get; set; }
    public string SiparisDurumu { get; set; } = string.Empty;
    public DateTime? SiparisTarihi { get; set; }
}
