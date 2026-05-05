using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Pages
{
    public class BayiStokPage : UserControl, IThemeable
    {
        private readonly MagazaStokRepository repo = new MagazaStokRepository();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private Panel gridPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblInfo;
        private TextBox txtArama;
        private CheckBox chkStokta;
        private CheckBox chkKritik;
        private Button btnTemizle;
        private DataGridView dgvStoklar;
        private CategoriesCard cToplamKart;
        private CategoriesCard cStokluUrun;
        private CategoriesCard cKritik;
        private CategoriesCard cTukendi;
        private Timer aramaTimer;
        private DataTable stokTable;

        public BayiStokPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            aramaTimer = new Timer { Interval = 350 };
            aramaTimer.Tick += AramaTimer_Tick;

            BuildLayout();
            Load += BayiStokPage_Load;
        }

        private void BuildLayout()
        {
            BuildHeader();
            BuildStats();
            BuildFilters();
            BuildGrid();

            Controls.Add(gridPanel);
            Controls.Add(filterPanel);
            Controls.Add(statsPanel);
            Controls.Add(headerPanel);
        }

        private void BuildHeader()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = "Bayi Stokları",
                Location = new Point(0, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            lblSubtitle = new Label
            {
                Text = "Teslim edilen siparişlerden oluşan mağaza stoklarını takip edin.",
                Location = new Point(2, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
        }

        private void BuildStats()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 116,
                BackColor = AppColors.Background
            };

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.Background
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            cToplamKart = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cStokluUrun = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cKritik = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cTukendi = new CategoriesCard { Dock = DockStyle.Fill, Margin = Padding.Empty };

            cToplamKart.SetData("□", "Stok Kartı", "0");
            cStokluUrun.SetData("□", "Stoklu Ürün", "0");
            cKritik.SetData("!", "Kritik", "0");
            cTukendi.SetData("-", "Tükendi", "0");

            grid.Controls.Add(cToplamKart, 0, 0);
            grid.Controls.Add(cStokluUrun, 1, 0);
            grid.Controls.Add(cKritik, 2, 0);
            grid.Controls.Add(cTukendi, 3, 0);

            statsPanel.Controls.Add(grid);
        }

        private void BuildFilters()
        {
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(16, 14, 16, 14)
            };

            txtArama = new TextBox
            {
                Width = 280,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 20),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
            txtArama.TextChanged += TxtArama_TextChanged;

            chkStokta = CreateCheckBox("Sadece stokta", 314);
            chkKritik = CreateCheckBox("Kritik / tükendi", 452);

            btnTemizle = new Button
            {
                Text = "Temizle",
                Width = 92,
                Height = 34,
                Location = new Point(610, 18),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.PrimarySoft,
                ForeColor = AppColors.Primary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTemizle.FlatAppearance.BorderSize = 0;
            btnTemizle.Click += BtnTemizle_Click;

            lblInfo = new Label
            {
                Text = "0 kayıt",
                Width = 180,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            filterPanel.Controls.Add(txtArama);
            filterPanel.Controls.Add(chkStokta);
            filterPanel.Controls.Add(chkKritik);
            filterPanel.Controls.Add(btnTemizle);
            filterPanel.Controls.Add(lblInfo);

            filterPanel.Resize += (sender, e) =>
            {
                lblInfo.Location = new Point(filterPanel.Width - lblInfo.Width - 16, 20);
                lblInfo.Visible = lblInfo.Left > btnTemizle.Right + 20;
            };
        }

        private CheckBox CreateCheckBox(string text, int x)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, 24),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            checkBox.CheckedChanged += (sender, e) => StoklariYukle();
            return checkBox;
        }

        private void BuildGrid()
        {
            gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(12)
            };

            dgvStoklar = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = AppColors.CardBackground,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvStoklar.EnableHeadersVisualStyles = false;
            dgvStoklar.ColumnHeadersHeight = 42;
            dgvStoklar.RowTemplate.Height = 48;
            dgvStoklar.GridColor = AppColors.Border;
            dgvStoklar.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvStoklar.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvStoklar.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvStoklar.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvStoklar.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvStoklar.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            dgvStoklar.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            dgvStoklar.CellFormatting += DgvStoklar_CellFormatting;

            ConfigureColumns();
            gridPanel.Controls.Add(dgvStoklar);
        }

        private void ConfigureColumns()
        {
            dgvStoklar.Columns.Clear();
            AddTextColumn("MusteriAdi", "Bayi", 150);
            AddTextColumn("MagazaAdi", "Mağaza", 150);
            AddTextColumn("UrunAdi", "Ürün", 180);
            AddTextColumn("KategoriAdi", "Kategori", 120);
            AddTextColumn("BayiStok", "Bayi Stok", 80);
            AddTextColumn("MinimumStok", "Min.", 70);
            AddTextColumn("MerkezStok", "Merkez", 80);
            AddTextColumn("StokDurumu", "Durum", 90);
            AddTextColumn("SonGirisTarihi", "Son Giriş", 110);
        }

        private void AddTextColumn(string name, string header, int width)
        {
            dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = header,
                Width = width
            });
        }

        private void BayiStokPage_Load(object sender, EventArgs e)
        {
            StoklariYukle();
        }

        private void StoklariYukle()
        {
            try
            {
                stokTable = repo.GetMagazaStoklari(
                    GetCurrentMagazaId(),
                    txtArama.Text.Trim(),
                    chkStokta.Checked,
                    chkKritik.Checked,
                    true,
                    AppSession.KullaniciId,
                    AppSession.AdminMi);

                dgvStoklar.DataSource = stokTable;
                lblInfo.Text = stokTable.Rows.Count + " kayıt";
                OzetleriYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OzetleriYukle()
        {
            DataRow row = repo.GetMagazaStokOzeti(
                GetCurrentMagazaId(),
                IsTumMagazalar(),
                AppSession.KullaniciId,
                AppSession.AdminMi);

            if (row == null)
            {
                cToplamKart.SetData("□", "Stok Kartı", "0");
                cStokluUrun.SetData("□", "Stoklu Ürün", "0");
                cKritik.SetData("!", "Kritik", "0");
                cTukendi.SetData("-", "Tükendi", "0");
                return;
            }

            cToplamKart.SetData("□", "Stok Kartı", GetInt(row, "ToplamStokKarti").ToString());
            cStokluUrun.SetData("□", "Stoklu Ürün", GetInt(row, "StokluUrunSayisi").ToString());
            cKritik.SetData("!", "Kritik", GetInt(row, "KritikStokKarti").ToString());
            cTukendi.SetData("-", "Tükendi", GetInt(row, "TukenmisStokKarti").ToString());
        }

        private int? GetCurrentMagazaId()
        {
            if (IsTumMagazalar())
                return null;

            return AppSession.SeciliMagazaId;
        }

        private bool IsTumMagazalar()
        {
            return AppSession.AdminMi && (AppSession.TumMagazalar || !AppSession.SeciliMagazaId.HasValue);
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private void AramaTimer_Tick(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            StoklariYukle();
        }

        private void BtnTemizle_Click(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            txtArama.Clear();
            chkStokta.Checked = false;
            chkKritik.Checked = false;
            StoklariYukle();
        }

        private void DgvStoklar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvStoklar.Columns[e.ColumnIndex].Name;

            if (columnName == "BayiStok" || columnName == "MinimumStok" || columnName == "MerkezStok")
            {
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            if (columnName == "SonGirisTarihi" && e.Value != null && e.Value != DBNull.Value)
            {
                e.Value = Convert.ToDateTime(e.Value).ToString("dd.MM.yyyy", new CultureInfo("tr-TR"));
                e.FormattingApplied = true;
            }

            if (columnName == "StokDurumu")
            {
                string text = Convert.ToString(e.Value);

                if (text == "Tukendi")
                    e.Value = "Tükendi";

                if (text == "Tukendi")
                    e.CellStyle.ForeColor = AppColors.Danger;
                else if (text == "Kritik")
                    e.CellStyle.ForeColor = AppColors.Warning;
                else
                    e.CellStyle.ForeColor = AppColors.Success;

                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                e.FormattingApplied = true;
            }
        }

        private int GetInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(row[columnName]);
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;

            if (headerPanel != null)
                headerPanel.BackColor = AppColors.Background;

            if (statsPanel != null)
                statsPanel.BackColor = AppColors.Background;

            if (filterPanel != null)
                filterPanel.BackColor = AppColors.CardBackground;

            if (gridPanel != null)
                gridPanel.BackColor = AppColors.CardBackground;

            if (lblTitle != null)
                lblTitle.ForeColor = AppColors.TextPrimary;

            if (lblSubtitle != null)
                lblSubtitle.ForeColor = AppColors.TextSecondary;

            if (lblInfo != null)
                lblInfo.ForeColor = AppColors.TextSecondary;
        }
    }
}
