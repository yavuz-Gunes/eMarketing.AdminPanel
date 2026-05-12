using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Forms
{
    public class BayiYetkiliModalForm : Form
    {
        private readonly ApiDataClient apiClient = new ApiDataClient();
        private readonly int bayiYetkiliId;
        private readonly int? contextMagazaId;
        private readonly int? contextBayiId;
        private bool loading;

        private ComboBox cmbKullanici;
        private ComboBox cmbBayi;
        private ComboBox cmbMagaza;
        private ComboBox cmbYetkiTipi;
        private TextBox txtAdSoyad;
        private TextBox txtTelefon;
        private TextBox txtEmail;
        private TextBox txtRol;
        private TextBox txtNotlar;
        private CheckBox chkAktif;
        private Button btnKaydet;
        private Button btnIptal;

        public bool IsSaved { get; private set; }

        public BayiYetkiliModalForm(int bayiYetkiliId = 0, int? magazaId = null, int? bayiId = null)
        {
            this.bayiYetkiliId = bayiYetkiliId;
            contextMagazaId = magazaId ?? AppSession.SeciliMagazaId;
            contextBayiId = bayiId ?? AppSession.SeciliMusteriId;
            BuildLayout();
            Load += BayiYetkiliModalForm_Load;
        }

        private void BuildLayout()
        {
            Text = bayiYetkiliId > 0 ? "Sipariş Yetkilisi Düzenle" : "Yeni Sipariş Yetkilisi";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ShowIcon = false;
            BackColor = AppColors.Background;
            Width = 760;
            Height = 650;

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(26, 18, 26, 12)
            };

            Label title = new Label
            {
                Text = Text,
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            Label subtitle = new Label
            {
                Text = "Aktif mağazadaki bir personeli sipariş yetkilisi olarak bağlayın.",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            header.Controls.Add(subtitle);
            header.Controls.Add(title);

            Panel body = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Padding = new Padding(24, 20, 24, 16)
            };

            Panel form = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(22)
            };

            cmbKullanici = CreateComboBox(22, 42, 654);
            cmbBayi = CreateComboBox(22, 116, 654);
            cmbMagaza = CreateComboBox(22, 190, 654);
            cmbYetkiTipi = CreateComboBox(386, 338, 290);
            cmbBayi.Enabled = false;
            cmbMagaza.Enabled = false;
            cmbYetkiTipi.Enabled = false;
            txtAdSoyad = CreateTextBox(22, 264, 310, true);
            txtTelefon = CreateTextBox(366, 264, 310, true);
            txtEmail = CreateTextBox(22, 338, 310, true);
            txtRol = CreateTextBox(22, 412, 310, true);
            txtNotlar = CreateTextBox(366, 412, 310, false);
            chkAktif = new CheckBox
            {
                Text = "Aktif",
                Location = new Point(22, 482),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            cmbKullanici.SelectedIndexChanged += CmbKullanici_SelectedIndexChanged;
            cmbBayi.SelectedIndexChanged += CmbBayi_SelectedIndexChanged;

            AddLabel(form, "Yetkili Personel", 22, 18);
            AddLabel(form, "Bayi", 22, 92);
            AddLabel(form, "Aktif Mağaza", 22, 166);
            AddLabel(form, "Ad Soyad", 22, 240);
            AddLabel(form, "Telefon", 366, 240);
            AddLabel(form, "E-Posta", 22, 314);
            AddLabel(form, "Yetki Tipi", 386, 314);
            AddLabel(form, "Rol", 22, 388);
            AddLabel(form, "Not", 366, 388);

            form.Controls.Add(cmbKullanici);
            form.Controls.Add(cmbBayi);
            form.Controls.Add(cmbMagaza);
            form.Controls.Add(txtAdSoyad);
            form.Controls.Add(txtTelefon);
            form.Controls.Add(txtEmail);
            form.Controls.Add(cmbYetkiTipi);
            form.Controls.Add(txtRol);
            form.Controls.Add(txtNotlar);
            form.Controls.Add(chkAktif);
            body.Controls.Add(form);

            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(24, 16, 24, 16)
            };

            btnIptal = CreateButton("İptal", false);
            btnKaydet = CreateButton(bayiYetkiliId > 0 ? "Güncelle" : "Kaydet", true);
            btnIptal.Click += (sender, e) => Close();
            btnKaydet.Click += BtnKaydet_Click;

            footer.Controls.Add(btnIptal);
            footer.Controls.Add(btnKaydet);
            footer.Resize += (sender, e) =>
            {
                btnKaydet.Location = new Point(footer.Width - btnKaydet.Width - 24, 18);
                btnIptal.Location = new Point(btnKaydet.Left - btnIptal.Width - 12, 18);
            };

            Controls.Add(body);
            Controls.Add(footer);
            Controls.Add(header);

            AcceptButton = btnKaydet;
            CancelButton = btnIptal;
        }

        private async void BayiYetkiliModalForm_Load(object sender, EventArgs e)
        {
            try
            {
                loading = true;
                LoadYetkiTipleri();
                await LoadKullanicilarAsync();
                await LoadBayilerAsync();
                loading = false;

                if (bayiYetkiliId > 0)
                    await LoadYetkiliAsync();
                else
                    FillUserFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
            finally
            {
                loading = false;
            }
        }

        private void LoadYetkiTipleri()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Kod", typeof(string));
            table.Columns.Add("Ad", typeof(string));
            table.Rows.Add("SiparisYetkilisi", "Sipariş Yetkilisi");

            cmbYetkiTipi.DisplayMember = "Ad";
            cmbYetkiTipi.ValueMember = "Kod";
            cmbYetkiTipi.DataSource = table;
        }

        private async Task LoadKullanicilarAsync()
        {
            if (!contextMagazaId.HasValue && bayiYetkiliId <= 0)
                throw new InvalidOperationException("Sipariş yetkilisi eklemek için önce aktif mağaza seçin.");

            DataTable users = await apiClient.GetPersonellerAsync("", true, contextMagazaId);
            for (int i = users.Rows.Count - 1; i >= 0; i--)
            {
                if (string.Equals(GetText(users.Rows[i], "Rol", ""), "Admin", StringComparison.OrdinalIgnoreCase))
                    users.Rows.RemoveAt(i);
            }

            if (!users.Columns.Contains("KullaniciGosterim"))
                users.Columns.Add("KullaniciGosterim", typeof(string));

            foreach (DataRow row in users.Rows)
            {
                string role = GetRolGorunenAd(GetText(row, "Rol", ""));
                row["KullaniciGosterim"] = GetText(row, "AdSoyad", "Personel") + " - " + role;
            }

            cmbKullanici.DisplayMember = "KullaniciGosterim";
            cmbKullanici.ValueMember = "KullaniciId";
            cmbKullanici.DataSource = users;
        }

        private async Task LoadBayilerAsync()
        {
            if (contextMagazaId.HasValue)
            {
                DataRow magaza = await apiClient.GetMagazaByIdAsync(contextMagazaId.Value, AppSession.KullaniciId, AppSession.AdminMi);
                if (magaza == null)
                    throw new InvalidOperationException("Aktif mağaza bilgisi alınamadı.");

                DataTable contextBayiler = new DataTable();
                contextBayiler.Columns.Add("CustomerId", typeof(int));
                contextBayiler.Columns.Add("BayiGosterim", typeof(string));
                contextBayiler.Rows.Add(GetInt(magaza, "MusteriId"), GetText(magaza, "MusteriAdi", "Bayi"));
                cmbBayi.DisplayMember = "BayiGosterim";
                cmbBayi.ValueMember = "CustomerId";
                cmbBayi.DataSource = contextBayiler;

                DataTable magazalar = new DataTable();
                magazalar.Columns.Add("CustomerStoreId", typeof(int));
                magazalar.Columns.Add("StoreName", typeof(string));
                magazalar.Rows.Add(GetInt(magaza, "MagazaId"), GetText(magaza, "MagazaAdi", "Mağaza"));
                cmbMagaza.DisplayMember = "StoreName";
                cmbMagaza.ValueMember = "CustomerStoreId";
                cmbMagaza.DataSource = magazalar;
                return;
            }

            DataTable bayiler = await apiClient.GetBayilerAsync("", 1);
            if (!bayiler.Columns.Contains("BayiGosterim"))
                bayiler.Columns.Add("BayiGosterim", typeof(string));

            foreach (DataRow row in bayiler.Rows)
                row["BayiGosterim"] = GetText(row, "CompanyName", GetText(row, "FullName", "Bayi"));

            cmbBayi.DisplayMember = "BayiGosterim";
            cmbBayi.ValueMember = "CustomerId";
            cmbBayi.DataSource = bayiler;

            await LoadMagazalarAsync();
        }

        private async Task LoadMagazalarAsync()
        {
            int bayiId;
            if (!TryGetSelectedInt(cmbBayi, out bayiId))
            {
                cmbMagaza.DataSource = null;
                return;
            }

            DataTable magazalar = await apiClient.GetBayiMagazalariAsync(bayiId, 1);
            cmbMagaza.DisplayMember = "StoreName";
            cmbMagaza.ValueMember = "CustomerStoreId";
            cmbMagaza.DataSource = magazalar;
        }

        private async Task LoadYetkiliAsync()
        {
            DataRow row = await apiClient.GetBayiYetkiliByIdAsync(bayiYetkiliId);
            if (row == null)
            {
                MessageBox.Show("Yetkili bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            cmbKullanici.SelectedValue = Convert.ToInt32(row["KullaniciId"]);
            cmbBayi.SelectedValue = Convert.ToInt32(row["BayiId"]);
            await LoadMagazalarAsync();
            cmbMagaza.SelectedValue = Convert.ToInt32(row["MagazaId"]);
            cmbYetkiTipi.SelectedValue = GetText(row, "YetkiTipi", "SiparisYetkilisi");
            txtNotlar.Text = GetText(row, "Notlar", "");
            chkAktif.Checked = GetBool(row, "AktifMi", true);
            FillUserFields();
        }

        private async void CmbBayi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            try
            {
                await LoadMagazalarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbKullanici_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillUserFields();
        }

        private void FillUserFields()
        {
            DataRowView rowView = cmbKullanici.SelectedItem as DataRowView;
            if (rowView == null)
            {
                txtAdSoyad.Text = "";
                txtTelefon.Text = "";
                txtEmail.Text = "";
                txtRol.Text = "";
                return;
            }

            txtAdSoyad.Text = GetText(rowView.Row, "AdSoyad", "");
            txtTelefon.Text = GetText(rowView.Row, "Telefon", "");
            txtEmail.Text = GetText(rowView.Row, "Email", "");
            txtRol.Text = GetRolGorunenAd(GetText(rowView.Row, "Rol", ""));
        }

        private async void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                btnKaydet.Enabled = false;
                await apiClient.SaveBayiYetkiliAsync(
                    bayiYetkiliId > 0 ? (int?)bayiYetkiliId : null,
                    Convert.ToInt32(cmbKullanici.SelectedValue),
                    Convert.ToInt32(cmbBayi.SelectedValue),
                    Convert.ToInt32(cmbMagaza.SelectedValue),
                    "SiparisYetkilisi",
                    txtNotlar.Text,
                    chkAktif.Checked);

                IsSaved = true;
                DialogResult = DialogResult.OK;
                Close();
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

        private bool ValidateForm()
        {
            if (!TryGetSelectedInt(cmbKullanici, out _))
            {
                MessageBox.Show("Yetkili personel seçimi zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbKullanici.Focus();
                return false;
            }

            if (!TryGetSelectedInt(cmbBayi, out _))
            {
                MessageBox.Show("Bayi seçimi zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBayi.Focus();
                return false;
            }

            if (!TryGetSelectedInt(cmbMagaza, out _))
            {
                MessageBox.Show("Mağaza seçimi zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbMagaza.Focus();
                return false;
            }

            return true;
        }

        private bool TryGetSelectedInt(ComboBox comboBox, out int value)
        {
            value = 0;
            if (comboBox.SelectedValue == null || comboBox.SelectedValue == DBNull.Value)
                return false;

            return int.TryParse(Convert.ToString(comboBox.SelectedValue), out value) && value > 0;
        }

        private void AddLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            });
        }

        private TextBox CreateTextBox(int x, int y, int width, bool readOnly)
        {
            TextBox textBox = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = readOnly ? Color.FromArgb(248, 250, 252) : AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary,
                ReadOnly = readOnly
            };
            ButtonStyleHelper.ApplyInput(textBox);
            return textBox;
        }

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            ComboBox comboBox = new ComboBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
            ButtonStyleHelper.ApplyDropdown(comboBox);
            return comboBox;
        }

        private Button CreateButton(string text, bool primary)
        {
            Button button = new Button
            {
                Text = text,
                Width = 112,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = primary ? AppColors.Primary : AppColors.CardBackground,
                ForeColor = primary ? Color.White : AppColors.TextSecondary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            if (primary)
                ButtonStyleHelper.ApplyPrimary(button);
            else
                ButtonStyleHelper.ApplyOutline(button);

            return button;
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
            if (row == null || !row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(row[columnName]);
        }

        private bool GetBool(DataRow row, string columnName, bool defaultValue)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            return Convert.ToBoolean(row[columnName]);
        }

        private string GetRolGorunenAd(string rol)
        {
            if (string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase))
                return "Admin";
            if (string.Equals(rol, "Yonetici", StringComparison.OrdinalIgnoreCase) || string.Equals(rol, "StoreManager", StringComparison.OrdinalIgnoreCase))
                return "Yönetici";
            if (string.Equals(rol, "MagazaYetkilisi", StringComparison.OrdinalIgnoreCase))
                return "Mağaza Yetkilisi";
            if (string.Equals(rol, "SalesPerson", StringComparison.OrdinalIgnoreCase) || string.Equals(rol, "Personel", StringComparison.OrdinalIgnoreCase))
                return "Personel";
            return string.IsNullOrWhiteSpace(rol) ? "Rol yok" : rol;
        }
    }
}
