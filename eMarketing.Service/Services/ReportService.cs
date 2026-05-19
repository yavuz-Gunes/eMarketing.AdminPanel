using System.Data;
using System.Text;
using eMarketing.Service.Connection;
using eMarketing.Service.Dtos;
using eMarketing.Service.Security;
using Microsoft.Data.SqlClient;

namespace eMarketing.Service.Services;

public interface IReportService
{
    Task<PortalReportDto> GetPortalReportAsync(int? storeId, bool allStores, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<ReportExportResultDto> ExportPortalReportPdfAsync(int? storeId, bool allStores, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

public sealed class ReportService : IReportService
{
    private static readonly TimeSpan DefaultPeriod = TimeSpan.FromDays(30);

    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public ReportService(ISqlConnectionFactory connectionFactory, ICurrentUserService currentUserService)
    {
        _connectionFactory = connectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<PortalReportDto> GetPortalReportAsync(int? storeId, bool allStores, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        CurrentUser currentUser = _currentUserService.CurrentUser;
        if (!currentUser.UserId.HasValue)
            throw new UnauthorizedAccessException("Kullanıcı oturumu bulunamadı.");

        DateTime periodEnd = (endDate?.Date ?? DateTime.Today).AddDays(1);
        DateTime periodStart = startDate?.Date ?? periodEnd.Subtract(DefaultPeriod);
        if (periodStart >= periodEnd)
            throw new ArgumentException("Başlangıç tarihi bitiş tarihinden önce olmalıdır.");

        bool canSeeAllStores = currentUser.CanSeeAllStores || currentUser.IsManager;
        bool effectiveAllStores = allStores && canSeeAllStores;

        await using SqlConnection connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        int? effectiveStoreId = effectiveAllStores
            ? null
            : await ResolveEffectiveStoreIdAsync(connection, currentUser, canSeeAllStores, storeId, cancellationToken);

        ReportAggregate aggregate = await LoadAggregateAsync(connection, currentUser, effectiveStoreId, effectiveAllStores, periodStart, periodEnd, cancellationToken);
        StockAggregate stock = await LoadStockAggregateAsync(connection, currentUser, effectiveStoreId, effectiveAllStores, cancellationToken);
        string storeName = await ResolveStoreNameAsync(connection, effectiveStoreId, effectiveAllStores, cancellationToken);

        return new PortalReportDto
        {
            Scope = canSeeAllStores ? "Admin" : "Personel",
            Title = canSeeAllStores ? "Yönetici raporları" : "Personel raporum",
            StoreName = storeName,
            StartDate = periodStart,
            EndDate = periodEnd.AddDays(-1),
            Metrics = BuildMetrics(canSeeAllStores, aggregate, stock),
            Personnel = canSeeAllStores
                ? await LoadPersonnelAsync(connection, currentUser, effectiveStoreId, effectiveAllStores, periodStart, periodEnd, cancellationToken)
                : [],
            Products = await LoadProductsAsync(connection, currentUser, effectiveStoreId, effectiveAllStores, periodStart, periodEnd, cancellationToken),
            Categories = await LoadCategoriesAsync(connection, currentUser, effectiveStoreId, effectiveAllStores, periodStart, periodEnd, cancellationToken),
            StockRisks = await LoadStockRisksAsync(connection, currentUser, effectiveStoreId, effectiveAllStores, cancellationToken),
            RecentOrders = await LoadRecentOrdersAsync(connection, currentUser, effectiveStoreId, effectiveAllStores, periodStart, periodEnd, cancellationToken)
        };
    }

    public async Task<ReportExportResultDto> ExportPortalReportPdfAsync(int? storeId, bool allStores, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        PortalReportDto report = await GetPortalReportAsync(storeId, allStores, startDate, endDate, cancellationToken);
        string fileName = $"rapor-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

        return new ReportExportResultDto
        {
            FileName = fileName,
            ContentType = "application/pdf",
            Content = BuildPortalReportPdf(report)
        };
    }

    private static ReportMetricDto[] BuildMetrics(bool isAdmin, ReportAggregate aggregate, StockAggregate stock)
    {
        return
        [
            new()
            {
                Key = "revenue",
                Label = isAdmin ? "Dönem ciro" : "Benim ciro",
                Value = aggregate.TotalRevenue,
                DisplayValue = aggregate.TotalRevenue.ToString("C", TurkishCulture.Value),
                Detail = "Son 30 gün",
                Tone = "blue"
            },
            new()
            {
                Key = "orders",
                Label = isAdmin ? "Sipariş" : "Benim sipariş",
                Value = aggregate.OrderCount,
                DisplayValue = aggregate.OrderCount.ToString("N0", TurkishCulture.Value),
                Detail = $"{aggregate.ItemQuantity.ToString("N0", TurkishCulture.Value)} ürün adedi",
                Tone = "emerald"
            },
            new()
            {
                Key = "critical",
                Label = "Kritik stok",
                Value = stock.CriticalCount,
                DisplayValue = stock.CriticalCount.ToString("N0", TurkishCulture.Value),
                Detail = "Aktif bayi stok riski",
                Tone = "rose"
            },
            new()
            {
                Key = "passive",
                Label = "Pasif ürün",
                Value = stock.PassiveCount,
                DisplayValue = stock.PassiveCount.ToString("N0", TurkishCulture.Value),
                Detail = $"{stock.OutOfStockCount.ToString("N0", TurkishCulture.Value)} stok yok",
                Tone = "slate"
            }
        ];
    }

    private async Task<ReportAggregate> LoadAggregateAsync(SqlConnection connection, CurrentUser currentUser, int? storeId, bool allStores, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        await using SqlCommand command = new($@"
SELECT
    COUNT(DISTINCT o.SiparisId) AS SiparisSayisi,
    ISNULL(SUM(o.ToplamTutar), 0) AS ToplamTutar,
    ISNULL(SUM(o.Adet), 0) AS UrunAdedi
FROM dbo.vw_Siparis_Liste o
WHERE {OrderScopeWhere("o", currentUser.CanSeeAllStores)}
  AND o.IsCancelled = 0
  AND o.SiparisTarihi >= @Baslangic
  AND o.SiparisTarihi < @Bitis;", connection);

        AddScopeParameters(command, currentUser, storeId, allStores, startDate, endDate);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new ReportAggregate();

        return new ReportAggregate(reader.GetInt("SiparisSayisi"), reader.GetInt("UrunAdedi"), reader.GetDecimal("ToplamTutar"));
    }

    private async Task<StockAggregate> LoadStockAggregateAsync(SqlConnection connection, CurrentUser currentUser, int? storeId, bool allStores, CancellationToken cancellationToken)
    {
        await using SqlCommand command = new($@"
SELECT
    SUM(CASE WHEN msl.AktifMi = 1 AND msl.BayiStok > 0 AND (msl.StokDurumu = N'Kritik' OR (msl.MinimumStok <= 0 AND msl.BayiStok <= 5)) THEN 1 ELSE 0 END) AS KritikStok,
    SUM(CASE WHEN msl.AktifMi = 1 AND msl.BayiStok <= 0 THEN 1 ELSE 0 END) AS StokYok,
    SUM(CASE WHEN msl.AktifMi = 0 THEN 1 ELSE 0 END) AS PasifUrun
FROM dbo.vw_MagazaStok_Liste msl
WHERE {StoreScopeWhere("msl", currentUser.CanSeeAllStores)};", connection);

        AddScopeParameters(command, currentUser, storeId, allStores);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return new StockAggregate();

        return new StockAggregate(reader.GetInt("KritikStok"), reader.GetInt("StokYok"), reader.GetInt("PasifUrun"));
    }

    private async Task<ReportPersonPerformanceDto[]> LoadPersonnelAsync(SqlConnection connection, CurrentUser currentUser, int? storeId, bool allStores, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var rows = new List<ReportPersonPerformanceDto>();
        await using SqlCommand command = new($@"
SELECT TOP 8
    k.KullaniciId,
    k.AdSoyad,
    MAX(o.MagazaAdi) AS MagazaAdi,
    COUNT(DISTINCT o.SiparisId) AS SiparisSayisi,
    ISNULL(SUM(o.Adet), 0) AS UrunAdedi,
    ISNULL(SUM(o.ToplamTutar), 0) AS ToplamTutar
FROM dbo.vw_Siparis_Liste o
INNER JOIN dbo.BayiYetkilileri byk ON byk.BayiYetkiliId = o.BayiYetkiliId
INNER JOIN dbo.Kullanicilar k ON k.KullaniciId = byk.KullaniciId
WHERE {OrderScopeWhere("o", currentUser.CanSeeAllStores)}
  AND o.IsCancelled = 0
  AND o.SiparisTarihi >= @Baslangic
  AND o.SiparisTarihi < @Bitis
GROUP BY k.KullaniciId, k.AdSoyad
ORDER BY ToplamTutar DESC, SiparisSayisi DESC;", connection);

        AddScopeParameters(command, currentUser, storeId, allStores, startDate, endDate);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ReportPersonPerformanceDto
            {
                KullaniciId = reader.GetInt("KullaniciId"),
                AdSoyad = reader.GetText("AdSoyad"),
                MagazaAdi = reader.GetText("MagazaAdi"),
                SiparisSayisi = reader.GetInt("SiparisSayisi"),
                UrunAdedi = reader.GetInt("UrunAdedi"),
                ToplamTutar = reader.GetDecimal("ToplamTutar")
            });
        }

        return rows.ToArray();
    }

    private async Task<ReportProductPerformanceDto[]> LoadProductsAsync(SqlConnection connection, CurrentUser currentUser, int? storeId, bool allStores, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var rows = new List<ReportProductPerformanceDto>();
        await using SqlCommand command = new($@"
WITH SiparisUrunleri AS
(
    SELECT
        o.SiparisId,
        o.CustomerStoreId,
        urun.ProductId,
        urun.Quantity,
        urun.TotalPrice
    FROM dbo.vw_Siparis_Liste o
    CROSS APPLY
    (
        SELECT oi.ProductId, oi.Quantity, oi.LineTotal AS TotalPrice
        FROM dbo.OrderItems oi
        WHERE oi.OrderId = o.SiparisId
        UNION ALL
        SELECT oo.ProductId, oo.Quantity, oo.TotalPrice
        FROM dbo.Orders oo
        WHERE oo.OrderId = o.SiparisId
          AND NOT EXISTS (SELECT 1 FROM dbo.OrderItems oi2 WHERE oi2.OrderId = o.SiparisId)
    ) urun
    WHERE {OrderScopeWhere("o", currentUser.CanSeeAllStores)}
      AND o.IsCancelled = 0
      AND o.SiparisTarihi >= @Baslangic
      AND o.SiparisTarihi < @Bitis
)
SELECT TOP 8
    p.ProductId AS UrunId,
    p.ProductName AS UrunAdi,
    ISNULL(c.CategoryName, N'Kategorisiz') AS KategoriAdi,
    COUNT(DISTINCT su.SiparisId) AS SiparisSayisi,
    ISNULL(SUM(su.Quantity), 0) AS SatisAdedi,
    ISNULL(SUM(su.TotalPrice), 0) AS ToplamTutar,
    ISNULL(MAX(msl.BayiStok), 0) AS BayiStok,
    CAST(MAX(CASE WHEN msl.BayiStok > 0 AND (msl.StokDurumu = N'Kritik' OR (msl.MinimumStok <= 0 AND msl.BayiStok <= 5)) THEN 1 ELSE 0 END) AS bit) AS KritikMi,
    CAST(CASE WHEN p.IsActive = 0 THEN 1 ELSE 0 END AS bit) AS PasifMi
FROM SiparisUrunleri su
INNER JOIN dbo.Products p ON p.ProductId = su.ProductId
LEFT JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
LEFT JOIN dbo.vw_MagazaStok_Liste msl ON msl.UrunId = p.ProductId AND msl.MagazaId = su.CustomerStoreId
GROUP BY p.ProductId, p.ProductName, c.CategoryName, p.IsActive
ORDER BY ToplamTutar DESC, SatisAdedi DESC;", connection);

        AddScopeParameters(command, currentUser, storeId, allStores, startDate, endDate);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ReportProductPerformanceDto
            {
                UrunId = reader.GetInt("UrunId"),
                UrunAdi = reader.GetText("UrunAdi"),
                KategoriAdi = reader.GetText("KategoriAdi"),
                SiparisSayisi = reader.GetInt("SiparisSayisi"),
                SatisAdedi = reader.GetInt("SatisAdedi"),
                ToplamTutar = reader.GetDecimal("ToplamTutar"),
                BayiStok = reader.GetInt("BayiStok"),
                KritikMi = reader.GetBool("KritikMi"),
                PasifMi = reader.GetBool("PasifMi")
            });
        }

        return rows.ToArray();
    }

    private async Task<ReportCategoryPerformanceDto[]> LoadCategoriesAsync(SqlConnection connection, CurrentUser currentUser, int? storeId, bool allStores, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var rows = new List<ReportCategoryPerformanceDto>();
        await using SqlCommand command = new($@"
WITH SiparisUrunleri AS
(
    SELECT o.SiparisId, urun.ProductId, urun.Quantity, urun.TotalPrice
    FROM dbo.vw_Siparis_Liste o
    CROSS APPLY
    (
        SELECT oi.ProductId, oi.Quantity, oi.LineTotal AS TotalPrice
        FROM dbo.OrderItems oi
        WHERE oi.OrderId = o.SiparisId
        UNION ALL
        SELECT oo.ProductId, oo.Quantity, oo.TotalPrice
        FROM dbo.Orders oo
        WHERE oo.OrderId = o.SiparisId
          AND NOT EXISTS (SELECT 1 FROM dbo.OrderItems oi2 WHERE oi2.OrderId = o.SiparisId)
    ) urun
    WHERE {OrderScopeWhere("o", currentUser.CanSeeAllStores)}
      AND o.IsCancelled = 0
      AND o.SiparisTarihi >= @Baslangic
      AND o.SiparisTarihi < @Bitis
)
SELECT TOP 6
    ISNULL(c.CategoryName, N'Kategorisiz') AS KategoriAdi,
    COUNT(DISTINCT su.SiparisId) AS SiparisSayisi,
    ISNULL(SUM(su.Quantity), 0) AS SatisAdedi,
    ISNULL(SUM(su.TotalPrice), 0) AS ToplamTutar
FROM SiparisUrunleri su
INNER JOIN dbo.Products p ON p.ProductId = su.ProductId
LEFT JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
GROUP BY c.CategoryName
ORDER BY ToplamTutar DESC, SatisAdedi DESC;", connection);

        AddScopeParameters(command, currentUser, storeId, allStores, startDate, endDate);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ReportCategoryPerformanceDto
            {
                KategoriAdi = reader.GetText("KategoriAdi"),
                SiparisSayisi = reader.GetInt("SiparisSayisi"),
                SatisAdedi = reader.GetInt("SatisAdedi"),
                ToplamTutar = reader.GetDecimal("ToplamTutar")
            });
        }

        return rows.ToArray();
    }

    private async Task<ReportStockRiskDto[]> LoadStockRisksAsync(SqlConnection connection, CurrentUser currentUser, int? storeId, bool allStores, CancellationToken cancellationToken)
    {
        var rows = new List<ReportStockRiskDto>();
        await using SqlCommand command = new($@"
SELECT TOP 12
    msl.UrunId,
    msl.UrunAdi,
    msl.KategoriAdi,
    msl.BayiStok,
    msl.MerkezStok,
    msl.MinimumStok,
    CASE
        WHEN msl.AktifMi = 0 THEN N'Pasif'
        WHEN msl.BayiStok <= 0 THEN N'Stok yok'
        WHEN msl.BayiStok > 0 AND (msl.StokDurumu = N'Kritik' OR (msl.MinimumStok <= 0 AND msl.BayiStok <= 5)) THEN N'Kritik'
        ELSE N'Yeterli'
    END AS Durum,
    msl.AktifMi
FROM dbo.vw_MagazaStok_Liste msl
WHERE {StoreScopeWhere("msl", currentUser.CanSeeAllStores)}
  AND
  (
      msl.AktifMi = 0
      OR msl.BayiStok <= 0
      OR (msl.BayiStok > 0 AND (msl.StokDurumu = N'Kritik' OR (msl.MinimumStok <= 0 AND msl.BayiStok <= 5)))
  )
ORDER BY
    CASE
        WHEN msl.AktifMi = 0 THEN 3
        WHEN msl.BayiStok <= 0 THEN 1
        WHEN msl.BayiStok > 0 AND (msl.StokDurumu = N'Kritik' OR (msl.MinimumStok <= 0 AND msl.BayiStok <= 5)) THEN 2
        ELSE 4
    END,
    msl.BayiStok ASC,
    msl.UrunAdi ASC;", connection);

        AddScopeParameters(command, currentUser, storeId, allStores);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ReportStockRiskDto
            {
                UrunId = reader.GetInt("UrunId"),
                UrunAdi = reader.GetText("UrunAdi"),
                KategoriAdi = reader.GetText("KategoriAdi"),
                BayiStok = reader.GetInt("BayiStok"),
                MerkezStok = reader.GetInt("MerkezStok"),
                MinimumStok = reader.GetInt("MinimumStok"),
                Durum = reader.GetText("Durum"),
                AktifMi = reader.GetBool("AktifMi")
            });
        }

        return rows.ToArray();
    }

    private async Task<ReportOrderRowDto[]> LoadRecentOrdersAsync(SqlConnection connection, CurrentUser currentUser, int? storeId, bool allStores, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var rows = new List<ReportOrderRowDto>();
        await using SqlCommand command = new($@"
SELECT TOP 8
    o.SiparisId,
    o.OrderNo,
    o.MagazaAdi,
    o.YetkiliAdi,
    o.UrunAdi,
    o.Adet,
    o.ToplamTutar,
    o.SiparisDurumu,
    o.SiparisTarihi
FROM dbo.vw_Siparis_Liste o
WHERE {OrderScopeWhere("o", currentUser.CanSeeAllStores)}
  AND o.SiparisTarihi >= @Baslangic
  AND o.SiparisTarihi < @Bitis
ORDER BY o.SiparisTarihi DESC, o.SiparisId DESC;", connection);

        AddScopeParameters(command, currentUser, storeId, allStores, startDate, endDate);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ReportOrderRowDto
            {
                SiparisId = reader.GetInt("SiparisId"),
                OrderNo = reader.GetText("OrderNo"),
                MagazaAdi = reader.GetText("MagazaAdi"),
                YetkiliAdi = reader.GetText("YetkiliAdi"),
                UrunAdi = reader.GetText("UrunAdi"),
                Adet = reader.GetInt("Adet"),
                ToplamTutar = reader.GetDecimal("ToplamTutar"),
                SiparisDurumu = reader.GetText("SiparisDurumu"),
                SiparisTarihi = reader.GetNullableDate("SiparisTarihi")
            });
        }

        return rows.ToArray();
    }

    private static async Task<string> ResolveStoreNameAsync(SqlConnection connection, int? storeId, bool allStores, CancellationToken cancellationToken)
    {
        if (allStores)
            return "Tüm bayiler";

        if (!storeId.HasValue)
            return "Aktif bayi";

        await using SqlCommand command = new("SELECT TOP 1 StoreName FROM dbo.CustomerStores WHERE CustomerStoreId = @MagazaId;", connection);
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId.Value;
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? "Aktif bayi" : Convert.ToString(result) ?? "Aktif bayi";
    }

    private static async Task<int?> ResolveEffectiveStoreIdAsync(SqlConnection connection, CurrentUser currentUser, bool canSeeAllStores, int? requestedStoreId, CancellationToken cancellationToken)
    {
        if (requestedStoreId.HasValue)
        {
            await using SqlCommand validateCommand = new($@"
SELECT TOP 1 cs.CustomerStoreId
FROM dbo.CustomerStores cs
WHERE cs.CustomerStoreId = @MagazaId
  AND ({(canSeeAllStores ? "1 = 1" : @"EXISTS
      (
          SELECT 1
          FROM dbo.KullaniciMagazalari km
          WHERE km.MagazaId = cs.CustomerStoreId
            AND km.KullaniciId = @KullaniciId
            AND km.AktifMi = 1
      )")});", connection);

            validateCommand.Parameters.Add("@MagazaId", SqlDbType.Int).Value = requestedStoreId.Value;
            validateCommand.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.HasValue ? currentUser.UserId.Value : DBNull.Value;

            object? validated = await validateCommand.ExecuteScalarAsync(cancellationToken);
            if (validated == null || validated == DBNull.Value)
                throw new UnauthorizedAccessException("Bu bayi için rapor görüntüleme yetkiniz yok.");

            return Convert.ToInt32(validated);
        }

        await using SqlCommand command = new($@"
SELECT TOP 1 cs.CustomerStoreId
FROM dbo.CustomerStores cs
WHERE cs.IsActive = 1
  AND ({(canSeeAllStores ? "1 = 1" : @"EXISTS
      (
          SELECT 1
          FROM dbo.KullaniciMagazalari km
          WHERE km.MagazaId = cs.CustomerStoreId
            AND km.KullaniciId = @KullaniciId
            AND km.AktifMi = 1
      )")})
ORDER BY cs.StoreName, cs.CustomerStoreId;", connection);

        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.HasValue ? currentUser.UserId.Value : DBNull.Value;
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    private static string OrderScopeWhere(string alias, bool isAdmin)
    {
        string storeScope = $"(@TumMagazalar = 1 OR {alias}.CustomerStoreId = @MagazaId)";
        string accessScope = isAdmin
            ? "1 = 1"
            : $@"EXISTS
              (
                  SELECT 1
                  FROM dbo.KullaniciMagazalari km
                  WHERE km.MagazaId = {alias}.CustomerStoreId
                    AND km.KullaniciId = @KullaniciId
                    AND km.AktifMi = 1
              )";
        string personnelScope = isAdmin
            ? "1 = 1"
            : $@"EXISTS
              (
                  SELECT 1
                  FROM dbo.BayiYetkilileri bykScope
                  WHERE bykScope.BayiYetkiliId = {alias}.BayiYetkiliId
                    AND bykScope.KullaniciId = @KullaniciId
                    AND bykScope.AktifMi = 1
              )";

        return $"{storeScope} AND {accessScope} AND {personnelScope}";
    }

    private static string StoreScopeWhere(string alias, bool isAdmin)
    {
        string storeScope = $"(@TumMagazalar = 1 OR {alias}.MagazaId = @MagazaId)";
        string accessScope = isAdmin
            ? "1 = 1"
            : $@"EXISTS
              (
                  SELECT 1
                  FROM dbo.KullaniciMagazalari km
                  WHERE km.MagazaId = {alias}.MagazaId
                    AND km.KullaniciId = @KullaniciId
                    AND km.AktifMi = 1
              )";

        return $"{storeScope} AND {accessScope}";
    }

    private static void AddScopeParameters(SqlCommand command, CurrentUser currentUser, int? storeId, bool allStores, DateTime? startDate = null, DateTime? endDate = null)
    {
        command.Parameters.Add("@MagazaId", SqlDbType.Int).Value = storeId.HasValue ? storeId.Value : DBNull.Value;
        command.Parameters.Add("@TumMagazalar", SqlDbType.Bit).Value = allStores;
        command.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = currentUser.UserId.HasValue ? currentUser.UserId.Value : DBNull.Value;

        if (startDate.HasValue)
            command.Parameters.Add("@Baslangic", SqlDbType.DateTime2).Value = startDate.Value;

        if (endDate.HasValue)
            command.Parameters.Add("@Bitis", SqlDbType.DateTime2).Value = endDate.Value;
    }

    private static readonly Lazy<System.Globalization.CultureInfo> TurkishCulture = new(() => System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));

    private static byte[] BuildPortalReportPdf(PortalReportDto report)
    {
        var content = new StringBuilder();
        int y = 790;

        AddPdfText(content, 42, y, 18, "eMarketing Raporlar");
        y -= 24;
        AddPdfText(content, 42, y, 12, report.Title);
        y -= 18;
        AddPdfText(content, 42, y, 9, $"Kapsam: {report.StoreName}");
        y -= 14;
        AddPdfText(content, 42, y, 9, $"Donem: {report.StartDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}");
        y -= 14;
        AddPdfText(content, 42, y, 9, $"Olusturma: {DateTime.Now:dd.MM.yyyy HH:mm}");
        y -= 30;

        AddPdfText(content, 42, y, 13, "Ozet Metrikler");
        y -= 22;
        for (int i = 0; i < report.Metrics.Length; i++)
        {
            ReportMetricDto metric = report.Metrics[i];
            int x = 54 + (i % 2) * 250;
            int rowY = y - (i / 2) * 22;
            AddPdfText(content, x, rowY, 10, $"{metric.Label}: {metric.DisplayValue}");
            AddPdfText(content, x, rowY - 11, 7, metric.Detail);
        }

        y -= 66;
        y = AddPdfSection(content, y, "Urun Performansi", report.Products.Select(product =>
            $"{product.UrunAdi} | {product.SatisAdedi:N0} adet | {product.ToplamTutar.ToString("C", TurkishCulture.Value)} | Stok {product.BayiStok:N0}"));

        y = AddPdfSection(content, y, "Stok Riskleri", report.StockRisks.Select(risk =>
            $"{risk.UrunAdi} | {risk.Durum} | Bayi {risk.BayiStok:N0} | Merkez {risk.MerkezStok:N0}"));

        y = AddPdfSection(content, y, "Son Siparisler", report.RecentOrders.Select(order =>
            $"{(string.IsNullOrWhiteSpace(order.OrderNo) ? $"SP-{order.SiparisId:D5}" : order.OrderNo)} | {order.UrunAdi} | {order.Adet:N0} adet | {order.ToplamTutar.ToString("C", TurkishCulture.Value)}"));

        if (report.Personnel.Length > 0 && y > 115)
        {
            AddPdfSection(content, y, "Personel Performansi", report.Personnel.Select(person =>
                $"{person.AdSoyad} | {person.MagazaAdi} | {person.SiparisSayisi:N0} siparis | {person.ToplamTutar.ToString("C", TurkishCulture.Value)}"));
        }

        AddPdfText(content, 42, 42, 8, "Bu rapor eMarketing Web portal uzerinden olusturuldu.");
        return WriteSinglePagePdf(content.ToString());
    }

    private static int AddPdfSection(StringBuilder content, int y, string title, IEnumerable<string> rows)
    {
        if (y < 120)
            return y;

        AddPdfText(content, 42, y, 12, title);
        y -= 18;

        int count = 0;
        foreach (string row in rows.Take(10))
        {
            AddPdfText(content, 54, y, 8, Truncate(ToPdfSafeText(row), 122));
            y -= 14;
            count++;

            if (y < 90)
                break;
        }

        if (count == 0)
        {
            AddPdfText(content, 54, y, 8, "Kayit bulunamadi.");
            y -= 14;
        }

        return y - 16;
    }

    private static void AddPdfText(StringBuilder content, int x, int y, int size, string text)
    {
        content.Append("BT /F1 ");
        content.Append(size);
        content.Append(" Tf ");
        content.Append(x);
        content.Append(' ');
        content.Append(y);
        content.Append(" Td (");
        content.Append(EscapePdfText(ToPdfSafeText(text)));
        content.AppendLine(") Tj ET");
    }

    private static byte[] WriteSinglePagePdf(string content)
    {
        var objects = new List<byte[]>
        {
            Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"),
            Encoding.ASCII.GetBytes("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"),
            Encoding.ASCII.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>"),
            Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>")
        };

        byte[] contentBytes = Encoding.ASCII.GetBytes(content);
        objects.Add(BuildPdfStreamObject($"<< /Length {contentBytes.Length} >>", contentBytes));

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes("%PDF-1.4\n"));
        var offsets = new List<long> { 0 };

        for (int i = 0; i < objects.Count; i++)
        {
            offsets.Add(stream.Position);
            writer.Write(Encoding.ASCII.GetBytes($"{i + 1} 0 obj\n"));
            writer.Write(objects[i]);
            writer.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
        }

        long xref = stream.Position;
        writer.Write(Encoding.ASCII.GetBytes($"xref\n0 {objects.Count + 1}\n"));
        writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
        for (int i = 1; i < offsets.Count; i++)
            writer.Write(Encoding.ASCII.GetBytes($"{offsets[i]:0000000000} 00000 n \n"));

        writer.Write(Encoding.ASCII.GetBytes($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF"));
        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildPdfStreamObject(string dictionary, byte[] bytes)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes(dictionary + "\nstream\n"));
        writer.Write(bytes);
        writer.Write(Encoding.ASCII.GetBytes("\nendstream"));
        writer.Flush();
        return stream.ToArray();
    }

    private static string EscapePdfText(string text) =>
        (text ?? string.Empty).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static string ToPdfSafeText(string text) =>
        (text ?? string.Empty)
            .Replace("ı", "i").Replace("İ", "I")
            .Replace("ğ", "g").Replace("Ğ", "G")
            .Replace("ü", "u").Replace("Ü", "U")
            .Replace("ş", "s").Replace("Ş", "S")
            .Replace("ö", "o").Replace("Ö", "O")
            .Replace("ç", "c").Replace("Ç", "C")
            .Replace("₺", "TL")
            .Replace("–", "-").Replace("—", "-");

    private static string Truncate(string text, int maxLength) =>
        string.IsNullOrEmpty(text) || text.Length <= maxLength
            ? text
            : text[..(maxLength - 3)] + "...";

    private sealed record ReportAggregate(int OrderCount = 0, int ItemQuantity = 0, decimal TotalRevenue = 0);
    private sealed record StockAggregate(int CriticalCount = 0, int OutOfStockCount = 0, int PassiveCount = 0);
}
