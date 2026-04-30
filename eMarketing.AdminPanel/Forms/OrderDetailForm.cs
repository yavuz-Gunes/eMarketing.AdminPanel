using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Forms
{
    public class OrderDetailForm : Form
    {
        public OrderDetailForm(DataRow row)
        {
            Text = "Sipariş Detayı";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 520;
            Height = 430;
            BackColor = Color.White;

            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(26),
                BackColor = Color.White
            };

            Label title = new Label
            {
                Text = "Sipariş Detayı",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Dock = DockStyle.Top,
                Height = 42
            };

            Label subtitle = new Label
            {
                Text = "Seçili sipariş kaydına ait özet bilgiler",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                Dock = DockStyle.Top,
                Height = 28
            };

            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 18, 0, 12)
            };

            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AddRow(body, "Sipariş ID", GetText(row, "SiparisId"));
            AddRow(body, "Müşteri", GetText(row, "MusteriAdi"));
            AddRow(body, "Parça", GetText(row, "UrunAdi"));
            AddRow(body, "Adet", GetText(row, "Adet"));
            AddRow(body, "Tutar", GetMoneyText(row, "ToplamTutar"));
            AddRow(body, "Durum", GetStatusDisplayText(GetText(row, "SiparisDurumu")));
            AddRow(body, "Tarih", GetDateText(row, "SiparisTarihi"));

            Button btnClose = new Button
            {
                Text = "Kapat",
                Width = 110,
                Height = 38,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.White
            };

            footer.Resize += (s, e) =>
            {
                btnClose.Location = new Point(footer.Width - btnClose.Width, 10);
            };

            footer.Controls.Add(btnClose);

            container.Controls.Add(footer);
            container.Controls.Add(body);
            container.Controls.Add(subtitle);
            container.Controls.Add(title);

            Controls.Add(container);
        }

        private void AddRow(TableLayoutPanel panel, string label, string value)
        {
            int rowIndex = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label lblTitle = new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Margin = new Padding(0, 0, 0, 14)
            };

            Label lblValue = new Label
            {
                Text = value,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextPrimary,
                Margin = new Padding(0, 0, 0, 14)
            };

            panel.Controls.Add(lblTitle, 0, rowIndex);
            panel.Controls.Add(lblValue, 1, rowIndex);
        }

        private string GetText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return "-";

            if (row[columnName] == DBNull.Value)
                return "-";

            string value = row[columnName]?.ToString();

            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private string GetDateText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return "-";

            if (row[columnName] == DBNull.Value)
                return "-";

            return Convert.ToDateTime(row[columnName]).ToString("dd.MM.yyyy HH:mm");
        }

        private string GetMoneyText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return "0,00 ₺";

            if (row[columnName] == DBNull.Value)
                return "0,00 ₺";

            decimal value = Convert.ToDecimal(row[columnName]);
            return value.ToString("N2", new CultureInfo("tr-TR")) + " ₺";
        }

        private string GetStatusDisplayText(string status)
        {
            if (status == "Hazirlaniyor")
                return "Hazırlanıyor";

            if (status == "Iptal")
                return "İptal";

            return status;
        }
    }
}