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
