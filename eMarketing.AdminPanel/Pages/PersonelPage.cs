using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class PersonelPage : UserControl, IThemeable
    {
        private readonly ApiDataClient apiClient = new ApiDataClient();

        private TableLayoutPanel anaYerlesim;
        private ShadowPanel personelPanel;
        private ShadowPanel yetkiPanel;

        private FlowLayoutPanel personelKartListesi;
        private FlowLayoutPanel yetkiliMagazaKartListesi;
        private FlowLayoutPanel atanabilirMagazaKartListesi;

        private TextBox txtArama;
        private TextBox txtKullaniciAdi;
        private TextBox txtSifre;
        private TextBox txtAdSoyad;
        private ComboBox cmbGorunum;
        private ComboBox cmbRol;
        private CheckBox chkAktif;
        private Button btnYeni;
        private Button btnKaydet;
        private Label lblSeciliPersonel;
        private Label lblAktifMagazaBaglami;
        private Label lblYetkiliMagazaBaslik;
        private Label lblAtanabilirMagazaBaslik;

        private int? seciliKullaniciId;
        private bool kullaniciAdiOtomatik = true;
        private bool kullaniciAdiKodlaDegisiyor;

        private bool YonetimModu
        {
            get { return AppSession.AdminMi; }
        }

        public PersonelPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            BuildLayout();
            Load += PersonelPage_Load;
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
            anaYerlesim.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            anaYerlesim.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));

            personelPanel = CreatePanel(new Padding(0, 0, 18, 0));
            yetkiPanel = CreatePanel(Padding.Empty);

            BuildPersonelPanel();
            BuildYetkiPanel();

            anaYerlesim.Controls.Add(personelPanel, 0, 0);
            anaYerlesim.Controls.Add(yetkiPanel, 1, 0);
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

        private void BuildPersonelPanel()
        {
            Label title = CreateTitle(YonetimModu ? "Personel" : "Bayi Personeli");
            Label subtitle = CreateSubtitle(YonetimModu
                ? "Kullanıcı bilgileri, rol ve mağaza erişimi"
                : "Bayinize bağlı kullanıcıları ve mağaza erişimlerini görüntüleyin");

            txtArama = CreateTextBox();
            txtArama.Dock = DockStyle.Top;
            txtArama.Margin = new Padding(0, 0, 0, 10);
            txtArama.TextChanged += async (sender, e) => await PersonelleriYukleAsync(false);

            cmbGorunum = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 32,
                Font = new Font("Segoe UI", 9F),
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary,
                Margin = new Padding(0, 0, 0, 10)
            };
            cmbGorunum.Items.Add("Aktif Mağazadakiler");
            cmbGorunum.Items.Add("Tüm Personel");
            cmbGorunum.Items.Add("Rol Bazlı");
            cmbGorunum.SelectedIndex = 0;
            cmbGorunum.Enabled = YonetimModu;
            cmbGorunum.SelectedIndexChanged += async (sender, e) => await PersonelleriYukleAsync(false);

            lblAktifMagazaBaglami = new Label
            {
                Dock = DockStyle.Top,
                Height = 30,
                Text = "Aktif mağaza: " + AppSession.MagazaGorunumAdi,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                BackColor = AppColors.PrimarySoft,
                Padding = new Padding(10, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblArama = CreateFieldLabel("Personel Ara");
            lblArama.Dock = DockStyle.Top;

            personelKartListesi = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 4, 10),
                BackColor = AppColors.CardBackground
            };
            personelKartListesi.SizeChanged += (sender, e) => FitCards(personelKartListesi);

            Panel form = BuildFormPanel();
            form.Visible = YonetimModu;
            if (!YonetimModu)
                form.Height = 0;

            personelPanel.Controls.Add(personelKartListesi);
            personelPanel.Controls.Add(cmbGorunum);
            personelPanel.Controls.Add(txtArama);
            personelPanel.Controls.Add(lblAktifMagazaBaglami);
            personelPanel.Controls.Add(lblArama);
            personelPanel.Controls.Add(subtitle);
            personelPanel.Controls.Add(title);
            personelPanel.Controls.Add(form);
        }

        private Panel BuildFormPanel()
        {
            Panel form = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 270,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 12, 0, 0)
            };

            txtAdSoyad = CreateTextBox();
            txtKullaniciAdi = CreateTextBox();
            txtSifre = CreateTextBox();
            txtSifre.UseSystemPasswordChar = true;

            txtAdSoyad.TextChanged += TxtAdSoyad_TextChanged;
            txtKullaniciAdi.TextChanged += TxtKullaniciAdi_TextChanged;

            cmbRol = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Width = 220,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
            cmbRol.DisplayMember = "Ad";
            cmbRol.ValueMember = "Kod";
            cmbRol.Items.Add(new RolSecenegi("Admin", "Yönetici"));
            cmbRol.Items.Add(new RolSecenegi("StoreManager", "Mağaza Yetkilisi"));
            cmbRol.Items.Add(new RolSecenegi("SalesPerson", "Satış Personeli"));
            cmbRol.SelectedIndex = 2;

            chkAktif = new CheckBox
            {
                Text = "Aktif",
                Checked = true,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            btnYeni = CreateButton("Yeni", false);
            btnKaydet = CreateButton("Kaydet", true);
            btnYeni.Click += (sender, e) => FormuTemizle();
            btnKaydet.Click += BtnKaydet_Click;

            TableLayoutPanel formGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 158,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            formGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            formGrid.Controls.Add(CreateFormField("Ad Soyad", txtAdSoyad), 0, 0);
            formGrid.Controls.Add(CreateFormField("Kullanıcı Adı", txtKullaniciAdi), 1, 0);
            formGrid.Controls.Add(CreateFormField("Şifre", txtSifre), 0, 1);
            formGrid.Controls.Add(CreateFormField("Rol", cmbRol), 1, 1);

            Panel altSatir = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = Color.Transparent
            };

            chkAktif.Location = new Point(0, 10);
            btnYeni.Location = new Point(0, 40);
            btnKaydet.Location = new Point(108, 40);

            altSatir.Controls.Add(chkAktif);
            altSatir.Controls.Add(btnYeni);
            altSatir.Controls.Add(btnKaydet);

            form.Controls.Add(altSatir);
            form.Controls.Add(formGrid);

            return form;
        }

        private Panel CreateFormField(string labelText, Control control)
        {
            Panel wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 12, 10)
            };

            Label label = CreateFieldLabel(labelText);
            label.Dock = DockStyle.Top;
            label.Height = 22;

            control.Dock = DockStyle.Top;
            control.Height = 30;

            wrapper.Controls.Add(control);
            wrapper.Controls.Add(label);

            return wrapper;
        }

        private void BuildYetkiPanel()
        {
            Label title = CreateTitle(YonetimModu ? "Mağaza Yetkileri" : "Bağlı Mağazalar");
            Label subtitle = CreateSubtitle(YonetimModu
                ? "Personelin çalıştığı mağazaları kart üzerinden yönetin"
                : "Seçili personelin bayiniz içindeki mağaza erişimleri");

            lblSeciliPersonel = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Önce personel seçin",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.Primary,
                BackColor = Color.Transparent
            };

            lblYetkiliMagazaBaslik = CreateSectionLabel("Yetkili Mağazalar");
            lblAtanabilirMagazaBaslik = CreateSectionLabel("Atanabilir Mağazalar");

            yetkiliMagazaKartListesi = CreateMagazaListesi();
            atanabilirMagazaKartListesi = CreateMagazaListesi();

            TableLayoutPanel magazaYerlesim = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = YonetimModu ? 4 : 2,
                BackColor = AppColors.CardBackground
            };
            magazaYerlesim.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            magazaYerlesim.RowStyles.Add(new RowStyle(SizeType.Percent, YonetimModu ? 48F : 100F));
            if (YonetimModu)
            {
                magazaYerlesim.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
                magazaYerlesim.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
            }
            magazaYerlesim.Controls.Add(lblYetkiliMagazaBaslik, 0, 0);
            magazaYerlesim.Controls.Add(yetkiliMagazaKartListesi, 0, 1);
            if (YonetimModu)
            {
                magazaYerlesim.Controls.Add(lblAtanabilirMagazaBaslik, 0, 2);
                magazaYerlesim.Controls.Add(atanabilirMagazaKartListesi, 0, 3);
            }

            yetkiPanel.Controls.Add(magazaYerlesim);
            yetkiPanel.Controls.Add(lblSeciliPersonel);
            yetkiPanel.Controls.Add(subtitle);
            yetkiPanel.Controls.Add(title);
        }

        private FlowLayoutPanel CreateMagazaListesi()
        {
            FlowLayoutPanel list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(0, 8, 4, 4),
                BackColor = AppColors.CardBackground
            };
            list.SizeChanged += (sender, e) => FitCards(list);
            return list;
        }

        private async void PersonelPage_Load(object sender, EventArgs e)
        {
            FormuTemizle();
            await PersonelleriYukleAsync(true);
        }

        private async Task PersonelleriYukleAsync(bool ilkKaydiSec)
        {
            try
            {
                DataTable table = await GetPersonellerAsync();
                table = await ApplyPersonelScopeAsync(table);

                personelKartListesi.SuspendLayout();
                personelKartListesi.Controls.Clear();

                foreach (DataRow row in table.Rows)
                {
                    personelKartListesi.Controls.Add(CreatePersonelCard(row));
                }

                if (table.Rows.Count == 0)
                    personelKartListesi.Controls.Add(CreateEmptyCard("Personel bulunamadı."));

                FitCards(personelKartListesi);

                if (ilkKaydiSec && table.Rows.Count > 0)
                    PersonelSec(table.Rows[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                personelKartListesi.ResumeLayout(true);
            }
        }

        private Panel CreatePersonelCard(DataRow row)
        {
            Panel card = CreateBaseCard(92);
            card.Name = "PersonelCard";
            card.Tag = row;

            Label adSoyad = CreateCardLabel(GetText(row, "AdSoyad", "Personel"), 10.5F, FontStyle.Bold, AppColors.TextPrimary, 24);
            Label kullanici = CreateCardLabel("@" + GetText(row, "KullaniciAdi", "-"), 8.5F, FontStyle.Regular, AppColors.TextSecondary, 20);
            Label rol = CreateCardLabel(GetRolAdi(GetText(row, "Rol", "")) + "  |  " + GetInt(row, "MagazaSayisi") + " mağaza", 8.5F, FontStyle.Bold, AppColors.Primary, 20);

            adSoyad.Dock = DockStyle.Top;
            kullanici.Dock = DockStyle.Top;
            rol.Dock = DockStyle.Bottom;

            card.Controls.Add(rol);
            card.Controls.Add(kullanici);
            card.Controls.Add(adSoyad);

            AttachClick(card, () => PersonelSec(row));
            AddHover(card);

            return card;
        }

        private async void PersonelSec(DataRow row)
        {
            seciliKullaniciId = Convert.ToInt32(row["KullaniciId"]);
            kullaniciAdiOtomatik = false;

            SetText(txtKullaniciAdi, GetText(row, "KullaniciAdi", ""));
            txtSifre.Text = "";
            txtAdSoyad.Text = GetText(row, "AdSoyad", "");
            SelectRol(GetText(row, "Rol", "SalesPerson"));
            chkAktif.Checked = GetBool(row, "AktifMi");
            lblSeciliPersonel.Text = GetText(row, "AdSoyad", "Personel");

            await MagazalariYukleAsync();
            VurgulaSeciliPersonel();
        }

        private void VurgulaSeciliPersonel()
        {
            foreach (Control control in personelKartListesi.Controls)
            {
                Panel card = control as Panel;
                if (card == null || !(card.Tag is DataRow))
                    continue;

                DataRow row = (DataRow)card.Tag;
                bool selected = seciliKullaniciId.HasValue &&
                    Convert.ToInt32(row["KullaniciId"]) == seciliKullaniciId.Value;
                card.BackColor = selected ? AppColors.PrimarySoft : AppColors.Surface;
            }
        }

        private async Task MagazalariYukleAsync()
        {
            if (!seciliKullaniciId.HasValue)
                return;

            try
            {
                DataTable yetkili = await GetPersonelMagazalariAsync(seciliKullaniciId.Value);
                DataTable atanabilir = YonetimModu
                    ? await GetAtanabilirMagazalarAsync(seciliKullaniciId.Value)
                    : new DataTable();

                FillMagazaCards(yetkiliMagazaKartListesi, yetkili, true);
                if (YonetimModu)
                    FillMagazaCards(atanabilirMagazaKartListesi, atanabilir, false);

                lblYetkiliMagazaBaslik.Text = "Yetkili Mağazalar (" + yetkili.Rows.Count + ")";
                if (YonetimModu)
                    lblAtanabilirMagazaBaslik.Text = "Atanabilir Mağazalar (" + atanabilir.Rows.Count + ")";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FillMagazaCards(FlowLayoutPanel list, DataTable table, bool yetkili)
        {
            list.SuspendLayout();
            list.Controls.Clear();

            foreach (DataRow row in table.Rows)
            {
                list.Controls.Add(CreateMagazaCard(row, yetkili));
            }

            if (table.Rows.Count == 0)
                list.Controls.Add(CreateEmptyCard(yetkili ? "Atanmış mağaza yok." : "Atanabilir mağaza yok."));

            FitCards(list);
            list.ResumeLayout(true);
        }

        private Panel CreateMagazaCard(DataRow row, bool yetkili)
        {
            Panel card = CreateBaseCard(118);
            card.Tag = row;

            Label magaza = CreateCardLabel(GetText(row, "MagazaAdi", "Mağaza"), 10F, FontStyle.Bold, AppColors.TextPrimary, 24);
            Label musteri = CreateCardLabel(GetText(row, "MusteriAdi", "Müşteri"), 8.5F, FontStyle.Regular, AppColors.TextSecondary, 20);
            Label konum = CreateCardLabel(GetKonumText(row), 8.5F, FontStyle.Regular, AppColors.TextMuted, 20);
            Label ozet = CreateCardLabel(GetInt(row, "SiparisSayisi") + " sipariş  |  " + GetMoney(row, "ToplamCiro"), 8.5F, FontStyle.Bold, AppColors.Primary, 22);

            magaza.Dock = DockStyle.Top;
            musteri.Dock = DockStyle.Top;
            konum.Dock = DockStyle.Top;
            ozet.Dock = DockStyle.Top;

            if (YonetimModu)
            {
                Button action = CreateSmallButton(yetkili ? "Kaldır" : "Ata", !yetkili);
                action.Dock = DockStyle.Bottom;
                action.Click += async (sender, e) =>
                {
                    if (yetkili)
                        await MagazaKaldirAsync(row);
                    else
                        await MagazaAtaAsync(row);
                };
                card.Controls.Add(action);
            }

            card.Controls.Add(ozet);
            card.Controls.Add(konum);
            card.Controls.Add(musteri);
            card.Controls.Add(magaza);

            AddHover(card);
            return card;
        }

        private Panel CreateBaseCard(int height)
        {
            Panel card = new Panel
            {
                Width = 260,
                Height = height,
                Margin = new Padding(0, 0, 12, 12),
                Padding = new Padding(14, 12, 14, 12),
                BackColor = AppColors.Surface,
                Cursor = Cursors.Hand
            };
            card.Paint += Card_Paint;
            return card;
        }

        private Panel CreateEmptyCard(string text)
        {
            Panel card = CreateBaseCard(78);
            Label label = CreateCardLabel(text, 9F, FontStyle.Regular, AppColors.TextSecondary, 52);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            card.Controls.Add(label);
            return card;
        }

        private async Task MagazaAtaAsync(DataRow row)
        {
            if (!YonetimModu)
                return;

            if (!seciliKullaniciId.HasValue)
                return;

            try
            {
                await MagazaAtaKaydetAsync(seciliKullaniciId.Value, Convert.ToInt32(row["MagazaId"]));
                await MagazalariYukleAsync();
                await PersonelleriYukleAsync(false);
                VurgulaSeciliPersonel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task MagazaKaldirAsync(DataRow row)
        {
            if (!YonetimModu)
                return;

            try
            {
                await MagazaKaldirKaydetAsync(Convert.ToInt32(row["KullaniciMagazaId"]));
                await MagazalariYukleAsync();
                await PersonelleriYukleAsync(false);
                VurgulaSeciliPersonel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (!YonetimModu)
                return;

            try
            {
                btnKaydet.Enabled = false;
                int id = await PersonelKaydetAsync();

                seciliKullaniciId = id;
                kullaniciAdiOtomatik = false;
                await PersonelleriYukleAsync(false);
                SelectPersonelById(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnKaydet.Enabled = true;
            }
        }

        private Task<DataTable> GetPersonellerAsync()
        {
            return apiClient.GetPersonellerAsync(
                txtArama == null ? "" : txtArama.Text.Trim(),
                false,
                AppSession.KullaniciId,
                AppSession.AdminMi);
        }

        private async Task<DataTable> ApplyPersonelScopeAsync(DataTable table)
        {
            if (table == null)
                return new DataTable();

            string scope = cmbGorunum == null || cmbGorunum.SelectedItem == null
                ? "Aktif Mağazadakiler"
                : cmbGorunum.SelectedItem.ToString();

            if (scope == "Tüm Personel" && YonetimModu)
                return table;

            if (scope == "Rol Bazlı")
            {
                DataView view = table.DefaultView;
                if (table.Columns.Contains("Rol") && table.Columns.Contains("AdSoyad"))
                    view.Sort = "Rol ASC, AdSoyad ASC";
                return view.ToTable();
            }

            if (!AppSession.SeciliMagazaId.HasValue)
                return table;

            DataTable filtered = table.Clone();
            foreach (DataRow row in table.Rows)
            {
                int kullaniciId = GetInt(row, "KullaniciId");
                if (kullaniciId > 0 && await PersonelHasCurrentStoreAsync(kullaniciId))
                    filtered.ImportRow(row);
            }

            return filtered;
        }

        private async Task<bool> PersonelHasCurrentStoreAsync(int kullaniciId)
        {
            if (!AppSession.SeciliMagazaId.HasValue)
                return true;

            try
            {
                DataTable stores = await GetPersonelMagazalariAsync(kullaniciId);
                foreach (DataRow store in stores.Rows)
                {
                    if (GetInt(store, "MagazaId") == AppSession.SeciliMagazaId.Value)
                        return true;
                }
            }
            catch
            {
                // Compatibility guard: personnel/store permission filtering should not block the whole page
                // if an older API or partial database script returns an error for one user.
                return true;
            }

            return false;
        }

        private Task<DataTable> GetPersonelMagazalariAsync(int kullaniciId)
        {
            return apiClient.GetPersonelMagazalariAsync(kullaniciId, AppSession.KullaniciId, AppSession.AdminMi);
        }

        private Task<DataTable> GetAtanabilirMagazalarAsync(int kullaniciId)
        {
            return apiClient.GetAtanabilirMagazalarAsync(kullaniciId, "", AppSession.KullaniciId, AppSession.AdminMi);
        }

        private Task<int> PersonelKaydetAsync()
        {
            return apiClient.SavePersonelAsync(
                seciliKullaniciId,
                txtKullaniciAdi.Text,
                txtSifre.Text,
                txtAdSoyad.Text,
                GetSeciliRolKodu(),
                chkAktif.Checked);
        }

        private Task MagazaAtaKaydetAsync(int kullaniciId, int magazaId)
        {
            return apiClient.AssignPersonelMagazaAsync(kullaniciId, magazaId);
        }

        private Task MagazaKaldirKaydetAsync(int kullaniciMagazaId)
        {
            return apiClient.RemovePersonelMagazaAsync(kullaniciMagazaId);
        }

        private void SelectPersonelById(int kullaniciId)
        {
            foreach (Control control in personelKartListesi.Controls)
            {
                Panel card = control as Panel;
                if (card == null || !(card.Tag is DataRow))
                    continue;

                DataRow row = (DataRow)card.Tag;
                if (Convert.ToInt32(row["KullaniciId"]) == kullaniciId)
                {
                    PersonelSec(row);
                    return;
                }
            }
        }

        private void FormuTemizle()
        {
            seciliKullaniciId = null;
            kullaniciAdiOtomatik = true;
            SetText(txtKullaniciAdi, "");
            txtSifre.Text = "";
            txtAdSoyad.Text = "";
            SelectRol("SalesPerson");
            chkAktif.Checked = true;
            lblSeciliPersonel.Text = "Yeni personel";
            yetkiliMagazaKartListesi.Controls.Clear();
            atanabilirMagazaKartListesi.Controls.Clear();
            lblYetkiliMagazaBaslik.Text = "Yetkili Mağazalar";
            lblAtanabilirMagazaBaslik.Text = "Atanabilir Mağazalar";
            VurgulaSeciliPersonel();
        }

        private void TxtAdSoyad_TextChanged(object sender, EventArgs e)
        {
            if (!kullaniciAdiOtomatik || seciliKullaniciId.HasValue)
                return;

            SetText(txtKullaniciAdi, KullaniciAdiOner(txtAdSoyad.Text));
        }

        private void TxtKullaniciAdi_TextChanged(object sender, EventArgs e)
        {
            if (kullaniciAdiKodlaDegisiyor)
                return;

            if (txtKullaniciAdi.Focused)
                kullaniciAdiOtomatik = false;
        }

        private string KullaniciAdiOner(string adSoyad)
        {
            string normalized = adSoyad.ToLowerInvariant()
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c");

            StringBuilder builder = new StringBuilder();
            bool lastDot = false;

            foreach (char c in normalized)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    builder.Append(c);
                    lastDot = false;
                }
                else if (!lastDot && builder.Length > 0)
                {
                    builder.Append('.');
                    lastDot = true;
                }
            }

            return builder.ToString().Trim('.');
        }

        private void SetText(TextBox textBox, string value)
        {
            kullaniciAdiKodlaDegisiyor = textBox == txtKullaniciAdi;
            textBox.Text = value;
            kullaniciAdiKodlaDegisiyor = false;
        }

        private string GetSeciliRolKodu()
        {
            RolSecenegi secenek = cmbRol.SelectedItem as RolSecenegi;
            return secenek == null ? "SalesPerson" : secenek.Kod;
        }

        private void SelectRol(string kod)
        {
            foreach (object item in cmbRol.Items)
            {
                RolSecenegi secenek = item as RolSecenegi;
                if (secenek != null && secenek.Kod == kod)
                {
                    cmbRol.SelectedItem = item;
                    return;
                }
            }

            cmbRol.SelectedIndex = 2;
        }

        private string GetRolAdi(string kod)
        {
            if (kod == "Admin")
                return "Yönetici";

            if (kod == "StoreManager")
                return "Mağaza Yetkilisi";

            return "Satış Personeli";
        }

        private void AddFormControl(Panel parent, string labelText, Control control, int x, int y)
        {
            Label label = CreateFieldLabel(labelText);
            label.Location = new Point(x, y);
            control.Location = new Point(x, y + 22);
            control.Width = 220;

            parent.Controls.Add(label);
            parent.Controls.Add(control);
        }

        private void AttachClick(Control control, Action action)
        {
            control.Click += (sender, e) => action();

            foreach (Control child in control.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += (sender, e) => action();
            }
        }

        private void AddHover(Panel card)
        {
            card.MouseEnter += (sender, e) =>
            {
                if (!(card.Tag is DataRow) || !IsSeciliPersonelCard(card))
                    card.BackColor = AppColors.PrimarySoft;
            };

            card.MouseLeave += (sender, e) =>
            {
                if (!(card.Tag is DataRow) || !IsSeciliPersonelCard(card))
                    card.BackColor = AppColors.Surface;
            };
        }

        private bool IsSeciliPersonelCard(Panel card)
        {
            if (card.Name != "PersonelCard")
                return false;

            if (!seciliKullaniciId.HasValue || !(card.Tag is DataRow))
                return false;

            DataRow row = (DataRow)card.Tag;

            if (!row.Table.Columns.Contains("KullaniciId"))
                return false;

            return Convert.ToInt32(row["KullaniciId"]) == seciliKullaniciId.Value;
        }

        private void FitCards(FlowLayoutPanel list)
        {
            int width = list.ClientSize.Width - 34;
            if (list == atanabilirMagazaKartListesi || list == yetkiliMagazaKartListesi)
                width = Math.Max(240, (list.ClientSize.Width - 44) / 2);
            else
                width = Math.Max(240, width);

            foreach (Control control in list.Controls)
            {
                control.Width = width;
            }
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

        private Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
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
                Width = 96,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = primary ? AppColors.Primary : AppColors.PrimarySoft,
                ForeColor = primary ? Color.White : AppColors.Primary
            };
            if (primary)
                ButtonStyleHelper.ApplyPrimary(button);
            else
                ButtonStyleHelper.ApplySoft(button);
            return button;
        }

        private Button CreateSmallButton(string text, bool primary)
        {
            Button button = CreateButton(text, primary);
            button.Height = 28;
            button.Width = 90;
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

        private string GetText(DataRow row, string columnName, string defaultValue)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            string value = Convert.ToString(row[columnName]);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private int GetInt(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(row[columnName]);
        }

        private decimal GetDecimal(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToDecimal(row[columnName]);
        }

        private bool GetBool(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return false;

            return Convert.ToBoolean(row[columnName]);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Control card = sender as Control;
            if (card == null)
                return;

            using (Pen pen = new Pen(AppColors.Border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            }
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;

            if (anaYerlesim != null)
                anaYerlesim.BackColor = AppColors.Background;

            if (personelPanel != null)
                personelPanel.BackColor = AppColors.CardBackground;

            if (yetkiPanel != null)
                yetkiPanel.BackColor = AppColors.CardBackground;
        }

        private class RolSecenegi
        {
            public string Kod { get; private set; }
            public string Ad { get; private set; }

            public RolSecenegi(string kod, string ad)
            {
                Kod = kod;
                Ad = ad;
            }

            public override string ToString()
            {
                return Ad;
            }
        }
    }
}
