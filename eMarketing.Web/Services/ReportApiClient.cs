using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class ReportApiClient : ApiClientBase
{
    public ReportApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<PortalReportDto> GetPortalReportAsync(int? storeId, bool allStores = false, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        string query = $"raporlar/portal?{BuildPortalReportQuery(storeId, allStores, startDate, endDate)}";

        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<PortalReportDto>(response, cancellationToken);
    }

    public async Task<ReportExportResultDto> DownloadPortalReportPdfAsync(int? storeId, bool allStores = false, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        string query = $"raporlar/portal/pdf?{BuildPortalReportQuery(storeId, allStores, startDate, endDate)}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));

        return new ReportExportResultDto
        {
            FileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                ?? $"rapor-{DateTime.Now:yyyyMMdd-HHmm}.pdf",
            ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf",
            Content = await response.Content.ReadAsByteArrayAsync(cancellationToken)
        };
    }

    private static string BuildPortalReportQuery(int? storeId, bool allStores, DateTime? startDate, DateTime? endDate)
    {
        var queryParts = new List<string>
        {
            $"tumMagazalar={allStores.ToString().ToLowerInvariant()}"
        };

        if (storeId.HasValue)
            queryParts.Add($"magazaId={storeId.Value}");

        if (startDate.HasValue)
            queryParts.Add($"startDate={startDate.Value:yyyy-MM-dd}");

        if (endDate.HasValue)
            queryParts.Add($"endDate={endDate.Value:yyyy-MM-dd}");

        return string.Join("&", queryParts);
    }
}
