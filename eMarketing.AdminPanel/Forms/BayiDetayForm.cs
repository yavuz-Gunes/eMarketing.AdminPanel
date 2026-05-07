using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public class BayiDetayForm : Form
    {
        private readonly DataRow magazaRow;
        private readonly int bayiId;
        private readonly int magazaId;

        private readonly BayiYetkiliRepository yetkiliRepo = new BayiYetkiliRepository();
        private readonly OrderRepository orderRepo = new OrderRepository();
        private readonly MagazaStokRepository stokRepo = new MagazaStokRepository();

        private TabControl tabs;
        private DataGridView dgvYetkililer;
        private DataGridView dgvSiparisler;
        private DataGridView dgvStoklar;

        public BayiDetayForm(DataRow magazaRow)
        {
            if (magazaRow == null)
                throw new ArgumentNullException(nameof(magazaRow));

            this.magazaRow = magazaRow;
            bayiId = GetInt(magazaRow, "MusteriId");
            magazaId = GetInt(magazaRow, "MagazaId");

            BuildLayout();
            Load += BayiDetayForm_Load;
        }

        private void BuildLayout()
        {
            Text = "Bayi Detayı";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ShowIcon = false;
            Width = 980;
            Height = 680;
            BackColor = AppColors.Background;

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(26, 18, 26, 12)
            };

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = GetText(magazaRow, "MusteriAdi", "Bayi"),
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            Label subtitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Text = GetText(magazaRow, "MagazaAdi", "Mağaza") + "  |  " + GetKonumText(magazaRow),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppColors.TextSecondary
            };

            header.Controls.Add(subtitle);
            header.Controls.Add(title);

            tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                Padding = new Point(16, 8)
            };

            tabs.TabPages.Add(CreateGeneralTab());
            tabs.TabPages.Add(CreateYetkililerTab());
            tabs.TabPages.Add(CreateSiparislerTab());
            tabs.TabPages.Add(CreateStoklarTab());
            tabs.TabPages.Add(CreateCariTab());

            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(24, 14, 24, 14)
            };

            Button btnClose = CreateButton("Kapat", true);
            btnClose.Width = 110;
            btnClose.Click += (sender, e) => Close();
            footer.Controls.Add(btnClose);
            footer.Resize += (sender, e) =>
            {
                btnClose.Location = new Point(footer.Width - btnClose.Width - 24, 14);
            };

            Controls.Add(tabs);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private TabPage CreateGeneralTab()
        {
            TabPage tab = CreateTab("Genel Bilgiler");

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(28, 24, 28, 16),
                BackColor = AppColors.CardBackground
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AddInfoRow(grid, "Bayi", GetText(magazaRow, "MusteriAdi", "-"));
            AddInfoRow(grid, "Mağaza / Şube", GetText(magazaRow, "MagazaAdi", "-"));
            AddInfoRow(grid, "Konum", GetKonumText(magazaRow));
            AddInfoRow(grid, "Telefon", GetText(magazaRow, "Telefon", "-"));
            AddInfoRow(grid, "Sorumlu", GetText(magazaRow, "SorumluKisi", "-"));
            AddInfoRow(grid, "Toplam Sipariş", GetInt(magazaRow, "SiparisSayisi") + " adet");
            AddInfoRow(grid, "Toplam Ciro", GetMoney(magazaRow, "ToplamCiro"));
            AddInfoRow(grid, "Son Sipariş", GetDate(magazaRow, "SonSiparisTarihi"));
            AddInfoRow(grid, "Durum", GetBool(magazaRow, "MagazaAktifMi") ? "Aktif" : "Pasif");

            tab.Controls.Add(grid);
            return tab;
        }

        private TabPage CreateYetkililerTab()
        {
            TabPage tab = CreateTab("Müşteriler / Yetkililer");
            Panel panel = CreateTabPanel();

            Panel top = CreateTabTop("Bu bayiye bağlı sipariş veren yetkili kişiler");
            Button btnYeni = CreateButton("+ Yeni Yetkili", true);
            btnYeni.Width = 130;
            btnYeni.Click += BtnYeniYetkili_Click;
            top.Controls.Add(btnYeni);
            top.Resize += (sender, e) => btnYeni.Location = new Point(top.Width - btnYeni.Width - 18, 12);

            dgvYetkililer = CreateGrid();
            AddGridColumn(dgvYetkililer, "AdSoyad", "Ad Soyad", 160);
            AddGridColumn(dgvYetkililer, "Gorev", "Görev", 140);
            AddGridColumn(dgvYetkililer, "Telefon", "Telefon", 120);
            AddGridColumn(dgvYetkililer, "Email", "E-Posta", 190);
            AddGridColumn(dgvYetkililer, "SiparisSayisi", "Sipariş", 80);
            AddGridColumn(dgvYetkililer, "AktifMi", "Aktif", 70);

            panel.Controls.Add(dgvYetkililer);
            panel.Controls.Add(top);
            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage CreateSiparislerTab()
        {
            TabPage tab = CreateTab("Siparişler");
            Panel panel = CreateTabPanel();

            panel.Controls.Add(CreateTabTop("Bu mağaza üzerinden oluşan siparişler"));
            dgvSiparisler = CreateGrid();
            AddGridColumn(dgvSiparisler, "SiparisId", "ID", 60);
            AddGridColumn(dgvSiparisler, "YetkiliAdi", "Yetkili", 140);
            AddGridColumn(dgvSiparisler, "UrunAdi", "Ürün", 190);
            AddGridColumn(dgvSiparisler, "Adet", "Adet", 70);
            AddGridColumn(dgvSiparisler, "ToplamTutar", "Tutar", 110);
            AddGridColumn(dgvSiparisler, "SiparisDurumu", "Durum", 120);
            AddGridColumn(dgvSiparisler, "SiparisTarihi", "Tarih", 130);
            dgvSiparisler.CellFormatting += DgvSiparisler_CellFormatting;

            panel.Controls.Add(dgvSiparisler);
            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage CreateStoklarTab()
        {
            TabPage tab = CreateTab("Stok İhtiyaçları");
            Panel panel = CreateTabPanel();

            panel.Controls.Add(CreateTabTop("Bayiye teslim edilmiş ürünler ve kritik stok durumu"));
            dgvStoklar = CreateGrid();
            AddGridColumn(dgvStoklar, "UrunAdi", "Ürün", 200);
            AddGridColumn(dgvStoklar, "KategoriAdi", "Kategori", 130);
            AddGridColumn(dgvStoklar, "BayiStok", "Bayi Stok", 90);
            AddGridColumn(dgvStoklar, "MinimumStok", "Min.", 70);
            AddGridColumn(dgvStoklar, "MerkezStok", "Merkez", 80);
            AddGridColumn(dgvStoklar, "StokDurumu", "Durum", 110);
            AddGridColumn(dgvStoklar, "SonHareketTarihi", "Son Hareket", 130);
            dgvStoklar.CellFormatting += DgvStoklar_CellFormatting;

            panel.Controls.Add(dgvStoklar);
            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage CreateCariTab()
        {
            TabPage tab = CreateTab("Cari / Notlar");
            Panel panel = CreateTabPanel();
            panel.Controls.Add(CreateEmptyState("Cari ve bakiye hareketleri için temel alan hazır. Ödeme tablosu eklendiğinde açık bakiye, tahsilat ve notlar burada beslenecek."));
            tab.Controls.Add(panel);
            return tab;
        }

        private void BayiDetayForm_Load(object sender, EventArgs e)
        {
            LoadYetkililer();
            LoadSiparisler();
            LoadStoklar();
        }

        private void LoadYetkililer()
        {
            try
            {
                dgvYetkililer.DataSource = yetkiliRepo.GetYetkililer("", -1, bayiId, magazaId);
            }
            catch (Exception ex)
            {
                ShowError("Yetkililer yüklenirken hata: " + ex.Message);
            }
        }

        private void LoadSiparisler()
        {
            try
            {
                dgvSiparisler.DataSource = orderRepo.GetAllOrders(magazaId, false);
            }
            catch (Exception ex)
            {
                ShowError("Siparişler yüklenirken hata: " + ex.Message);
            }
        }

        private void LoadStoklar()
        {
            try
            {
                dgvStoklar.DataSource = stokRepo.GetMagazaStoklari(
                    magazaId,
                    "",
                    false,
                    false,
                    true,
                    AppSession.KullaniciId,
                    AppSession.AdminMi);
            }
            catch (Exception ex)
            {
                ShowError("Stoklar yüklenirken hata: " + ex.Message);
            }
        }

        private void BtnYeniYetkili_Click(object sender, EventArgs e)
        {
            using (BayiYetkiliModalForm form = new BayiYetkiliModalForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.IsSaved)
                    LoadYetkililer();
            }
        }

        private TabPage CreateTab(string title)
        {
            return new TabPage
            {
                Text = title,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(0)
            };
        }

        private Panel CreateTabPanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(18)
            };
        }

        private Panel CreateTabTop(string text)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = AppColors.CardBackground
            };

            Label label = new Label
            {
                Text = text,
                Dock = DockStyle.Left,
                Width = 560,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            panel.Controls.Add(label);
            return panel;
        }

        private DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = AppColors.CardBackground,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
            };

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 40;
            grid.RowTemplate.Height = 42;
            grid.GridColor = AppColors.Border;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = AppColors.PrimarySoft;
            grid.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            return grid;
        }

        private void AddGridColumn(DataGridView grid, string propertyName, string header, int width)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = propertyName,
                DataPropertyName = propertyName,
                HeaderText = header,
                Width = width,
                FillWeight = Math.Max(6, width / 10),
                MinimumWidth = Math.Max(48, Math.Min(width, 110)),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void AddInfoRow(TableLayoutPanel panel, string label, string value)
        {
            int rowIndex = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label lblTitle = new Label
            {
                Text = label,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Margin = new Padding(0, 0, 0, 14)
            };

            Label lblValue = new Label
            {
                Text = value,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppColors.TextPrimary,
                Margin = new Padding(0, 0, 0, 14)
            };

            panel.Controls.Add(lblTitle, 0, rowIndex);
            panel.Controls.Add(lblValue, 1, rowIndex);
        }

        private Control CreateEmptyState(string text)
        {
            Label label = new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextSecondary,
                BackColor = AppColors.CardBackground
            };
            return label;
        }

        private Button CreateButton(string text, bool primary)
        {
            Button button = new Button
            {
                Text = text,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? AppColors.Primary : AppColors.PrimarySoft,
                ForeColor = primary ? Color.White : AppColors.Primary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void DgvSiparisler_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvSiparisler.Columns[e.ColumnIndex].Name;
            if (columnName == "ToplamTutar" && e.Value != null && e.Value != DBNull.Value)
            {
                e.Value = Convert.ToDecimal(e.Value).ToString("N2", new CultureInfo("tr-TR")) + " TL";
                e.FormattingApplied = true;
            }

            if (columnName == "SiparisTarihi" && e.Value != null && e.Value != DBNull.Value)
            {
                e.Value = Convert.ToDateTime(e.Value).ToString("dd.MM.yyyy HH:mm");
                e.FormattingApplied = true;
            }
        }

        private void DgvStoklar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvStoklar.Columns[e.ColumnIndex].Name;
            if (columnName == "SonHareketTarihi" && e.Value != null && e.Value != DBNull.Value)
            {
                e.Value = Convert.ToDateTime(e.Value).ToString("dd.MM.yyyy HH:mm");
                e.FormattingApplied = true;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string GetKonumText(DataRow row)
        {
            string sehir = GetText(row, "Sehir", "");
            string ilce = GetText(row, "Ilce", "");

            if (!string.IsNullOrWhiteSpace(sehir) && !string.IsNullOrWhiteSpace(ilce))
                return sehir + " / " + ilce;

            if (!string.IsNullOrWhiteSpace(sehir))
                return sehir;

            return "Konum girilmemiş";
        }

        private string GetDate(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return "-";

            return Convert.ToDateTime(row[columnName]).ToString("dd.MM.yyyy HH:mm");
        }

        private string GetMoney(DataRow row, string columnName)
        {
            decimal value = GetDecimal(row, columnName);
            return value.ToString("N2", new CultureInfo("tr-TR")) + " TL";
        }

        private string GetText(DataRow row, string columnName, string defaultValue)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            string value = Convert.ToString(row[columnName]);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private int GetInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(row[columnName]);
        }

        private decimal GetDecimal(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToDecimal(row[columnName]);
        }

        private bool GetBool(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return false;

            return Convert.ToBoolean(row[columnName]);
        }
    }
}
