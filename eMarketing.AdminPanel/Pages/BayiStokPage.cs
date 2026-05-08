using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
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
        private Panel adminPanel;
        private Panel gridPanel;
        private Panel hareketPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblInfo;
        private Label lblHareketBaslik;
        private Label lblHareketInfo;
        private Label lblSeciliStok;
        private TextBox txtHareketMiktar;
        private TextBox txtMinimumStok;
        private Button btnStokGirisi;
        private Button btnStokCikisi;
        private Button btnMinimumGuncelle;
        private TextBox txtArama;
        private CheckBox chkStokta;
        private CheckBox chkKritik;
        private Button btnTemizle;
        private DataGridView dgvStoklar;
        private DataGridView dgvHareketler;
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
            BuildAdminPanel();
            BuildGrid();

            Controls.Add(gridPanel);
            Controls.Add(adminPanel);
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

        private void BuildAdminPanel()
        {
            adminPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = AppSession.AdminMi ? 84 : 0,
                Visible = AppSession.AdminMi,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(16, 12, 16, 12)
            };

            Label title = new Label
            {
                Text = "Stok işlemi",
                Location = new Point(16, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            lblSeciliStok = new Label
            {
                Text = "Bir stok kartı seçin.",
                Location = new Point(16, 38),
                Width = 360,
                Height = 24,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            Label lblMiktar = CreateSmallLabel("Miktar", 396, 14);
            txtHareketMiktar = CreateAdminTextBox(396, 38, 76);

            Label lblMinimum = CreateSmallLabel("Minimum", 486, 14);
            txtMinimumStok = CreateAdminTextBox(486, 38, 84);

            btnStokGirisi = CreateAdminButton("+ Giriş", AppColors.Success, 594);
            btnStokCikisi = CreateAdminButton("- Çıkış", AppColors.Danger, 692);
            btnMinimumGuncelle = CreateAdminButton("Min. Güncelle", AppColors.Primary, 790);

            btnStokGirisi.Click += (sender, e) => StokHareketiYap("ManuelGiris");
            btnStokCikisi.Click += (sender, e) => StokHareketiYap("ManuelCikis");
            btnMinimumGuncelle.Click += BtnMinimumGuncelle_Click;

            adminPanel.Controls.Add(title);
            adminPanel.Controls.Add(lblSeciliStok);
            adminPanel.Controls.Add(lblMiktar);
            adminPanel.Controls.Add(txtHareketMiktar);
            adminPanel.Controls.Add(lblMinimum);
            adminPanel.Controls.Add(txtMinimumStok);
            adminPanel.Controls.Add(btnStokGirisi);
            adminPanel.Controls.Add(btnStokCikisi);
            adminPanel.Controls.Add(btnMinimumGuncelle);
        }

        private Label CreateSmallLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };
        }

        private TextBox CreateAdminTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
        }

        private Button CreateAdminButton(string text, Color backColor, int x)
        {
            Button button = new Button
            {
                Text = text,
                Width = text.Length > 8 ? 126 : 84,
                Height = 34,
                Location = new Point(x, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
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
            dgvStoklar.SelectionChanged += DgvStoklar_SelectionChanged;

            ConfigureColumns();
            BuildHareketPanel();
            gridPanel.Controls.Add(dgvStoklar);
            gridPanel.Controls.Add(hareketPanel);
        }

        private void BuildHareketPanel()
        {
            hareketPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 190,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(0, 12, 0, 0)
            };

            lblHareketBaslik = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Text = "Hareket Geçmişi",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            lblHareketInfo = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                Text = "Stok kartı seçildiğinde son hareketler burada görünür.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            dgvHareketler = new DataGridView
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
            };

            dgvHareketler.EnableHeadersVisualStyles = false;
            dgvHareketler.ColumnHeadersHeight = 34;
            dgvHareketler.RowTemplate.Height = 36;
            dgvHareketler.GridColor = AppColors.Border;
            dgvHareketler.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvHareketler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgvHareketler.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvHareketler.DefaultCellStyle.Font = new Font("Segoe UI", 8.5F);
            dgvHareketler.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvHareketler.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            dgvHareketler.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            dgvHareketler.CellFormatting += DgvHareketler_CellFormatting;

            ConfigureHareketColumns();

            hareketPanel.Controls.Add(dgvHareketler);
            hareketPanel.Controls.Add(lblHareketInfo);
            hareketPanel.Controls.Add(lblHareketBaslik);
        }

        private void ConfigureColumns()
        {
            dgvStoklar.Columns.Clear();
            AddHiddenColumn("MagazaStokId");
            AddHiddenColumn("MagazaId");
            AddHiddenColumn("UrunId");
            AddHiddenColumn("GorselUrl");
            AddImageColumn("UrunGorsel", "Görsel", 7, 48);
            AddTextColumn("MusteriAdi", "Bayi", 18, 120);
            AddTextColumn("MagazaAdi", "Mağaza", 16, 110);
            AddTextColumn("SorumluKisi", "Sorumlu", 12, 90);
            AddTextColumn("UrunAdi", "Ürün", 18, 120);
            AddTextColumn("KategoriAdi", "Kategori", 10, 82);
            AddTextColumn("BayiStok", "Bayi", 7, 58);
            AddTextColumn("MinimumStok", "Min.", 7, 56);
            AddTextColumn("MerkezStok", "Merkez", 7, 60);
            AddTextColumn("StokDurumu", "Durum", 9, 72);
            AddTextColumn("SonGirisTarihi", "Son Giriş", 9, 78);
        }

        private void AddHiddenColumn(string name)
        {
            dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = name,
                Visible = false
            });
        }

        private void AddImageColumn(string name, string header, float fillWeight, int minWidth)
        {
            dgvStoklar.Columns.Add(new DataGridViewImageColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = header,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = fillWeight,
                MinimumWidth = minWidth,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            });
        }

        private void AddTextColumn(string name, string header, float fillWeight, int minWidth)
        {
            dgvStoklar.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = header,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = fillWeight,
                MinimumWidth = minWidth
            });
        }

        private void ConfigureHareketColumns()
        {
            dgvHareketler.Columns.Clear();
            AddHareketColumn("OlusturmaTarihi", "Tarih", 14, 110);
            AddHareketColumn("HareketYonu", "Yön", 10, 78);
            AddHareketColumn("HareketAciklama", "İşlem", 20, 130);
            AddHareketColumn("Miktar", "Miktar", 8, 60);
            AddHareketColumn("OncekiStok", "Önce", 8, 58);
            AddHareketColumn("SonrakiStok", "Sonra", 8, 58);
            AddHareketColumn("SiparisNo", "Sipariş", 12, 88);
            AddHareketColumn("Aciklama", "Açıklama", 21, 140);
        }

        private void AddHareketColumn(string name, string header, float fillWeight, int minWidth)
        {
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = header,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = fillWeight,
                MinimumWidth = minWidth
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

                PrepareStokTable(stokTable);
                dgvStoklar.DataSource = stokTable;
                UpdateAdminSelection();
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

        private void PrepareStokTable(DataTable table)
        {
            if (table == null)
                return;

            if (!table.Columns.Contains("UrunGorsel"))
                table.Columns.Add("UrunGorsel", typeof(Image));

            foreach (DataRow row in table.Rows)
            {
                string imagePath = "";

                if (row.Table.Columns.Contains("GorselUrl") && row["GorselUrl"] != DBNull.Value)
                    imagePath = Convert.ToString(row["GorselUrl"]);

                row["UrunGorsel"] = LoadProductThumbnail(imagePath);
            }
        }

        private Image LoadProductThumbnail(string imagePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    string fullPath = imagePath;

                    if (!Path.IsPathRooted(fullPath))
                        fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);

                    if (File.Exists(fullPath))
                    {
                        using (Image tempImage = Image.FromFile(fullPath))
                        {
                            return new Bitmap(tempImage, new Size(40, 40));
                        }
                    }
                }
            }
            catch
            {
            }

            return CreateProductPlaceholder();
        }

        private Image CreateProductPlaceholder()
        {
            Bitmap bitmap = new Bitmap(40, 40);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (SolidBrush background = new SolidBrush(AppColors.PrimarySoft))
            using (SolidBrush foreground = new SolidBrush(AppColors.Primary))
            using (Pen border = new Pen(AppColors.Border))
            using (Font font = new Font("Segoe UI", 12F, FontStyle.Bold))
            {
                graphics.Clear(Color.Transparent);
                graphics.FillRectangle(background, 2, 2, 36, 36);
                graphics.DrawRectangle(border, 2, 2, 35, 35);

                StringFormat format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.DrawString("P", font, foreground, new RectangleF(2, 2, 36, 36), format);
            }

            return bitmap;
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

        private void DgvStoklar_SelectionChanged(object sender, EventArgs e)
        {
            UpdateAdminSelection();
        }

        private void UpdateAdminSelection()
        {
            DataGridViewRow row = GetSelectedGridRow();

            if (row == null)
            {
                lblSeciliStok.Text = "Bir stok kartı seçin.";
                txtMinimumStok.Clear();
                HareketleriTemizle("Stok kartı seçildiğinde son hareketler burada görünür.");
                return;
            }

            string bayi = GetCellText(row, "MusteriAdi");
            string urun = GetCellText(row, "UrunAdi");
            int bayiStok = GetCellInt(row, "BayiStok");
            int minimumStok = GetCellInt(row, "MinimumStok");

            lblSeciliStok.Text = bayi + " / " + urun + " - stok: " + bayiStok;
            txtMinimumStok.Text = minimumStok.ToString();
            HareketleriYukle(row, bayi, urun);
        }

        private void HareketleriYukle(DataGridViewRow row, string bayi, string urun)
        {
            if (dgvHareketler == null)
                return;

            int magazaId = GetCellInt(row, "MagazaId");
            int urunId = GetCellInt(row, "UrunId");

            if (magazaId <= 0 || urunId <= 0)
            {
                HareketleriTemizle("Hareket geçmişi için bayi ve ürün bilgisi eksik.");
                return;
            }

            try
            {
                DataTable hareketler = repo.GetStokHareketleri(
                    magazaId,
                    urunId,
                    25,
                    AppSession.KullaniciId,
                    AppSession.AdminMi);

                dgvHareketler.DataSource = hareketler;

                if (hareketler.Rows.Count == 0)
                    lblHareketInfo.Text = bayi + " / " + urun + " için henüz stok hareketi yok.";
                else
                    lblHareketInfo.Text = bayi + " / " + urun + " için son " + hareketler.Rows.Count + " hareket";
            }
            catch (Exception ex)
            {
                HareketleriTemizle("Hareket geçmişi yüklenemedi: " + ex.Message);
            }
        }

        private void HareketleriTemizle(string message)
        {
            if (dgvHareketler != null)
                dgvHareketler.DataSource = null;

            if (lblHareketInfo != null)
                lblHareketInfo.Text = message;
        }

        private void StokHareketiYap(string hareketTipi)
        {
            if (!AppSession.AdminMi)
                return;

            DataGridViewRow row = GetSelectedGridRow();

            if (row == null)
            {
                MessageBox.Show("Lütfen önce bir bayi stok kartı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryGetPositiveInt(txtHareketMiktar.Text, out int miktar))
            {
                MessageBox.Show("Stok miktarı sıfırdan büyük bir sayı olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHareketMiktar.Focus();
                return;
            }

            int magazaId = GetCellInt(row, "MagazaId");
            int urunId = GetCellInt(row, "UrunId");
            int mevcutStok = GetCellInt(row, "BayiStok");

            if (magazaId <= 0 || urunId <= 0)
            {
                MessageBox.Show("Seçili stok kartı için bayi veya ürün bilgisi eksik.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool cikisMi = hareketTipi == "ManuelCikis";

            if (cikisMi && miktar > mevcutStok)
            {
                MessageBox.Show("Bayi stoğu eksiye düşemez. Çıkış miktarını kontrol edin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string islemAdi = cikisMi ? "stok çıkışı" : "stok girişi";
            DialogResult result = MessageBox.Show(
                "Seçili bayi için " + miktar + " adet " + islemAdi + " yapılacak. Devam edilsin mi?",
                "Stok İşlemi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                repo.StokHareketiIsle(
                    magazaId,
                    urunId,
                    hareketTipi,
                    miktar,
                    "Admin panel manuel " + islemAdi);

                txtHareketMiktar.Clear();
                StoklariYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMinimumGuncelle_Click(object sender, EventArgs e)
        {
            if (!AppSession.AdminMi)
                return;

            DataGridViewRow row = GetSelectedGridRow();

            if (row == null)
            {
                MessageBox.Show("Lütfen önce bir bayi stok kartı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryGetNonNegativeInt(txtMinimumStok.Text, out int minimumStok))
            {
                MessageBox.Show("Minimum stok negatif olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMinimumStok.Focus();
                return;
            }

            int magazaStokId = GetCellInt(row, "MagazaStokId");

            if (magazaStokId <= 0)
            {
                MessageBox.Show("Seçili stok kartı bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                repo.MinimumStokGuncelle(magazaStokId, minimumStok);
                StoklariYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataGridViewRow GetSelectedGridRow()
        {
            if (dgvStoklar == null || dgvStoklar.CurrentRow == null || dgvStoklar.CurrentRow.IsNewRow)
                return null;

            return dgvStoklar.CurrentRow;
        }

        private bool TryGetPositiveInt(string text, out int value)
        {
            return int.TryParse(text, out value) && value > 0;
        }

        private bool TryGetNonNegativeInt(string text, out int value)
        {
            return int.TryParse(text, out value) && value >= 0;
        }

        private int GetCellInt(DataGridViewRow row, string columnName)
        {
            if (row == null || !dgvStoklar.Columns.Contains(columnName))
                return 0;

            object value = row.Cells[columnName].Value;

            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToInt32(value);
        }

        private string GetCellText(DataGridViewRow row, string columnName)
        {
            if (row == null || !dgvStoklar.Columns.Contains(columnName))
                return "";

            object value = row.Cells[columnName].Value;

            if (value == null || value == DBNull.Value)
                return "";

            return Convert.ToString(value);
        }

        private void DgvHareketler_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvHareketler.Columns[e.ColumnIndex].Name;

            if (columnName == "OlusturmaTarihi" && e.Value != null && e.Value != DBNull.Value)
            {
                e.Value = Convert.ToDateTime(e.Value).ToString("dd.MM.yyyy HH:mm", new CultureInfo("tr-TR"));
                e.FormattingApplied = true;
            }

            if (columnName == "Miktar" || columnName == "OncekiStok" || columnName == "SonrakiStok")
            {
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            }

            if (columnName == "HareketYonu")
            {
                string text = Convert.ToString(e.Value);
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

                if (text == "Giriş")
                {
                    e.Value = "+ Giriş";
                    e.CellStyle.ForeColor = AppColors.Success;
                    e.FormattingApplied = true;
                }
                else if (text == "Çıkış")
                {
                    e.Value = "- Çıkış";
                    e.CellStyle.ForeColor = AppColors.Danger;
                    e.FormattingApplied = true;
                }
                else
                    e.CellStyle.ForeColor = AppColors.TextSecondary;
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

            if (adminPanel != null)
                adminPanel.BackColor = AppColors.CardBackground;

            if (gridPanel != null)
                gridPanel.BackColor = AppColors.CardBackground;

            if (hareketPanel != null)
                hareketPanel.BackColor = AppColors.CardBackground;

            if (lblTitle != null)
                lblTitle.ForeColor = AppColors.TextPrimary;

            if (lblSubtitle != null)
                lblSubtitle.ForeColor = AppColors.TextSecondary;

            if (lblInfo != null)
                lblInfo.ForeColor = AppColors.TextSecondary;

            if (lblHareketBaslik != null)
                lblHareketBaslik.ForeColor = AppColors.TextPrimary;

            if (lblHareketInfo != null)
                lblHareketInfo.ForeColor = AppColors.TextSecondary;

            if (lblSeciliStok != null)
                lblSeciliStok.ForeColor = AppColors.TextSecondary;

            if (txtArama != null)
            {
                txtArama.BackColor = AppColors.InputBackground;
                txtArama.ForeColor = AppColors.TextPrimary;
            }

            if (txtHareketMiktar != null)
            {
                txtHareketMiktar.BackColor = AppColors.InputBackground;
                txtHareketMiktar.ForeColor = AppColors.TextPrimary;
            }

            if (txtMinimumStok != null)
            {
                txtMinimumStok.BackColor = AppColors.InputBackground;
                txtMinimumStok.ForeColor = AppColors.TextPrimary;
            }

            if (btnTemizle != null)
            {
                btnTemizle.BackColor = AppColors.PrimarySoft;
                btnTemizle.ForeColor = AppColors.Primary;
            }

            if (btnStokGirisi != null)
                btnStokGirisi.BackColor = AppColors.Success;

            if (btnStokCikisi != null)
                btnStokCikisi.BackColor = AppColors.Danger;

            if (btnMinimumGuncelle != null)
                btnMinimumGuncelle.BackColor = AppColors.Primary;

            if (dgvHareketler != null)
            {
                dgvHareketler.BackgroundColor = AppColors.CardBackground;
                dgvHareketler.GridColor = AppColors.Border;
                dgvHareketler.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvHareketler.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvHareketler.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }
        }
    }
}
