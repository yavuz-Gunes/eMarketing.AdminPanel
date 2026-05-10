using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class MagazalarPage : UserControl, IThemeable
    {
        private readonly ApiDataClient apiClient = new ApiDataClient();

        private TableLayoutPanel anaYerlesim;
        private ShadowPanel listePanel;
        private ShadowPanel detayPanel;
        private FlowLayoutPanel magazaKartListesi;
        private TextBox txtArama;
        private ComboBox cmbBayi;
        private Button btnYeniMagaza;
        private Label lblOzet;
        private Label lblDetayBaslik;
        private Label lblBayi;
        private Label lblKonum;
        private Label lblTelefon;
        private Label lblSorumlu;
        private Label lblSiparis;
        private Label lblCiro;
        private Label lblSonSiparis;
        private Label lblDurum;
        private Button btnDuzenle;
        private Button btnDetay;
        private Button btnDurum;
        private Button btnAktifMagaza;

        private DataTable magazaTable;
        private DataRow seciliMagaza;
        private bool bayilerYukleniyor;

        public MagazalarPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            BuildLayout();
            Load += MagazalarPage_Load;
        }

        private void BuildLayout()
        {
            anaYerlesim = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppColors.Background
            };
            anaYerlesim.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            anaYerlesim.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

            listePanel = CreatePanel(new Padding(0, 0, 18, 0));
            detayPanel = CreatePanel(Padding.Empty);

            BuildListePanel();
            BuildDetayPanel();

            anaYerlesim.Controls.Add(listePanel, 0, 0);
            anaYerlesim.Controls.Add(detayPanel, 1, 0);
            Controls.Add(anaYerlesim);
        }

        private ShadowPanel CreatePanel(Padding margin)
        {
            return new ShadowPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                BorderColor = AppColors.Border,
                ShadowColor = Color.FromArgb(18, 15, 23, 42),
                CornerRadius = 12,
                ShadowSize = 4,
                Padding = new Padding(22),
                Margin = margin
            };
        }

        private void BuildListePanel()
        {
            Label title = CreateTitle("Bayiler ve Mağazalar");
            Label subtitle = CreateSubtitle("Bayi firmaları, mağaza/şube kartları ve mağaza performansı");

            Panel aracCubugu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = Color.Transparent
            };

            txtArama = CreateTextBox();
            txtArama.Location = new Point(0, 28);
            txtArama.Width = 260;
            txtArama.TextChanged += async (sender, e) => await MagazalariYukleAsync(false);

            cmbBayi = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary,
                Location = new Point(280, 28),
                Width = 260
            };
            cmbBayi.DisplayMember = "BayiGosterim";
            cmbBayi.ValueMember = "CustomerId";
            cmbBayi.SelectedIndexChanged += async (sender, e) =>
            {
                if (!bayilerYukleniyor)
                    await MagazalariYukleAsync(false);
            };

            btnYeniMagaza = CreateButton("Yeni Mağaza", true);
            btnYeniMagaza.Location = new Point(560, 26);
            btnYeniMagaza.Width = 124;
            btnYeniMagaza.Click += BtnYeniMagaza_Click;

            Label lblArama = CreateFieldLabel("Mağaza Ara");
            lblArama.Location = new Point(0, 6);

            Label lblBayiSec = CreateFieldLabel("Bayi");
            lblBayiSec.Location = new Point(280, 6);

            aracCubugu.Controls.Add(lblArama);
            aracCubugu.Controls.Add(txtArama);
            aracCubugu.Controls.Add(lblBayiSec);
            aracCubugu.Controls.Add(cmbBayi);
            aracCubugu.Controls.Add(btnYeniMagaza);
            aracCubugu.Resize += (sender, e) =>
            {
                int buttonLeft = Math.Max(0, aracCubugu.Width - btnYeniMagaza.Width);
                btnYeniMagaza.Location = new Point(buttonLeft, 26);
                cmbBayi.Width = Math.Max(200, buttonLeft - cmbBayi.Left - 20);
            };

            lblOzet = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Mağazalar yükleniyor",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                BackColor = Color.Transparent
            };

            magazaKartListesi = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(0, 10, 4, 4),
                BackColor = AppColors.CardBackground
            };
            magazaKartListesi.SizeChanged += (sender, e) => FitCards();

            listePanel.Controls.Add(magazaKartListesi);
            listePanel.Controls.Add(lblOzet);
            listePanel.Controls.Add(aracCubugu);
            listePanel.Controls.Add(subtitle);
            listePanel.Controls.Add(title);
        }

        private void BuildDetayPanel()
        {
            Label title = CreateTitle("Mağaza Detayı");
            Label subtitle = CreateSubtitle("Seçili bayi mağazasının operasyon özeti");

            lblDetayBaslik = CreateDetailTitle("Mağaza seçin");
            lblBayi = CreateDetailLine("Bayi", "-");
            lblKonum = CreateDetailLine("Konum", "-");
            lblTelefon = CreateDetailLine("Telefon", "-");
            lblSorumlu = CreateDetailLine("Sorumlu", "-");
            lblSiparis = CreateDetailLine("Sipariş", "-");
            lblCiro = CreateDetailLine("Ciro", "-");
            lblSonSiparis = CreateDetailLine("Son Sipariş", "-");
            lblDurum = CreateDetailLine("Durum", "-");

            FlowLayoutPanel butonlar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 106,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 12, 0, 0)
            };

            btnDuzenle = CreateButton("Düzenle", true);
            btnDetay = CreateButton("Detay", false);
            btnDurum = CreateButton("Pasifleştir", false);
            btnAktifMagaza = CreateButton("Aktif Seç", false);

            btnDuzenle.Click += BtnDuzenle_Click;
            btnDetay.Click += BtnDetay_Click;
            btnDurum.Click += BtnDurum_Click;
            btnAktifMagaza.Click += BtnAktifMagaza_Click;

            butonlar.Controls.Add(btnDuzenle);
            butonlar.Controls.Add(btnDetay);
            butonlar.Controls.Add(btnDurum);
            butonlar.Controls.Add(btnAktifMagaza);

            detayPanel.Controls.Add(butonlar);
            detayPanel.Controls.Add(lblDurum);
            detayPanel.Controls.Add(lblSonSiparis);
            detayPanel.Controls.Add(lblCiro);
            detayPanel.Controls.Add(lblSiparis);
            detayPanel.Controls.Add(lblSorumlu);
            detayPanel.Controls.Add(lblTelefon);
            detayPanel.Controls.Add(lblKonum);
            detayPanel.Controls.Add(lblBayi);
            detayPanel.Controls.Add(lblDetayBaslik);
            detayPanel.Controls.Add(subtitle);
            detayPanel.Controls.Add(title);
        }

        private async void MagazalarPage_Load(object sender, EventArgs e)
        {
            await BayileriYukleAsync();
            await MagazalariYukleAsync(true);
        }

        private async Task BayileriYukleAsync()
        {
            try
            {
                bayilerYukleniyor = true;
                DataTable table = await GetBayilerAsync();

                if (!table.Columns.Contains("BayiGosterim"))
                    table.Columns.Add("BayiGosterim", typeof(string));

                foreach (DataRow row in table.Rows)
                    row["BayiGosterim"] = GetText(row, "CompanyName", GetText(row, "FullName", "Bayi"));

                DataRow allRow = table.NewRow();
                allRow["CustomerId"] = 0;
                allRow["FullName"] = "Tüm Bayiler";
                allRow["CompanyName"] = "Tüm Bayiler";
                allRow["CustomerType"] = "Toptan";
                allRow["IsActive"] = true;
                allRow["BayiGosterim"] = "Tüm Bayiler";
                table.Rows.InsertAt(allRow, 0);

                cmbBayi.DataSource = table;
                cmbBayi.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bayiler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                bayilerYukleniyor = false;
            }
        }

        private async Task MagazalariYukleAsync(bool ilkKaydiSec)
        {
            try
            {
                magazaTable = await GetMagazalarAsync();

                magazaKartListesi.SuspendLayout();
                magazaKartListesi.Controls.Clear();

                int seciliBayiId = GetSelectedBayiId();
                int gosterilenKayit = 0;

                foreach (DataRow row in magazaTable.Rows)
                {
                    if (seciliBayiId > 0 && GetInt(row, "MusteriId") != seciliBayiId)
                        continue;

                    magazaKartListesi.Controls.Add(CreateMagazaCard(row));
                    gosterilenKayit++;
                }

                if (gosterilenKayit == 0)
                    magazaKartListesi.Controls.Add(CreateEmptyCard("Filtreye uygun bayi mağazası bulunamadı."));

                lblOzet.Text = gosterilenKayit + " mağaza listeleniyor";
                FitCards();

                if (ilkKaydiSec && gosterilenKayit > 0)
                    SelectFirstVisibleCard();
                else if (seciliMagaza != null)
                    SelectMagazaById(GetInt(seciliMagaza, "MagazaId"));
                else
                    DetayiTemizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                magazaKartListesi.ResumeLayout(true);
            }
        }

        private int GetSelectedBayiId()
        {
            if (cmbBayi == null || cmbBayi.SelectedValue == null)
                return 0;

            if (cmbBayi.SelectedValue is DataRowView)
                return 0;

            int value;
            return int.TryParse(Convert.ToString(cmbBayi.SelectedValue), out value) ? value : 0;
        }

        private void SelectFirstVisibleCard()
        {
            foreach (Control control in magazaKartListesi.Controls)
            {
                Panel card = control as Panel;
                if (card != null && card.Tag is DataRow)
                {
                    MagazaSec((DataRow)card.Tag);
                    return;
                }
            }

            DetayiTemizle();
        }

        private Panel CreateMagazaCard(DataRow row)
        {
            Panel card = CreateBaseCard(172);
            card.Tag = row;

            Label magaza = CreateCardLabel(GetText(row, "MagazaAdi", "Mağaza"), 11F, FontStyle.Bold, AppColors.TextPrimary, 28);
            Label bayi = CreateCardLabel(GetText(row, "MusteriAdi", "Bayi"), 9F, FontStyle.Regular, AppColors.TextSecondary, 22);
            Label konum = CreateCardLabel(GetKonumText(row), 8.5F, FontStyle.Regular, AppColors.TextMuted, 22);
            Label sorumlu = CreateCardLabel("Sorumlu: " + GetText(row, "SorumluKisi", "-"), 8.5F, FontStyle.Regular, AppColors.TextSecondary, 22);
            Label ozet = CreateCardLabel(GetInt(row, "SiparisSayisi") + " sipariş  |  " + GetMoney(row, "ToplamCiro"), 9F, FontStyle.Bold, AppColors.Primary, 24);
            Label durum = CreateStatusLabel(GetBool(row, "MagazaAktifMi") ? "Aktif" : "Pasif", GetBool(row, "MagazaAktifMi"));
            Panel header = CreateCardHeader(row, magaza, bayi);

            konum.Dock = DockStyle.Top;
            sorumlu.Dock = DockStyle.Top;
            ozet.Dock = DockStyle.Top;
            durum.Dock = DockStyle.Bottom;

            card.Controls.Add(durum);
            card.Controls.Add(ozet);
            card.Controls.Add(sorumlu);
            card.Controls.Add(konum);
            card.Controls.Add(header);

            AttachClick(card, () => MagazaSec(row));
            AddHover(card);

            return card;
        }

        private Panel CreateBaseCard(int height)
        {
            Panel card = new Panel
            {
                Width = 300,
                Height = height,
                Margin = new Padding(0, 0, 14, 14),
                Padding = new Padding(15, 13, 15, 13),
                BackColor = AppColors.Surface,
                Cursor = Cursors.Hand
            };
            card.Paint += Card_Paint;
            return card;
        }

        private Panel CreateCardHeader(DataRow row, Label magaza, Label bayi)
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.Transparent
            };

            Label logo = CreateLogoBadge(row);
            Panel titleArea = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 1, 0, 0),
                BackColor = Color.Transparent
            };

            logo.Dock = DockStyle.Left;
            magaza.Dock = DockStyle.Top;
            bayi.Dock = DockStyle.Top;

            titleArea.Controls.Add(bayi);
            titleArea.Controls.Add(magaza);

            header.Controls.Add(titleArea);
            header.Controls.Add(logo);

            return header;
        }

        private Label CreateLogoBadge(DataRow row)
        {
            return new Label
            {
                Text = GetInitials(GetText(row, "MusteriAdi", GetText(row, "MagazaAdi", "Bayi"))),
                Width = 44,
                Height = 44,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                BackColor = AppColors.PrimarySoft,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 2, 8, 6)
            };
        }

        private Panel CreateEmptyCard(string text)
        {
            Panel card = CreateBaseCard(84);
            Label label = CreateCardLabel(text, 9F, FontStyle.Regular, AppColors.TextSecondary, 56);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(label);
            return card;
        }

        private void MagazaSec(DataRow row)
        {
            seciliMagaza = row;
            lblDetayBaslik.Text = GetText(row, "MagazaAdi", "Mağaza");
            lblBayi.Text = "Bayi: " + GetText(row, "MusteriAdi", "-");
            lblKonum.Text = "Konum: " + GetKonumText(row);
            lblTelefon.Text = "Telefon: " + GetText(row, "Telefon", "-");
            lblSorumlu.Text = "Sorumlu: " + GetText(row, "SorumluKisi", "-");
            lblSiparis.Text = "Sipariş: " + GetInt(row, "SiparisSayisi") + " adet";
            lblCiro.Text = "Ciro: " + GetMoney(row, "ToplamCiro");
            lblSonSiparis.Text = "Son Sipariş: " + GetDate(row, "SonSiparisTarihi");

            bool aktif = GetBool(row, "MagazaAktifMi") && GetBool(row, "MusteriAktifMi");
            lblDurum.Text = "Durum: " + (aktif ? "Aktif" : "Pasif");
            btnDurum.Text = aktif ? "Pasifleştir" : "Aktifleştir";
            btnDuzenle.Enabled = true;
            btnDetay.Enabled = true;
            btnDurum.Enabled = true;
            btnAktifMagaza.Enabled = aktif;

            VurgulaSeciliKart();
        }

        private void DetayiTemizle()
        {
            seciliMagaza = null;
            lblDetayBaslik.Text = "Mağaza seçin";
            lblBayi.Text = "Bayi: -";
            lblKonum.Text = "Konum: -";
            lblTelefon.Text = "Telefon: -";
            lblSorumlu.Text = "Sorumlu: -";
            lblSiparis.Text = "Sipariş: -";
            lblCiro.Text = "Ciro: -";
            lblSonSiparis.Text = "Son Sipariş: -";
            lblDurum.Text = "Durum: -";
            btnDuzenle.Enabled = false;
            btnDetay.Enabled = false;
            btnDurum.Enabled = false;
            btnAktifMagaza.Enabled = false;
        }

        private void SelectMagazaById(int magazaId)
        {
            if (magazaTable == null)
            {
                DetayiTemizle();
                return;
            }

            foreach (DataRow row in magazaTable.Rows)
            {
                if (GetInt(row, "MagazaId") == magazaId)
                {
                    MagazaSec(row);
                    return;
                }
            }

            DetayiTemizle();
        }

        private async void BtnYeniMagaza_Click(object sender, EventArgs e)
        {
            if (cmbBayi.SelectedValue == null)
            {
                MessageBox.Show("Yeni mağaza eklemek için önce bayi seçin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int musteriId = Convert.ToInt32(cmbBayi.SelectedValue);

            using (CustomerStoreModalForm form = new CustomerStoreModalForm(musteriId))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.IsSaved)
                    await MagazalariYukleAsync(false);
            }
        }

        private async void BtnDuzenle_Click(object sender, EventArgs e)
        {
            if (seciliMagaza == null)
                return;

            using (CustomerStoreModalForm form = new CustomerStoreModalForm(
                GetInt(seciliMagaza, "MusteriId"),
                GetInt(seciliMagaza, "MagazaId")))
            {
                if (form.ShowDialog(this) == DialogResult.OK && form.IsSaved)
                    await MagazalariYukleAsync(false);
            }
        }

        private void BtnDetay_Click(object sender, EventArgs e)
        {
            if (seciliMagaza == null)
                return;

            using (BayiDetayForm form = new BayiDetayForm(seciliMagaza))
            {
                form.ShowDialog(this);
            }
        }

        private async void BtnDurum_Click(object sender, EventArgs e)
        {
            if (seciliMagaza == null)
                return;

            try
            {
                bool aktif = GetBool(seciliMagaza, "MagazaAktifMi");
                await MagazaDurumGuncelleAsync(GetInt(seciliMagaza, "MagazaId"), !aktif);
                await MagazalariYukleAsync(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mağaza durumu güncellenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAktifMagaza_Click(object sender, EventArgs e)
        {
            if (seciliMagaza == null)
                return;

            AppSession.MagazaSec(
                GetInt(seciliMagaza, "MusteriId"),
                GetInt(seciliMagaza, "MagazaId"),
                GetText(seciliMagaza, "MusteriAdi", ""),
                GetText(seciliMagaza, "MagazaAdi", ""),
                GetText(seciliMagaza, "Sehir", ""));

            FrmMain main = FindForm() as FrmMain;
            if (main != null)
                main.MagazaSeciminiYenile();

            MessageBox.Show("Aktif mağaza seçimi güncellendi.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Task<DataTable> GetBayilerAsync()
        {
            return apiClient.GetBayilerAsync("", 1);
        }

        private Task<DataTable> GetMagazalarAsync()
        {
            return apiClient.GetMagazaSecimListesiAsync(
                txtArama == null ? "" : txtArama.Text.Trim(),
                false,
                AppSession.KullaniciId,
                AppSession.AdminMi);
        }

        private Task MagazaDurumGuncelleAsync(int magazaId, bool aktifMi)
        {
            return apiClient.SetBayiMagazaStatusAsync(magazaId, aktifMi);
        }

        private void VurgulaSeciliKart()
        {
            foreach (Control control in magazaKartListesi.Controls)
            {
                Panel card = control as Panel;
                if (card == null || !(card.Tag is DataRow))
                    continue;

                DataRow row = (DataRow)card.Tag;
                bool secili = seciliMagaza != null &&
                    GetInt(row, "MagazaId") == GetInt(seciliMagaza, "MagazaId");

                card.BackColor = secili ? AppColors.PrimarySoft : AppColors.Surface;
            }
        }

        private void AttachClick(Control control, Action action)
        {
            control.Cursor = Cursors.Hand;
            control.Click += (sender, e) => action();

            foreach (Control child in control.Controls)
            {
                AttachClick(child, action);
            }
        }

        private void AddHover(Panel card)
        {
            card.MouseEnter += (sender, e) =>
            {
                if (!IsSeciliMagazaCard(card))
                    card.BackColor = AppColors.PrimarySoft;
            };

            card.MouseLeave += (sender, e) =>
            {
                if (!IsSeciliMagazaCard(card))
                    card.BackColor = AppColors.Surface;
            };
        }

        private bool IsSeciliMagazaCard(Panel card)
        {
            if (seciliMagaza == null || !(card.Tag is DataRow))
                return false;

            DataRow row = (DataRow)card.Tag;
            return GetInt(row, "MagazaId") == GetInt(seciliMagaza, "MagazaId");
        }

        private void FitCards()
        {
            if (magazaKartListesi == null)
                return;

            int width = Math.Max(270, (magazaKartListesi.ClientSize.Width - 52) / 2);

            foreach (Control control in magazaKartListesi.Controls)
                control.Width = width;
        }

        private Label CreateTitle(string text)
        {
            return new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = text,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };
        }

        private Label CreateSubtitle(string text)
        {
            return new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Text = text,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };
        }

        private Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                Width = 220,
                Height = 20,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };
        }

        private Label CreateDetailTitle(string text)
        {
            return new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                Text = text,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
        }

        private Label CreateDetailLine(string label, string value)
        {
            return new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = label + ": " + value,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
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

        private Label CreateStatusLabel(string text, bool aktif)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Height = 24,
                Width = 80,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = aktif ? AppColors.Success : AppColors.Danger,
                BackColor = aktif ? AppColors.SuccessSoft : AppColors.DangerSoft,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
        }

        private Button CreateButton(string text, bool primary)
        {
            Button button = new Button
            {
                Text = text,
                Width = 112,
                Height = 34,
                Margin = new Padding(0, 0, 10, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = primary ? AppColors.Primary : AppColors.PrimarySoft,
                ForeColor = primary ? Color.White : AppColors.Primary
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
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

        private string GetMoney(DataRow row, string columnName)
        {
            decimal value = GetDecimal(row, columnName);
            return value.ToString("N2", new CultureInfo("tr-TR")) + " TL";
        }

        private string GetDate(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return "-";

            return Convert.ToDateTime(row[columnName]).ToString("dd.MM.yyyy HH:mm", new CultureInfo("tr-TR"));
        }

        private string GetInitials(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "B";

            string[] parts = text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder();

            foreach (string part in parts)
            {
                builder.Append(char.ToUpper(part[0], new CultureInfo("tr-TR")));

                if (builder.Length == 2)
                    break;
            }

            return builder.Length == 0 ? "B" : builder.ToString();
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

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Control card = sender as Control;
            if (card == null)
                return;

            using (Pen pen = new Pen(AppColors.Border))
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;

            if (anaYerlesim != null)
                anaYerlesim.BackColor = AppColors.Background;

            if (listePanel != null)
                listePanel.BackColor = AppColors.CardBackground;

            if (detayPanel != null)
                detayPanel.BackColor = AppColors.CardBackground;

            if (magazaKartListesi != null)
                magazaKartListesi.BackColor = AppColors.CardBackground;
        }
    }
}
