using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class BayiStokPage : UserControl, IThemeable
    {
        private readonly ApiDataClient apiClient = new ApiDataClient();

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
        private Button btnMerkezStokArtir;
        private Button btnMinimumGuncelle;
        private TextBox txtArama;
        private CheckBox chkStokta;
        private CheckBox chkKritik;
        private Button btnViewMode;
        private Button btnTemizle;
        private DataGridView dgvStoklar;
        private DataGridView dgvHareketler;
        private FlowLayoutPanel stockCardsPanel;
        private CategoriesCard cToplamKart;
        private CategoriesCard cStokluUrun;
        private CategoriesCard cKritik;
        private CategoriesCard cTukendi;
        private Timer aramaTimer;
        private DataTable stokTable;
        private bool cardViewActive = true;
        private int? selectedStockId;
        private Label lblStokEmptyState;
        private Label lblHareketEmptyState;
        private const string TotalCardIcon = "▤";
        private const string StockedCardIcon = "✓";
        private const string CriticalCardIcon = "!";
        private const string EmptyCardIcon = "×";
        private const string TotalStockCardTitle = "Stok Kartı";
        private const string StockedProductTitle = "Stoklu Ürün";
        private const string EmptyStockTitle = "Tükendi";
        private const string SelectStockMessage = "Bir stok kartı seçin.";
        private const string StockHistoryIntro = "Stok kartı seçildiğinde son hareketler burada görünür.";
        private const string WarningTitle = "Uyarı";
        private const string DirectionIn = "Giriş";
        private const string DirectionOut = "Çıkış";

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
            Controls.Add(filterPanel);
            Controls.Add(statsPanel);
            Controls.Add(headerPanel);
        }

        private void BuildHeader()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
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
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblTitle);
        }
        private void BuildStats()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
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

            cToplamKart.SetData(TotalCardIcon, TotalStockCardTitle, "0");
            cStokluUrun.SetData(StockedCardIcon, StockedProductTitle, "0");
            cKritik.SetData(CriticalCardIcon, "Kritik", "0");
            cTukendi.SetData(EmptyCardIcon, EmptyStockTitle, "0");

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
                Height = 58,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(16, 10, 16, 10)
            };

            txtArama = new TextBox
            {
                Width = 280,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 14),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
            txtArama.TextChanged += TxtArama_TextChanged;
            ButtonStyleHelper.ApplyInput(txtArama);

            chkStokta = CreateCheckBox("Sadece stokta", 314);
            chkKritik = CreateCheckBox("Kritik / tükendi", 452);

            btnViewMode = new Button
            {
                Text = "Tablo",
                Width = 82,
                Height = 34,
                Location = new Point(596, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.PrimarySoft,
                ForeColor = AppColors.Primary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewMode.FlatAppearance.BorderSize = 0;
            ButtonStyleHelper.ApplySoft(btnViewMode);
            btnViewMode.Click += BtnViewMode_Click;

            btnTemizle = new Button
            {
                Text = "Temizle",
                Width = 92,
                Height = 34,
                Location = new Point(690, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.PrimarySoft,
                ForeColor = AppColors.Primary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTemizle.FlatAppearance.BorderSize = 0;
            ButtonStyleHelper.ApplyOutline(btnTemizle);
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
            DataGridViewStyleHelper.UpdateCountLabel(lblInfo, 0, 0);

            filterPanel.Controls.Add(txtArama);
            filterPanel.Controls.Add(chkStokta);
            filterPanel.Controls.Add(chkKritik);
            filterPanel.Controls.Add(btnViewMode);
            filterPanel.Controls.Add(btnTemizle);
            filterPanel.Controls.Add(lblInfo);

            filterPanel.Resize += (sender, e) => UpdateFilterLayout();
            UpdateFilterLayout();
        }
        private CheckBox CreateCheckBox(string text, int x)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = text,
                AutoSize = true,
                Location = new Point(x, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            checkBox.CheckedChanged += async (sender, e) => await StoklariYukleAsync();
            return checkBox;
        }

        private void BuildAdminPanel()
        {
            adminPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = AppSession.AdminMi ? 104 : 0,
                Visible = AppSession.AdminMi,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(18, 16, 18, 16)
            };

            Label title = new Label
            {
                Text = "Stok işlemi",
                Location = new Point(18, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            lblSeciliStok = new Label
            {
                Text = SelectStockMessage,
                Location = new Point(18, 44),
                Width = 360,
                Height = 24,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            Label lblMiktar = CreateSmallLabel("Miktar", 420, 20);
            txtHareketMiktar = CreateAdminTextBox(420, 48, 76);

            Label lblMinimum = CreateSmallLabel("Minimum", 510, 20);
            txtMinimumStok = CreateAdminTextBox(510, 48, 84);

            btnStokGirisi = CreateAdminButton("+ Bayi Giriş", AppColors.Success, 620);
            btnStokCikisi = CreateAdminButton("- Bayi Çıkış", AppColors.Danger, 718);
            btnMerkezStokArtir = CreateAdminButton("+ Merkez", AppColors.Primary, 816);
            btnMinimumGuncelle = CreateAdminButton("Min. Güncelle", AppColors.Primary, 922);

            btnStokGirisi.Click += (sender, e) => StokHareketiYap("ManuelGiris");
            btnStokCikisi.Click += (sender, e) => StokHareketiYap("ManuelCikis");
            btnMerkezStokArtir.Click += BtnMerkezStokArtir_Click;
            btnMinimumGuncelle.Click += BtnMinimumGuncelle_Click;

            ButtonStyleHelper.ApplySuccess(btnStokGirisi);
            ButtonStyleHelper.ApplyDanger(btnStokCikisi);
            ButtonStyleHelper.ApplyPrimary(btnMerkezStokArtir);
            ButtonStyleHelper.ApplyPrimary(btnMinimumGuncelle);

            adminPanel.Controls.Add(title);
            adminPanel.Controls.Add(lblSeciliStok);
            adminPanel.Controls.Add(lblMiktar);
            adminPanel.Controls.Add(txtHareketMiktar);
            adminPanel.Controls.Add(lblMinimum);
            adminPanel.Controls.Add(txtMinimumStok);
            adminPanel.Controls.Add(btnStokGirisi);
            adminPanel.Controls.Add(btnStokCikisi);
            adminPanel.Controls.Add(btnMerkezStokArtir);
            adminPanel.Controls.Add(btnMinimumGuncelle);
            adminPanel.Resize += (sender, e) => UpdateAdminPanelLayout();
            UpdateAdminPanelLayout();
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
                ScrollBars = ScrollBars.Both
            };

            DataGridViewStyleHelper.ApplyModernGrid(dgvStoklar);
            dgvStoklar.CellFormatting += DgvStoklar_CellFormatting;
            dgvStoklar.CellPainting += DgvStoklar_CellPainting;
            dgvStoklar.SelectionChanged += DgvStoklar_SelectionChanged;

            stockCardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(4, 2, 4, 2),
                BackColor = AppColors.CardBackground
            };
            stockCardsPanel.Resize += (sender, e) => FitStockCards();

            ConfigureColumns();
            BuildHareketPanel();

            Panel listHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground
            };
            listHost.Controls.Add(dgvStoklar);
            listHost.Controls.Add(stockCardsPanel);
            ApplyStockViewMode();

            TableLayoutPanel contentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = AppColors.CardBackground
            };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, AppSession.AdminMi ? 110F : 0F));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 210F));

            listHost.Margin = new Padding(0, 0, 0, 14);
            adminPanel.Margin = new Padding(0, 0, 0, 14);
            hareketPanel.Margin = Padding.Empty;

            contentLayout.Controls.Add(listHost, 0, 0);
            contentLayout.Controls.Add(adminPanel, 0, 1);
            contentLayout.Controls.Add(hareketPanel, 0, 2);
            gridPanel.Controls.Add(contentLayout);
        }

        private void BuildHareketPanel()
        {
            hareketPanel = new Panel
            {
                Dock = DockStyle.Fill,
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
                Text = StockHistoryIntro,
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
                ScrollBars = ScrollBars.Both
            };

            DataGridViewStyleHelper.ApplyModernGrid(dgvHareketler);
            dgvHareketler.ColumnHeadersHeight = 36;
            dgvHareketler.RowTemplate.Height = 38;
            dgvHareketler.CellFormatting += DgvHareketler_CellFormatting;
            dgvHareketler.CellPainting += DgvHareketler_CellPainting;

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

        private async void BayiStokPage_Load(object sender, EventArgs e)
        {
            await StoklariYukleAsync();
        }

        private async Task StoklariYukleAsync()
        {
            try
            {
                stokTable = await GetStoklarAsync();

                PrepareStokTable(stokTable);
                dgvStoklar.DataSource = stokTable;
                RenderStockCards(stokTable);
                UpdateAdminSelection();
                DataGridViewStyleHelper.UpdateCountLabel(lblInfo, stokTable.Rows.Count, stokTable.Rows.Count);
                ToggleGridEmptyState(dgvStoklar, ref lblStokEmptyState, "Stok listesinde gösterilecek kayıt bulunamadı.");
                await OzetleriYukleAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task OzetleriYukleAsync()
        {
            DataRow row = await GetStokOzetiAsync();

            if (row == null)
            {
                cToplamKart.SetData(TotalCardIcon, TotalStockCardTitle, "0");
                cStokluUrun.SetData(StockedCardIcon, StockedProductTitle, "0");
                cKritik.SetData(CriticalCardIcon, "Kritik", "0");
                cTukendi.SetData(EmptyCardIcon, EmptyStockTitle, "0");
                return;
            }

            cToplamKart.SetData(TotalCardIcon, TotalStockCardTitle, GetInt(row, "ToplamStokKarti").ToString());
            cStokluUrun.SetData(StockedCardIcon, StockedProductTitle, GetInt(row, "StokluUrunSayisi").ToString());
            cKritik.SetData(CriticalCardIcon, "Kritik", GetInt(row, "KritikStokKarti").ToString());
            cTukendi.SetData(EmptyCardIcon, EmptyStockTitle, GetInt(row, "TukenmisStokKarti").ToString());
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

        private void BtnViewMode_Click(object sender, EventArgs e)
        {
            cardViewActive = !cardViewActive;
            ApplyStockViewMode();
        }

        private void ApplyStockViewMode()
        {
            if (stockCardsPanel != null)
                stockCardsPanel.Visible = cardViewActive;

            if (dgvStoklar != null)
                dgvStoklar.Visible = !cardViewActive;

            if (btnViewMode != null)
                btnViewMode.Text = cardViewActive ? "Tablo" : "Kartlar";
        }

        private void RenderStockCards(DataTable table)
        {
            if (stockCardsPanel == null)
                return;

            stockCardsPanel.SuspendLayout();
            stockCardsPanel.Controls.Clear();

            if (table == null || table.Rows.Count == 0)
            {
                stockCardsPanel.Controls.Add(CreateStockEmptyCard());
                stockCardsPanel.ResumeLayout(true);
                return;
            }

            foreach (DataRow row in table.Rows)
                stockCardsPanel.Controls.Add(CreateStockCard(row));

            FitStockCards();
            RefreshStockCardSelection();
            stockCardsPanel.ResumeLayout(true);
        }

        private Panel CreateStockCard(DataRow row)
        {
            Panel card = new Panel
            {
                Width = 300,
                Height = 126,
                Margin = new Padding(0, 0, 18, 14),
                Padding = new Padding(12),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = row
            };
            card.Paint += StockCard_Paint;

            PictureBox image = new PictureBox
            {
                Width = 56,
                Height = 56,
                Location = new Point(12, 16),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = row.Table.Columns.Contains("UrunGorsel") ? row["UrunGorsel"] as Image : null,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            Label id = CreateStockCardLabel("Stok #" + GetText(row, "MagazaStokId", "-") + "  |  Ürün #" + GetText(row, "UrunId", "-"), 8F, FontStyle.Bold, AppColors.Primary, 18);
            id.Location = new Point(82, 12);
            id.Width = card.Width - 98;

            Label product = CreateStockCardLabel(GetText(row, "UrunAdi", "Ürün"), 10.5F, FontStyle.Bold, AppColors.TextPrimary, 24);
            product.Location = new Point(82, 34);
            product.Width = card.Width - 98;

            Label store = CreateStockCardLabel(GetText(row, "MusteriAdi", "Bayi") + " / " + GetText(row, "MagazaAdi", "Mağaza"), 8.5F, FontStyle.Regular, AppColors.TextSecondary, 24);
            store.Location = new Point(82, 58);
            store.Width = card.Width - 98;

            Label stock = CreateStockBadge("Bayi: " + GetText(row, "BayiStok", "0"), AppColors.PrimarySoft, AppColors.Primary);
            stock.Location = new Point(12, 88);

            Label min = CreateStockBadge("Min: " + GetText(row, "MinimumStok", "0"), Color.FromArgb(248, 250, 252), AppColors.TextSecondary);
            min.Location = new Point(104, 88);

            string statusText = NormalizeStockStatus(GetText(row, "StokDurumu", "Yeterli"));
            Label status = CreateStockBadge(statusText, GetStockBadgeBack(statusText), GetStockBadgeFore(statusText));
            status.Location = new Point(196, 88);

            card.Controls.Add(image);
            card.Controls.Add(id);
            card.Controls.Add(product);
            card.Controls.Add(store);
            card.Controls.Add(stock);
            card.Controls.Add(min);
            card.Controls.Add(status);

            AttachStockCardClick(card, row);
            AddStockCardHover(card);

            return card;
        }
        private Panel CreateStockEmptyCard()
        {
            Panel card = new Panel
            {
                Width = 320,
                Height = 90,
                Margin = new Padding(0, 0, 16, 16),
                BackColor = Color.White
            };
            card.Paint += StockCard_Paint;
            Label label = CreateStockCardLabel("Stok kartı bulunamadı.", 10F, FontStyle.Bold, AppColors.TextSecondary, 80);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(label);
            return card;
        }
        private Label CreateStockCardLabel(string text, float size, FontStyle style, Color color, int height)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Height = height,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
        }

        private Label CreateStockBadge(string text, Color backColor, Color foreColor)
        {
            return new Label
            {
                Text = text,
                Width = 86,
                Height = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = foreColor
            };
        }

        private void AttachStockCardClick(Control control, DataRow row)
        {
            control.Click += (sender, e) => SelectStockCard(row);
            foreach (Control child in control.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += (sender, e) => SelectStockCard(row);
            }
        }

        private void SelectStockCard(DataRow row)
        {
            int stockId = GetInt(row, "MagazaStokId");
            selectedStockId = stockId > 0 ? (int?)stockId : null;

            if (dgvStoklar != null && stockId > 0)
            {
                foreach (DataGridViewRow gridRow in dgvStoklar.Rows)
                {
                    if (gridRow.IsNewRow)
                        continue;

                    object value = gridRow.Cells["MagazaStokId"].Value;
                    if (value != null && value != DBNull.Value && Convert.ToInt32(value) == stockId)
                    {
                        dgvStoklar.CurrentCell = gridRow.Cells["UrunAdi"];
                        gridRow.Selected = true;
                        break;
                    }
                }
            }

            UpdateAdminSelection();
            RefreshStockCardSelection();
        }

        private void AddStockCardHover(Panel card)
        {
            EventHandler enter = (sender, e) => { if (!IsSelectedStockCard(card)) card.BackColor = Color.FromArgb(239, 246, 255); card.Invalidate(); };
            EventHandler leave = (sender, e) => { if (!IsSelectedStockCard(card)) card.BackColor = Color.White; card.Invalidate(); };
            card.MouseEnter += enter;
            card.MouseLeave += leave;
            foreach (Control child in card.Controls)
            {
                child.MouseEnter += enter;
                child.MouseLeave += leave;
            }
        }

        private void RefreshStockCardSelection()
        {
            if (stockCardsPanel == null)
                return;

            foreach (Control control in stockCardsPanel.Controls)
            {
                Panel card = control as Panel;
                if (card == null)
                    continue;

                card.BackColor = IsSelectedStockCard(card) ? AppColors.PrimarySoft : Color.White;
                card.Invalidate();
            }
        }

        private bool IsSelectedStockCard(Panel card)
        {
            if (card == null || !selectedStockId.HasValue || !(card.Tag is DataRow row))
                return false;

            return GetInt(row, "MagazaStokId") == selectedStockId.Value;
        }

        private void StockCard_Paint(object sender, PaintEventArgs e)
        {
            Control card = sender as Control;
            if (card == null)
                return;

            Color borderColor = IsSelectedStockCard(card as Panel) ? AppColors.Primary : AppColors.Border;
            using (Pen border = new Pen(borderColor, IsSelectedStockCard(card as Panel) ? 1.6F : 1F))
                e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
        }

        private void FitStockCards()
        {
            if (stockCardsPanel == null)
                return;

            int available = stockCardsPanel.ClientSize.Width - 30;
            int columns = Math.Max(1, available / 330);
            int width = Math.Max(292, (available / columns) - 18);

            foreach (Control control in stockCardsPanel.Controls)
                control.Width = width;
        }

        private string GetText(DataRow row, string columnName, string defaultValue)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            string value = Convert.ToString(row[columnName]);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private string NormalizeStockStatus(string status)
        {
            if (status == "Tukendi")
                return EmptyStockTitle;

            return string.IsNullOrWhiteSpace(status) ? "Yeterli" : status;
        }
        private string FormatDate(string value)
        {
            DateTime date;
            if (DateTime.TryParse(value, out date))
                return date.ToString("dd.MM.yyyy");

            return value;
        }

        private Color GetStockBadgeBack(string status)
        {
            if (status == "Kritik")
                return AppColors.WarningSoft;
            if (status == EmptyStockTitle)
                return AppColors.DangerSoft;
            return AppColors.SuccessSoft;
        }
        private Color GetStockBadgeFore(string status)
        {
            if (status == "Kritik")
                return AppColors.Warning;
            if (status == EmptyStockTitle)
                return AppColors.Danger;
            return AppColors.Success;
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

        private async void AramaTimer_Tick(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            await StoklariYukleAsync();
        }

        private async void BtnTemizle_Click(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            txtArama.Clear();
            chkStokta.Checked = false;
            chkKritik.Checked = false;
            await StoklariYukleAsync();
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
                    e.Value = EmptyStockTitle;

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
                lblSeciliStok.Text = SelectStockMessage;
                txtMinimumStok.Clear();
                HareketleriTemizle(StockHistoryIntro);
                return;
            }

            string bayi = GetCellText(row, "MusteriAdi");
            string urun = GetCellText(row, "UrunAdi");
            int bayiStok = GetCellInt(row, "BayiStok");
            int minimumStok = GetCellInt(row, "MinimumStok");
            selectedStockId = GetCellInt(row, "MagazaStokId");

            lblSeciliStok.Text = bayi + " / " + urun + " - stok: " + bayiStok;
            txtMinimumStok.Text = minimumStok.ToString();
            RefreshStockCardSelection();
            HareketleriYukle(row, bayi, urun);
        }
        private async void HareketleriYukle(DataGridViewRow row, string bayi, string urun)
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
                DataTable hareketler = await GetStokHareketleriAsync(magazaId, urunId);

                dgvHareketler.DataSource = hareketler;
                ToggleGridEmptyState(dgvHareketler, ref lblHareketEmptyState, "Seçili stok için hareket kaydı bulunamadı.");

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

            ToggleGridEmptyState(dgvHareketler, ref lblHareketEmptyState, message);

            if (lblHareketInfo != null)
                lblHareketInfo.Text = message;
        }

        private async void StokHareketiYap(string hareketTipi)
        {
            if (!AppSession.AdminMi)
                return;

            DataGridViewRow row = GetSelectedGridRow();

            if (row == null)
            {
                MessageBox.Show("Lütfen önce bir bayi stok kartı seçin.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryGetPositiveInt(txtHareketMiktar.Text, out int miktar))
            {
                MessageBox.Show("Stok miktarı sıfırdan büyük bir sayı olmalıdır.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHareketMiktar.Focus();
                return;
            }

            int magazaId = GetCellInt(row, "MagazaId");
            int urunId = GetCellInt(row, "UrunId");
            int mevcutStok = GetCellInt(row, "BayiStok");

            if (magazaId <= 0 || urunId <= 0)
            {
                MessageBox.Show("Seçili stok kartı için bayi veya ürün bilgisi eksik.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool cikisMi = hareketTipi == "ManuelCikis";

            if (cikisMi && miktar > mevcutStok)
            {
                MessageBox.Show("Bayi stoğu eksiye düşemez. Çıkış miktarını kontrol edin.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                await StokHareketiKaydetAsync(magazaId, urunId, hareketTipi, miktar, "Admin panel manuel " + islemAdi);

                txtHareketMiktar.Clear();
                await StoklariYukleAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void BtnMinimumGuncelle_Click(object sender, EventArgs e)
        {
            if (!AppSession.AdminMi)
                return;

            DataGridViewRow row = GetSelectedGridRow();

            if (row == null)
            {
                MessageBox.Show("Lütfen önce bir bayi stok kartı seçin.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryGetNonNegativeInt(txtMinimumStok.Text, out int minimumStok))
            {
                MessageBox.Show("Minimum stok negatif olamaz.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMinimumStok.Focus();
                return;
            }

            int magazaStokId = GetCellInt(row, "MagazaStokId");

            if (magazaStokId <= 0)
            {
                MessageBox.Show("Seçili stok kartı bulunamadı.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                await MinimumStokGuncelleAsync(magazaStokId, minimumStok);
                await StoklariYukleAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void BtnMerkezStokArtir_Click(object sender, EventArgs e)
        {
            if (!AppSession.AdminMi)
                return;

            DataGridViewRow row = GetSelectedGridRow();

            if (row == null)
            {
                MessageBox.Show("Lütfen önce bir stok kartı seçin.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!TryGetPositiveInt(txtHareketMiktar.Text, out int miktar))
            {
                MessageBox.Show("Merkez stok miktarı sıfırdan büyük bir sayı olmalıdır.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHareketMiktar.Focus();
                return;
            }

            int urunId = GetCellInt(row, "UrunId");
            string urun = GetCellText(row, "UrunAdi");

            if (urunId <= 0)
            {
                MessageBox.Show("Seçili stok kartı için ürün bilgisi eksik.", WarningTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                urun + " için merkez stoğa " + miktar + " adet eklenecek. Bu işlem bayi stoğunu değiştirmez. Devam edilsin mi?",
                "Merkez Stok Artırma",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                await MerkezStokArtirAsync(urunId, miktar, "Admin panel merkez stok artırma");
                txtHareketMiktar.Clear();
                await StoklariYukleAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Task<DataTable> GetStoklarAsync()
        {
            return apiClient.GetBayiStoklariAsync(
                GetCurrentMagazaId(),
                txtArama.Text.Trim(),
                chkStokta.Checked,
                chkKritik.Checked,
                true,
                AppSession.KullaniciId,
                AppSession.AdminMi);
        }

        private Task<DataRow> GetStokOzetiAsync()
        {
            return apiClient.GetBayiStokOzetiAsync(
                GetCurrentMagazaId(),
                IsTumMagazalar(),
                AppSession.KullaniciId,
                AppSession.AdminMi);
        }

        private Task<DataTable> GetStokHareketleriAsync(int magazaId, int urunId)
        {
            return apiClient.GetBayiStokHareketleriAsync(
                magazaId,
                urunId,
                25,
                AppSession.KullaniciId,
                AppSession.AdminMi);
        }

        private Task StokHareketiKaydetAsync(int magazaId, int urunId, string hareketTipi, int miktar, string aciklama)
        {
            return apiClient.ProcessBayiStokMovementAsync(magazaId, urunId, hareketTipi, miktar, aciklama, null);
        }

        private Task MerkezStokArtirAsync(int urunId, int miktar, string aciklama)
        {
            return apiClient.IncreaseCentralStockAsync(urunId, miktar, aciklama);
        }

        private Task MinimumStokGuncelleAsync(int magazaStokId, int minimumStok)
        {
            return apiClient.UpdateBayiStokMinimumAsync(magazaStokId, minimumStok);
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
                bool cikisMi = IsStockOutDirection(text);
                e.Value = cikisMi ? "- " + DirectionOut : "+ " + DirectionIn;
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                e.CellStyle.ForeColor = cikisMi ? AppColors.Danger : AppColors.Success;
                e.FormattingApplied = true;
            }
        }
        private void DgvStoklar_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dgvStoklar.Columns[e.ColumnIndex].Name != "StokDurumu")
                return;

            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            string text = NormalizeStockStatus(Convert.ToString(e.FormattedValue));
            BadgeStyleHelper.GetStatusColors(text, out Color backColor, out Color foreColor);

            Rectangle badgeRect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - 84) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                84,
                24);

            using (SolidBrush brush = new SolidBrush(backColor))
            using (SolidBrush textBrush = new SolidBrush(foreColor))
            using (StringFormat sf = new StringFormat())
            using (Font font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.FillRectangle(brush, badgeRect);
                e.Graphics.DrawString(text, font, textBrush, badgeRect, sf);
            }
        }

        private void DgvHareketler_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dgvHareketler.Columns[e.ColumnIndex].Name != "HareketYonu")
                return;

            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            string direction = Convert.ToString(e.FormattedValue);
            bool cikisMi = IsStockOutDirection(direction);
            string text = cikisMi ? "- " + DirectionOut : "+ " + DirectionIn;
            Color backColor = cikisMi ? AppColors.DangerSoft : AppColors.SuccessSoft;
            Color foreColor = cikisMi ? AppColors.Danger : AppColors.Success;

            Rectangle badgeRect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - 84) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                84,
                24);

            using (SolidBrush brush = new SolidBrush(backColor))
            using (SolidBrush textBrush = new SolidBrush(foreColor))
            using (StringFormat sf = new StringFormat())
            using (Font font = new Font("Segoe UI", 8.4F, FontStyle.Bold))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.FillRectangle(brush, badgeRect);
                e.Graphics.DrawString(text, font, textBrush, badgeRect, sf);
            }
        }

        private bool IsStockOutDirection(string direction)
        {
            if (string.IsNullOrWhiteSpace(direction))
                return false;

            return direction.IndexOf("Cikis", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   direction.IndexOf("Çıkış", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   direction.IndexOf("Cikis", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   direction.IndexOf("-", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private void UpdateFilterLayout()
        {
            if (filterPanel == null)
                return;

            lblInfo.Location = new Point(filterPanel.Width - lblInfo.Width - 16, 14);
            lblInfo.Visible = lblInfo.Left > btnTemizle.Right + 20;
        }

        private void UpdateAdminPanelLayout()
        {
            if (adminPanel == null)
                return;

            int right = adminPanel.Width - 18;
            btnMinimumGuncelle.Location = new Point(right - btnMinimumGuncelle.Width, 34);
            right = btnMinimumGuncelle.Left - 10;
            btnMerkezStokArtir.Location = new Point(right - btnMerkezStokArtir.Width, 34);
            right = btnMerkezStokArtir.Left - 10;
            btnStokCikisi.Location = new Point(right - btnStokCikisi.Width, 34);
            right = btnStokCikisi.Left - 10;
            btnStokGirisi.Location = new Point(right - btnStokGirisi.Width, 34);

            txtMinimumStok.Location = new Point(btnStokGirisi.Left - 190, 48);
            txtHareketMiktar.Location = new Point(btnStokGirisi.Left - 280, 48);
        }

        private void ToggleGridEmptyState(DataGridView grid, ref Label label, string message)
        {
            if (grid == null)
                return;

            bool hasRows = grid.Rows.Count > 0;
            if (hasRows)
            {
                if (label != null)
                    label.Visible = false;
                return;
            }

            if (label == null)
            {
                label = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = AppColors.TextSecondary,
                    BackColor = AppColors.CardBackground
                };
                grid.Controls.Add(label);
                label.BringToFront();
            }

            label.Text = message;
            label.Visible = true;
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
