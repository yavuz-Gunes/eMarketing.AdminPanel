using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using eMarketing.AdminPanel.Models;

namespace eMarketing.AdminPanel.Services
{
    public sealed class PdfReportBuilder
    {
        private readonly CultureInfo _culture = new CultureInfo("tr-TR");

        public void Save(ReportDto report, string filePath)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            string qrPayload = new JavaScriptSerializer().Serialize(new
            {
                reportId = report.ReportId,
                createdAt = report.CreatedAt.ToString("O"),
                storeName = report.StoreName,
                createdBy = report.CreatedBy,
                summaryHash = report.SummaryHash
            });

            byte[] qrBytes;
            using (Bitmap qr = QrCodeHelper.CreateQrBitmap(qrPayload, 150))
            using (MemoryStream imageStream = new MemoryStream())
            {
                qr.Save(imageStream, ImageFormat.Jpeg);
                qrBytes = imageStream.ToArray();
            }

            string content = BuildContent(report);
            WriteSinglePagePdf(filePath, content, qrBytes);
        }

        private string BuildContent(ReportDto report)
        {
            StringBuilder sb = new StringBuilder();
            AddText(sb, 42, 790, 18, "eMarketing - Oto Yedek Parça Yönetim Paneli");
            AddText(sb, 42, 766, 12, report.ReportType);
            AddText(sb, 42, 744, 9, "Rapor No: " + report.ReportId);
            AddText(sb, 42, 728, 9, "Mağaza/Bayi: " + report.StoreName);
            AddText(sb, 42, 712, 9, "Oluşturan: " + report.CreatedBy);
            AddText(sb, 42, 696, 9, "Tarih: " + report.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
            AddText(sb, 430, 700, 8, "QR doğrulama");
            sb.AppendLine("q 120 0 0 120 430 570 cm /Im1 Do Q");

            int y = 650;
            AddText(sb, 42, y, 13, "Özet Metrikler");
            y -= 24;

            for (int i = 0; i < report.Metrics.Count; i++)
            {
                ReportMetricDto metric = report.Metrics[i];
                AddText(sb, 54 + (i % 2) * 245, y - (i / 2) * 22, 10, metric.Title + ": " + metric.Value);
            }

            y -= 70;

            foreach (ReportTableDto table in report.Tables)
            {
                AddText(sb, 42, y, 12, table.Title);
                y -= 18;
                y = AddTable(sb, table, y);
                y -= 24;

                if (y < 100)
                    break;
            }

            AddText(sb, 42, 42, 8, "Bu rapor eMarketing AdminPanel tarafından oluşturuldu. Sayfa 1/1");
            AddText(sb, 405, 42, 8, "Doğrulama: " + report.SummaryHash);
            return sb.ToString();
        }

        private int AddTable(StringBuilder sb, ReportTableDto table, int y)
        {
            if (table.Data == null || table.Columns == null)
            {
                AddText(sb, 54, y, 9, "Kayıt bulunamadı.");
                return y - 16;
            }

            AddText(sb, 54, y, 8, string.Join(" | ", table.Columns));
            y -= 16;

            int count = 0;
            foreach (DataRow row in table.Data.Rows)
            {
                List<string> values = new List<string>();
                foreach (string column in table.Columns)
                {
                    if (!table.Data.Columns.Contains(column) || row[column] == DBNull.Value)
                        values.Add("-");
                    else
                        values.Add(FormatValue(column, row[column]));
                }

                AddText(sb, 54, y, 8, Truncate(string.Join(" | ", values), 118));
                y -= 14;
                count++;

                if (count >= 10 || y < 80)
                    break;
            }

            if (count == 0)
                AddText(sb, 54, y, 9, "Kayıt bulunamadı.");

            return y;
        }

        private string FormatValue(string column, object value)
        {
            if (column.IndexOf("Tutar", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                decimal money;
                if (decimal.TryParse(Convert.ToString(value), out money))
                    return money.ToString("N2", _culture) + " TL";
            }

            if (column.IndexOf("Tarih", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DateTime date;
                if (DateTime.TryParse(Convert.ToString(value), out date))
                    return date.ToString("dd.MM.yyyy HH:mm");
            }

            return Convert.ToString(value);
        }

        private static void AddText(StringBuilder sb, int x, int y, int size, string text)
        {
            sb.Append("BT /F1 ");
            sb.Append(size);
            sb.Append(" Tf ");
            sb.Append(x);
            sb.Append(" ");
            sb.Append(y);
            sb.Append(" Td (");
            sb.Append(EscapePdfText(ToPdfSafeText(text)));
            sb.AppendLine(") Tj ET");
        }

        private static void WriteSinglePagePdf(string filePath, string content, byte[] imageBytes)
        {
            List<byte[]> objects = new List<byte[]>();
            objects.Add(Encoding.ASCII.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"));
            objects.Add(Encoding.ASCII.GetBytes("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"));
            objects.Add(Encoding.ASCII.GetBytes("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> /XObject << /Im1 5 0 R >> >> /Contents 6 0 R >>"));
            objects.Add(Encoding.ASCII.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"));
            objects.Add(BuildStreamObject("<< /Type /XObject /Subtype /Image /Width 150 /Height 150 /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length " + imageBytes.Length + " >>", imageBytes));
            byte[] contentBytes = Encoding.ASCII.GetBytes(content);
            objects.Add(BuildStreamObject("<< /Length " + contentBytes.Length + " >>", contentBytes));

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(Encoding.ASCII.GetBytes("%PDF-1.4\n"));
                List<long> offsets = new List<long> { 0 };

                for (int i = 0; i < objects.Count; i++)
                {
                    offsets.Add(fs.Position);
                    writer.Write(Encoding.ASCII.GetBytes((i + 1) + " 0 obj\n"));
                    writer.Write(objects[i]);
                    writer.Write(Encoding.ASCII.GetBytes("\nendobj\n"));
                }

                long xref = fs.Position;
                writer.Write(Encoding.ASCII.GetBytes("xref\n0 " + (objects.Count + 1) + "\n"));
                writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
                for (int i = 1; i < offsets.Count; i++)
                    writer.Write(Encoding.ASCII.GetBytes(offsets[i].ToString("0000000000") + " 00000 n \n"));
                writer.Write(Encoding.ASCII.GetBytes("trailer\n<< /Size " + (objects.Count + 1) + " /Root 1 0 R >>\nstartxref\n" + xref + "\n%%EOF"));
            }
        }

        private static byte[] BuildStreamObject(string dictionary, byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes(dictionary + "\nstream\n"));
                writer.Write(bytes);
                writer.Write(Encoding.ASCII.GetBytes("\nendstream"));
                return stream.ToArray();
            }
        }

        private static string EscapePdfText(string text)
        {
            return (text ?? "").Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static string ToPdfSafeText(string text)
        {
            return (text ?? "")
                .Replace("ı", "i").Replace("İ", "I")
                .Replace("ğ", "g").Replace("Ğ", "G")
                .Replace("ü", "u").Replace("Ü", "U")
                .Replace("ş", "s").Replace("Ş", "S")
                .Replace("ö", "o").Replace("Ö", "O")
                .Replace("ç", "c").Replace("Ç", "C");
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
