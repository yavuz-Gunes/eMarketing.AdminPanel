using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Models;

namespace eMarketing.AdminPanel.Services
{
    public sealed class ReportService
    {
        private readonly ApiDataClient _apiClient;
        private readonly CultureInfo _culture = new CultureInfo("tr-TR");

        public ReportService(ApiDataClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ReportDto> CreateReportAsync(string reportType)
        {
            int? magazaId = GetCurrentMagazaId();
            bool tumMagazalar = AppSession.AdminMi && (AppSession.TumMagazalar || !AppSession.SeciliMagazaId.HasValue);

            DashboardSummaryView summary = await _apiClient.GetDashboardSummaryAsync(magazaId, tumMagazalar);
            DataTable recentOrders = await _apiClient.GetDashboardRecentOrdersAsync(magazaId, tumMagazalar);
            DataTable criticalStock = await _apiClient.GetDashboardCriticalStockAsync(magazaId, tumMagazalar);

            ReportDto report = new ReportDto
            {
                ReportId = "RPT-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ReportType = string.IsNullOrWhiteSpace(reportType) ? "Genel Özet Raporu" : reportType,
                CreatedAt = DateTime.Now,
                StoreName = AppSession.MagazaGorunumAdi,
                CreatedBy = string.IsNullOrWhiteSpace(AppSession.AdSoyad) ? AppSession.KullaniciAdi : AppSession.AdSoyad
            };

            report.Metrics.Add(new ReportMetricDto { Title = "Toplam Ciro", Value = summary.TotalRevenue.ToString("N2", _culture) + " TL" });
            report.Metrics.Add(new ReportMetricDto { Title = "Toplam Sipariş", Value = summary.TotalOrders.ToString() });
            report.Metrics.Add(new ReportMetricDto { Title = "Aktif Mağaza", Value = summary.ActiveStores.ToString() });
            report.Metrics.Add(new ReportMetricDto { Title = "Kritik Stok", Value = summary.LowStockProducts.ToString() });

            if (report.ReportType == "Sipariş Raporu" || report.ReportType == "Genel Özet Raporu" || report.ReportType == "Mağaza/Personel Bazlı Özet Rapor")
            {
                report.Tables.Add(new ReportTableDto
                {
                    Title = "Son Siparişler",
                    Data = recentOrders,
                    Columns = new[] { "MusteriAdi", "UrunAdi", "Adet", "ToplamTutar", "SiparisDurumu", "SiparisTarihi" }
                });
            }

            if (report.ReportType == "Stok Raporu" || report.ReportType == "Kritik Stok Raporu" || report.ReportType == "Genel Özet Raporu")
            {
                report.Tables.Add(new ReportTableDto
                {
                    Title = "Kritik Stok",
                    Data = criticalStock,
                    Columns = new[] { "MagazaAdi", "UrunAdi", "KategoriAdi", "Stok", "MinimumStok", "Durum" }
                });
            }

            report.SummaryHash = CreateHash(report.ReportId + report.ReportType + report.StoreName + report.CreatedAt.ToString("O"));
            return report;
        }

        private int? GetCurrentMagazaId()
        {
            if (AppSession.AdminMi && AppSession.TumMagazalar)
                return null;

            return AppSession.SeciliMagazaId;
        }

        private static string CreateHash(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 16);
            }
        }
    }
}
