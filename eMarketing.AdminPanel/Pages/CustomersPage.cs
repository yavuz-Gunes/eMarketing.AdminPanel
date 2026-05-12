using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class CustomersPage : UserControl, IThemeable
    {
        private readonly ApiDataClient apiClient = new ApiDataClient();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private Panel gridPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblInfo;
        private TextBox txtArama;
        private ComboBox cmbDurum;
        private Button btnYeni;
        private Button btnAra;
        private Button btnTemizle;
        private DataGridView dgvYetkililer;
        private FlowLayoutPanel yetkiliKartListesi;
        private CategoriesCard cToplam;
        private CategoriesCard cAktif;
        private CategoriesCard cPasif;
        private CategoriesCard cSiparis;
        private Timer aramaTimer;
        private DataTable yetkiliTable;
        private int hoveredRow = -1;
        private int hoveredCol = -1;

        public CustomersPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            aramaTimer = new Timer { Interval = 350 };
            aramaTimer.Tick += AramaTimer_Tick;

            BuildLayout();
            Load += CustomersPage_Load;
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
                Height = 82,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = "Sipariş Yetkilileri",
                Location = new Point(0, 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            lblSubtitle = new Label
            {
                Text = "Aktif mağazadaki sipariş verebilen personelleri yönetin.",
                Location = new Point(2, 38),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            btnYeni = new Button
            {
                Text = "+ Yeni Sipariş Yetkilisi",
                Width = 150,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnYeni.FlatAppearance.BorderSize = 0;
            btnYeni.Click += BtnYeni_Click;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(btnYeni);
            headerPanel.Resize += (sender, e) =>
            {
                btnYeni.Location = new Point(headerPanel.Width - btnYeni.Width, 6);
            };
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

            cToplam = CreateStatCard("Toplam", "0", new Padding(0, 0, 16, 0));
            cAktif = CreateStatCard("Aktif", "0", new Padding(0, 0, 16, 0));
            cPasif = CreateStatCard("Pasif", "0", new Padding(0, 0, 16, 0));
            cSiparis = CreateStatCard("Sipariş Yetkilisi", "0", Padding.Empty);

            grid.Controls.Add(cToplam, 0, 0);
            grid.Controls.Add(cAktif, 1, 0);
            grid.Controls.Add(cPasif, 2, 0);
            grid.Controls.Add(cSiparis, 3, 0);
            statsPanel.Controls.Add(grid);
        }

        private CategoriesCard CreateStatCard(string title, string value, Padding margin)
        {
            CategoriesCard card = new CategoriesCard { Dock = DockStyle.Fill, Margin = margin };
            card.SetData("□", title, value);
            return card;
        }

        private void BuildFilters()
        {
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(16, 14, 16, 14)
            };

            txtArama = new TextBox
            {
                Width = 320,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };

            cmbDurum = new ComboBox
            {
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
            cmbDurum.Items.Add("Hepsi");
            cmbDurum.Items.Add("Aktif");
            cmbDurum.Items.Add("Pasif");
            cmbDurum.SelectedIndex = 1;

            btnAra = CreateButton("Ara", true);
            btnTemizle = CreateButton("Temizle", false);

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

            txtArama.TextChanged += TxtArama_TextChanged;
            txtArama.KeyDown += TxtArama_KeyDown;
            cmbDurum.SelectedIndexChanged += async (sender, e) => await YetkilileriYukleAsync();
            btnAra.Click += async (sender, e) => await YetkilileriYukleAsync();
            btnTemizle.Click += BtnTemizle_Click;

            filterPanel.Controls.Add(txtArama);
            filterPanel.Controls.Add(cmbDurum);
            filterPanel.Controls.Add(btnAra);
            filterPanel.Controls.Add(btnTemizle);
            filterPanel.Controls.Add(lblInfo);
            filterPanel.Resize += (sender, e) => PlaceFilterControls();
            PlaceFilterControls();
        }

        private Button CreateButton(string text, bool primary)
        {
            Button button = new Button
            {
                Text = text,
                Width = primary ? 88 : 92,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? AppColors.Primary : AppColors.CardBackground,
                ForeColor = primary ? Color.White : AppColors.TextSecondary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = AppColors.Border;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            return button;
        }

        private void PlaceFilterControls()
        {
            int x = 16;
            int y = 20;

            txtArama.Location = new Point(x, y);
            x += txtArama.Width + 14;

            cmbDurum.Location = new Point(x, y);
            x += cmbDurum.Width + 14;

            btnAra.Location = new Point(x, y - 2);
            x += btnAra.Width + 10;

            btnTemizle.Location = new Point(x, y - 2);

            lblInfo.Location = new Point(filterPanel.Width - lblInfo.Width - 16, y);
            lblInfo.Visible = lblInfo.Left > btnTemizle.Right + 20;
        }

        private void BuildGrid()
        {
            gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(12)
            };

            yetkiliKartListesi = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(6),
                BackColor = AppColors.CardBackground
            };
            yetkiliKartListesi.SizeChanged += (sender, e) => FitYetkiliCards();

            dgvYetkililer = new DataGridView
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

            dgvYetkililer.EnableHeadersVisualStyles = false;
            dgvYetkililer.ColumnHeadersHeight = 42;
            dgvYetkililer.RowTemplate.Height = 50;
            dgvYetkililer.GridColor = AppColors.Border;
            dgvYetkililer.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvYetkililer.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvYetkililer.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvYetkililer.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvYetkililer.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dgvYetkililer.ColumnHeadersDefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            dgvYetkililer.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvYetkililer.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvYetkililer.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            dgvYetkililer.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            ConfigureColumns();
            dgvYetkililer.CellDoubleClick += DgvYetkililer_CellDoubleClick;
            dgvYetkililer.CellContentClick += DgvYetkililer_CellContentClick;
            dgvYetkililer.CellPainting += DgvYetkililer_CellPainting;
            dgvYetkililer.CellMouseMove += DgvYetkililer_CellMouseMove;
            dgvYetkililer.MouseLeave += DgvYetkililer_MouseLeave;
            dgvYetkililer.CellFormatting += DgvYetkililer_CellFormatting;

            dgvYetkililer.Visible = false;
            gridPanel.Controls.Add(yetkiliKartListesi);
        }

        private void ConfigureColumns()
        {
            dgvYetkililer.Columns.Clear();
            AddColumn("BayiYetkiliId", "Id", 50, false);
            AddColumn("AdSoyad", "Ad Soyad", 170, true);
            AddColumn("BayiAdi", "Bayi", 180, true);
            AddColumn("MagazaAdi", "Mağaza", 170, true);
            AddColumn("Telefon", "Telefon", 120, true);
            AddColumn("Email", "E-Posta", 170, true);
            AddColumn("Gorev", "Görev", 120, true);
            AddColumn("ProfilDoluluk", "Profil", 80, true);
            AddColumn("SiparisSayisi", "Sipariş", 75, true);
            AddColumn("SonSiparisTarihi", "Son İşlem", 120, true);
            AddColumn("AktifMi", "Durum", 90, true);

            dgvYetkililer.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colEdit",
                HeaderText = "",
                Text = "Düzenle",
                UseColumnTextForButtonValue = true,
                Width = 92
            });

            dgvYetkililer.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colStatus",
                HeaderText = "",
                Text = "Durum",
                UseColumnTextForButtonValue = true,
                Width = 98
            });

            ApplyGridColumnSizing();
        }

        private void ApplyGridColumnSizing()
        {
            SetFill("AdSoyad", 15, 95);
            SetFill("BayiAdi", 16, 100);
            SetFill("MagazaAdi", 15, 95);
            SetFill("Telefon", 10, 78);
            SetFill("Email", 15, 95);
            SetFill("Gorev", 11, 75);
            SetFill("ProfilDoluluk", 7, 58);
            SetFill("SiparisSayisi", 7, 58);
            SetFill("SonSiparisTarihi", 10, 78);
            SetFill("AktifMi", 8, 62);
            SetFill("colEdit", 8, 66);
            SetFill("colStatus", 8, 68);
        }

        private void SetFill(string columnName, float fillWeight, int minWidth)
        {
            if (!dgvYetkililer.Columns.Contains(columnName))
                return;

            DataGridViewColumn column = dgvYetkililer.Columns[columnName];
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.FillWeight = fillWeight;
            column.MinimumWidth = minWidth;
        }

        private void AddColumn(string name, string header, int width, bool visible)
        {
            dgvYetkililer.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = header,
                Width = width,
                Visible = visible
            });
        }

        private async void CustomersPage_Load(object sender, EventArgs e)
        {
            await YetkilileriYukleAsync();
        }

        private async Task YetkilileriYukleAsync()
        {
            try
            {
                yetkiliTable = await GetYetkililerAsync();
                ProfilDolulukHazirla(yetkiliTable);
                dgvYetkililer.DataSource = yetkiliTable;
                FillYetkiliCards();
                lblInfo.Text = yetkiliTable.Rows.Count + " kayıt";
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FillYetkiliCards()
        {
            if (yetkiliKartListesi == null)
                return;

            yetkiliKartListesi.SuspendLayout();
            yetkiliKartListesi.Controls.Clear();

            if (yetkiliTable == null || yetkiliTable.Rows.Count == 0)
            {
                Panel empty = CreateYetkiliBaseCard();
                Label label = CreateCardLabel("Sipariş yetkilisi bulunamadı.", 9F, FontStyle.Regular, AppColors.TextSecondary, 42);
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleCenter;
                empty.Controls.Add(label);
                yetkiliKartListesi.Controls.Add(empty);
            }
            else
            {
                foreach (DataRow row in yetkiliTable.Rows)
                    yetkiliKartListesi.Controls.Add(CreateYetkiliCard(row));
            }

            FitYetkiliCards();
            yetkiliKartListesi.ResumeLayout(true);
        }

        private Panel CreateYetkiliCard(DataRow row)
        {
            Panel card = CreateYetkiliBaseCard();
            int id = GetRowInt(row, "BayiYetkiliId");
            bool aktif = GetRowBool(row, "AktifMi");
            string yetkiTipi = GetRowText(row, "YetkiTipi", "");
            bool siparisYetkilisi = string.Equals(yetkiTipi, "SiparisYetkilisi", StringComparison.OrdinalIgnoreCase);
            if (siparisYetkilisi && aktif)
                card.BackColor = AppColors.SuccessSoft;

            Label avatar = new Label
            {
                Text = GetInitials(GetRowText(row, "AdSoyad", "YK")),
                Location = new Point(16, 18),
                Size = new Size(58, 58),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                BackColor = AppColors.PrimarySoft
            };

            Label ad = CreateCardLabel(GetRowText(row, "AdSoyad", "Yetkili"), 11F, FontStyle.Bold, AppColors.TextPrimary, 24);
            ad.Location = new Point(88, 16);
            ad.Width = 260;

            Label yetki = CreateCardLabel(GetRowText(row, "YetkiTipiGorunenAd", GetRowText(row, "Gorev", "")), 8.5F, FontStyle.Regular, AppColors.TextSecondary, 20);
            yetki.Location = new Point(88, 42);
            yetki.Width = 260;

            Label bayi = CreateCardLabel(GetRowText(row, "BayiAdi", "-") + " / " + GetRowText(row, "MagazaAdi", "-"), 8.5F, FontStyle.Regular, AppColors.TextMuted, 20);
            bayi.Location = new Point(88, 64);
            bayi.Width = 360;

            Label telefon = CreateCardLabel(GetRowText(row, "Telefon", "-"), 8.5F, FontStyle.Regular, AppColors.TextSecondary, 20);
            telefon.Location = new Point(18, 94);
            telefon.Width = 170;

            Label email = CreateCardLabel(GetRowText(row, "Email", "-"), 8.5F, FontStyle.Regular, AppColors.TextSecondary, 20);
            email.Location = new Point(196, 94);
            email.Width = 230;

            Label durum = CreateCardLabel(aktif ? "Aktif" : "Pasif", 8.5F, FontStyle.Bold, aktif ? AppColors.Success : AppColors.Danger, 26);
            durum.Location = new Point(18, 128);
            durum.Width = 86;
            durum.TextAlign = ContentAlignment.MiddleCenter;
            durum.BackColor = aktif ? AppColors.SuccessSoft : AppColors.DangerSoft;

            Label siparis = CreateCardLabel(GetRowInt(row, "SiparisSayisi") + " sipariş", 8.5F, FontStyle.Bold, AppColors.Primary, 26);
            siparis.Location = new Point(112, 128);
            siparis.Width = 100;
            siparis.TextAlign = ContentAlignment.MiddleCenter;
            siparis.BackColor = AppColors.PrimarySoft;

            Label yetkiRozeti = CreateCardLabel("Sipariş Yetkilisi", 8F, FontStyle.Bold, AppColors.Success, 24);
            yetkiRozeti.Location = new Point(18, 162);
            yetkiRozeti.Width = 150;
            yetkiRozeti.TextAlign = ContentAlignment.MiddleCenter;
            yetkiRozeti.BackColor = siparisYetkilisi ? Color.White : AppColors.PrimarySoft;

            Button edit = CreateButton("Düzenle", true);
            edit.Size = new Size(92, 30);
            edit.Location = new Point(card.Width - 208, 160);
            edit.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            edit.Click += (sender, e) => OpenEditById(id);

            Button status = CreateButton(aktif ? "Pasife Al" : "Aktifleştir", false);
            status.Size = new Size(96, 30);
            status.Location = new Point(card.Width - 108, 160);
            status.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            status.ForeColor = aktif ? AppColors.Danger : AppColors.Success;
            status.Click += async (sender, e) => await ToggleStatusAsync(id, aktif);

            card.Controls.Add(status);
            card.Controls.Add(edit);
            card.Controls.Add(yetkiRozeti);
            card.Controls.Add(siparis);
            card.Controls.Add(durum);
            card.Controls.Add(email);
            card.Controls.Add(telefon);
            card.Controls.Add(bayi);
            card.Controls.Add(yetki);
            card.Controls.Add(ad);
            card.Controls.Add(avatar);
            return card;
        }

        private Panel CreateYetkiliBaseCard()
        {
            Panel card = new Panel
            {
                Width = 460,
                Height = 206,
                Margin = new Padding(8),
                Padding = new Padding(14),
                BackColor = AppColors.Surface
            };
            card.Paint += (sender, e) =>
            {
                using (Pen pen = new Pen(AppColors.Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            return card;
        }

        private void FitYetkiliCards()
        {
            if (yetkiliKartListesi == null)
                return;

            int columns = yetkiliKartListesi.ClientSize.Width >= 980 ? 2 : 1;
            int width = Math.Max(420, (yetkiliKartListesi.ClientSize.Width - 38 - (columns * 16)) / columns);
            foreach (Control control in yetkiliKartListesi.Controls)
                control.Width = width;
        }

        private async void OpenEditById(int id)
        {
            if (id <= 0)
                return;

            using (BayiYetkiliModalForm form = new BayiYetkiliModalForm(id, AppSession.SeciliMagazaId, AppSession.SeciliMusteriId))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.IsSaved)
                    await YetkilileriYukleAsync();
            }
        }

        private async Task ToggleStatusAsync(int id, bool aktif)
        {
            DialogResult result = MessageBox.Show(
                aktif ? "Bu yetkiliyi pasife almak istiyor musunuz?" : "Bu yetkiliyi tekrar aktifleştirmek istiyor musunuz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            await DurumGuncelleAsync(id, !aktif);
            await YetkilileriYukleAsync();
        }

        private Label CreateCardLabel(string text, float size, FontStyle style, Color color, int height)
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

        private string GetInitials(string name)
        {
            string[] parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return "YK";
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private string GetRowText(DataRow row, string columnName, string defaultValue)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;
            string value = Convert.ToString(row[columnName]);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private int GetRowInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;
            return Convert.ToInt32(row[columnName]);
        }

        private bool GetRowBool(DataRow row, string columnName)
        {
            return row != null && row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value && Convert.ToBoolean(row[columnName]);
        }

        private int GetDurum()
        {
            if (cmbDurum.SelectedIndex == 1)
                return 1;

            if (cmbDurum.SelectedIndex == 2)
                return 0;

            return -1;
        }

        private void UpdateStats()
        {
            int total = 0;
            int active = 0;
            int passive = 0;
            int ordered = 0;

            if (yetkiliTable != null)
            {
                total = yetkiliTable.Rows.Count;
                foreach (DataRow row in yetkiliTable.Rows)
                {
                    bool aktif = row["AktifMi"] != DBNull.Value && Convert.ToBoolean(row["AktifMi"]);
                    int siparis = row["SiparisSayisi"] == DBNull.Value ? 0 : Convert.ToInt32(row["SiparisSayisi"]);

                    if (aktif)
                        active++;
                    else
                        passive++;

                    if (siparis > 0)
                        ordered++;
                }
            }

            cToplam.SetData("□", "Toplam", total.ToString());
            cAktif.SetData("□", "Aktif", active.ToString());
            cPasif.SetData("□", "Pasif", passive.ToString());
            cSiparis.SetData("□", "Sipariş Yetkilisi", active.ToString());
        }

        private void ProfilDolulukHazirla(DataTable table)
        {
            if (table == null)
                return;

            if (!table.Columns.Contains("ProfilDoluluk"))
                table.Columns.Add("ProfilDoluluk", typeof(int));

            foreach (DataRow row in table.Rows)
            {
                int doluAlan = 0;
                int toplamAlan = 6;

                if (HasValue(row, "AdSoyad"))
                    doluAlan++;

                if (HasValue(row, "Telefon"))
                    doluAlan++;

                if (HasValue(row, "Email"))
                    doluAlan++;

                if (HasValue(row, "BayiAdi"))
                    doluAlan++;

                if (HasValue(row, "MagazaAdi"))
                    doluAlan++;

                if (HasValue(row, "Gorev"))
                    doluAlan++;

                row["ProfilDoluluk"] = (int)Math.Round((doluAlan * 100D) / toplamAlan);
            }
        }

        private bool HasValue(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName)
                && row[columnName] != DBNull.Value
                && !string.IsNullOrWhiteSpace(Convert.ToString(row[columnName]));
        }

        private async void BtnYeni_Click(object sender, EventArgs e)
        {
            using (BayiYetkiliModalForm form = new BayiYetkiliModalForm(0, AppSession.SeciliMagazaId, AppSession.SeciliMusteriId))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.IsSaved)
                    await YetkilileriYukleAsync();
            }
        }

        private void DgvYetkililer_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            OpenEdit(e.RowIndex);
        }

        private async void DgvYetkililer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvYetkililer.Columns[e.ColumnIndex].Name;

            if (columnName == "colEdit")
                OpenEdit(e.RowIndex);
            else if (columnName == "colStatus")
                await ToggleStatusAsync(e.RowIndex);
        }

        private async void OpenEdit(int rowIndex)
        {
            int id = GetId(rowIndex);
            if (id <= 0)
                return;

            using (BayiYetkiliModalForm form = new BayiYetkiliModalForm(id, AppSession.SeciliMagazaId, AppSession.SeciliMusteriId))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.IsSaved)
                    await YetkilileriYukleAsync();
            }
        }

        private async Task ToggleStatusAsync(int rowIndex)
        {
            int id = GetId(rowIndex);
            bool aktif = GetActive(rowIndex);

            DialogResult result = MessageBox.Show(
                aktif ? "Bu yetkiliyi pasife almak istiyor musunuz?" : "Bu yetkiliyi tekrar aktifleştirmek istiyor musunuz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            await DurumGuncelleAsync(id, !aktif);
            await YetkilileriYukleAsync();
        }

        private Task<DataTable> GetYetkililerAsync()
        {
            int? bayiId = AppSession.TumMagazalar ? null : AppSession.SeciliMusteriId;
            int? magazaId = AppSession.TumMagazalar ? null : AppSession.SeciliMagazaId;
            return apiClient.GetBayiYetkilileriAsync(txtArama.Text.Trim(), GetDurum(), bayiId, magazaId);
        }

        private Task DurumGuncelleAsync(int id, bool aktifMi)
        {
            return apiClient.SetBayiYetkiliStatusAsync(id, aktifMi);
        }

        private int GetId(int rowIndex)
        {
            object value = dgvYetkililer.Rows[rowIndex].Cells["BayiYetkiliId"].Value;
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private bool GetActive(int rowIndex)
        {
            object value = dgvYetkililer.Rows[rowIndex].Cells["AktifMi"].Value;
            return value != null && value != DBNull.Value && Convert.ToBoolean(value);
        }

        private void DgvYetkililer_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvYetkililer.Columns[e.ColumnIndex].Name;

            if (columnName == "SonSiparisTarihi" && e.Value != null && e.Value != DBNull.Value)
            {
                e.Value = Convert.ToDateTime(e.Value).ToString("dd.MM.yyyy", new CultureInfo("tr-TR"));
                e.FormattingApplied = true;
            }

            if (columnName == "ProfilDoluluk" && e.Value != null && e.Value != DBNull.Value)
            {
                int oran = Convert.ToInt32(e.Value);
                e.Value = "%" + oran;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

                if (oran >= 80)
                    e.CellStyle.ForeColor = AppColors.Success;
                else if (oran >= 50)
                    e.CellStyle.ForeColor = AppColors.Warning;
                else
                    e.CellStyle.ForeColor = AppColors.Danger;

                e.FormattingApplied = true;
            }

            if ((columnName == "Email" || columnName == "BayiAdi" || columnName == "MagazaAdi") && e.Value != null)
            {
                string text = Convert.ToString(e.Value);
                if (text.Length > 24)
                {
                    e.Value = text.Substring(0, 24) + "...";
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvYetkililer_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvYetkililer.Columns[e.ColumnIndex].Name;

            if (columnName == "AktifMi")
            {
                PaintStatusBadge(e);
                return;
            }

            if (columnName == "colEdit" || columnName == "colStatus")
            {
                PaintActionButton(e, columnName);
                return;
            }
        }

        private void PaintStatusBadge(DataGridViewCellPaintingEventArgs e)
        {
            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            bool aktif = e.Value != null && e.Value != DBNull.Value && Convert.ToBoolean(e.Value);
            string text = aktif ? "Aktif" : "Pasif";
            Color backColor = aktif ? AppColors.SuccessSoft : AppColors.DangerSoft;
            Color foreColor = aktif ? AppColors.Success : AppColors.Danger;

            Rectangle rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 12, e.CellBounds.Width - 16, 24);
            using (SolidBrush back = new SolidBrush(backColor))
            using (SolidBrush fore = new SolidBrush(foreColor))
            using (StringFormat sf = new StringFormat())
            using (Font font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.FillRectangle(back, rect);
                e.Graphics.DrawString(text, font, fore, rect, sf);
            }
        }

        private void PaintActionButton(DataGridViewCellPaintingEventArgs e, string columnName)
        {
            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            bool isHovered = e.RowIndex == hoveredRow && e.ColumnIndex == hoveredCol;
            bool active = GetActive(e.RowIndex);
            bool edit = columnName == "colEdit";
            string text = edit ? "Düzenle" : (active ? "Pasife Al" : "Aktifleştir");
            Color baseColor = edit ? AppColors.Primary : (active ? AppColors.Danger : AppColors.Success);
            Color fillColor = isHovered ? baseColor : Color.White;
            Color textColor = isHovered ? Color.White : baseColor;

            Rectangle rect = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y + 7, e.CellBounds.Width - 12, e.CellBounds.Height - 14);
            using (SolidBrush fill = new SolidBrush(fillColor))
            using (Pen border = new Pen(baseColor))
            using (SolidBrush fore = new SolidBrush(textColor))
            using (StringFormat sf = new StringFormat())
            using (Font font = new Font("Segoe UI", 8.2F, FontStyle.Bold))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.FillRectangle(fill, rect);
                e.Graphics.DrawRectangle(border, rect);
                e.Graphics.DrawString(text, font, fore, rect, sf);
            }
        }

        private void DgvYetkililer_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            int newRow = -1;
            int newCol = -1;

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string name = dgvYetkililer.Columns[e.ColumnIndex].Name;
                if (name == "colEdit" || name == "colStatus")
                {
                    newRow = e.RowIndex;
                    newCol = e.ColumnIndex;
                }
            }

            if (newRow == hoveredRow && newCol == hoveredCol)
                return;

            int oldRow = hoveredRow;
            int oldCol = hoveredCol;
            hoveredRow = newRow;
            hoveredCol = newCol;

            if (oldRow >= 0 && oldCol >= 0)
                dgvYetkililer.InvalidateCell(oldCol, oldRow);

            if (hoveredRow >= 0 && hoveredCol >= 0)
                dgvYetkililer.InvalidateCell(hoveredCol, hoveredRow);
        }

        private void DgvYetkililer_MouseLeave(object sender, EventArgs e)
        {
            int oldRow = hoveredRow;
            int oldCol = hoveredCol;
            hoveredRow = -1;
            hoveredCol = -1;

            if (oldRow >= 0 && oldCol >= 0)
                dgvYetkililer.InvalidateCell(oldCol, oldRow);
        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            aramaTimer.Start();
        }

        private async void TxtArama_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                aramaTimer.Stop();
                await YetkilileriYukleAsync();
                e.SuppressKeyPress = true;
            }
        }

        private async void AramaTimer_Tick(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            await YetkilileriYukleAsync();
        }

        private async void BtnTemizle_Click(object sender, EventArgs e)
        {
            aramaTimer.Stop();
            txtArama.Clear();
            cmbDurum.SelectedIndex = 1;
            await YetkilileriYukleAsync();
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;
            if (headerPanel != null) headerPanel.BackColor = AppColors.Background;
            if (statsPanel != null) statsPanel.BackColor = AppColors.Background;
            if (filterPanel != null) filterPanel.BackColor = AppColors.CardBackground;
            if (gridPanel != null) gridPanel.BackColor = AppColors.CardBackground;
            if (lblTitle != null) lblTitle.ForeColor = AppColors.TextPrimary;
            if (lblSubtitle != null) lblSubtitle.ForeColor = AppColors.TextSecondary;
            if (lblInfo != null) lblInfo.ForeColor = AppColors.TextSecondary;
        }
    }
}
