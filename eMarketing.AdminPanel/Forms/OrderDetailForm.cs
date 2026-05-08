using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
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
            ShowIcon = false;
            Width = 660;
            Height = 610;
            BackColor = AppColors.Background;

            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                BackColor = AppColors.Background
            };

            Panel headerCard = CreateHeaderCard(row);
            TableLayoutPanel infoGrid = CreateInfoGrid(row);
            Panel footer = CreateFooter();

            container.Controls.Add(infoGrid);
            container.Controls.Add(headerCard);
            container.Controls.Add(footer);

            Controls.Add(container);
        }

        private Panel CreateHeaderCard(DataRow row)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Top,
                Height = 152,
                Padding = new Padding(16),
                BackColor = AppColors.CardBackground
            };

            PictureBox image = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 112,
                BackColor = AppColors.PrimarySoft,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = LoadProductImage(row)
            };

            Panel textArea = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 0, 0),
                BackColor = Color.Transparent
            };

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = GetText(row, "UrunAdi", "Sipariş Ürünü"),
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoEllipsis = true
            };

            Label subtitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = GetText(row, "MusteriAdi", "Bayi") + " / " + GetText(row, "MagazaAdi", "Mağaza"),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppColors.TextSecondary,
                AutoEllipsis = true
            };

            Label meta = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Text = "Sipariş #" + GetText(row, "SiparisId", "-") + "  •  " + GetDateText(row, "SiparisTarihi"),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextMuted,
                AutoEllipsis = true
            };

            Label status = CreateStatusBadge(GetText(row, "SiparisDurumu", "-"));
            status.Location = new Point(16, 96);

            textArea.Controls.Add(status);
            textArea.Controls.Add(meta);
            textArea.Controls.Add(subtitle);
            textArea.Controls.Add(title);

            card.Controls.Add(textArea);
            card.Controls.Add(image);

            return card;
        }

        private TableLayoutPanel CreateInfoGrid(DataRow row)
        {
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 18, 0, 0),
                BackColor = AppColors.Background
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Panel bayiCard = CreateSectionCard("Bayi ve Yetkili");
            AddInfoRow(bayiCard, "Bayi", GetText(row, "MusteriAdi", "-"));
            AddInfoRow(bayiCard, "Mağaza", GetText(row, "MagazaAdi", "-"));
            AddInfoRow(bayiCard, "Yetkili", GetText(row, "YetkiliAdi", "-"));
            AddInfoRow(bayiCard, "Telefon", GetText(row, "YetkiliTelefon", GetText(row, "MusteriTelefon", "-")));
            AddInfoRow(bayiCard, "E-Posta", GetText(row, "YetkiliEmail", GetText(row, "MusteriEmail", "-")));

            Panel siparisCard = CreateSectionCard("Sipariş Özeti");
            AddInfoRow(siparisCard, "Ürün", GetText(row, "UrunAdi", "-"));
            AddInfoRow(siparisCard, "Adet", GetText(row, "Adet", "0"));
            AddInfoRow(siparisCard, "Bayide Stok", GetText(row, "BayiStok", "0"));
            AddInfoRow(siparisCard, "Tutar", GetMoneyText(row, "ToplamTutar"));
            AddInfoRow(siparisCard, "Kaynak", GetSourceText(row));

            grid.Controls.Add(bayiCard, 0, 0);
            grid.Controls.Add(siparisCard, 1, 0);

            return grid;
        }

        private Panel CreateSectionCard(string title)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 12, 0),
                BackColor = AppColors.CardBackground
            };

            Label label = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            panel.Controls.Add(label);
            return panel;
        }

        private void AddInfoRow(Panel panel, string label, string value)
        {
            Panel row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = Color.Transparent
            };

            Label title = new Label
            {
                Dock = DockStyle.Left,
                Width = 92,
                Text = label,
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label content = new Label
            {
                Dock = DockStyle.Fill,
                Text = value,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            row.Controls.Add(content);
            row.Controls.Add(title);
            panel.Controls.Add(row);
            row.SendToBack();
        }

        private Label CreateStatusBadge(string status)
        {
            string display = GetStatusDisplayText(status);
            Color backColor = AppColors.PrimarySoft;
            Color foreColor = AppColors.Primary;

            if (status == "Teslim Edildi")
            {
                backColor = AppColors.SuccessSoft;
                foreColor = AppColors.Success;
            }
            else if (status == "Iptal")
            {
                backColor = AppColors.DangerSoft;
                foreColor = AppColors.Danger;
            }
            else if (status == "Hazirlaniyor")
            {
                backColor = AppColors.WarningSoft;
                foreColor = AppColors.Warning;
            }

            return new Label
            {
                Width = 132,
                Height = 28,
                Text = display,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private Panel CreateFooter()
        {
            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = AppColors.Background
            };

            Button btnClose = new Button
            {
                Text = "Kapat",
                Width = 118,
                Height = 38,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            footer.Resize += (s, e) =>
            {
                btnClose.Location = new Point(footer.Width - btnClose.Width, 10);
            };

            footer.Controls.Add(btnClose);
            return footer;
        }

        private Image LoadProductImage(DataRow row)
        {
            string imagePath = GetText(row, "GorselUrl", "");

            try
            {
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    string fullPath = imagePath;

                    if (!Path.IsPathRooted(fullPath))
                        fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);

                    if (File.Exists(fullPath))
                    {
                        using (Image temp = Image.FromFile(fullPath))
                            return new Bitmap(temp);
                    }
                }
            }
            catch
            {
            }

            return CreatePlaceholderImage(GetText(row, "UrunAdi", "P"));
        }

        private Image CreatePlaceholderImage(string text)
        {
            Bitmap bitmap = new Bitmap(96, 96);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (SolidBrush background = new SolidBrush(AppColors.PrimarySoft))
            using (SolidBrush foreground = new SolidBrush(AppColors.Primary))
            using (Font font = new Font("Segoe UI", 22F, FontStyle.Bold))
            {
                graphics.Clear(AppColors.PrimarySoft);

                string letter = string.IsNullOrWhiteSpace(text) ? "P" : text.Substring(0, 1).ToUpper(new CultureInfo("tr-TR"));
                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.DrawString(letter, font, foreground, new RectangleF(0, 0, 96, 96), format);
            }

            return bitmap;
        }

        private string GetText(DataRow row, string columnName, string defaultValue)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            string value = Convert.ToString(row[columnName]);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private string GetDateText(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return "-";

            return Convert.ToDateTime(row[columnName]).ToString("dd.MM.yyyy HH:mm");
        }

        private string GetMoneyText(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return "0,00 TL";

            decimal value = Convert.ToDecimal(row[columnName]);
            return value.ToString("N2", new CultureInfo("tr-TR")) + " TL";
        }

        private string GetSourceText(DataRow row)
        {
            string source = GetText(row, "OrderSource", "-");
            string type = GetText(row, "OrderType", "Bayi");

            if (source == "AdminPanel")
                source = "Admin";

            return source + " / " + type;
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
