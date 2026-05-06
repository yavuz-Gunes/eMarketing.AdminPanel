using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public class BayiYetkiliModalForm : Form
    {
        private readonly BayiYetkiliRepository yetkiliRepo = new BayiYetkiliRepository();
        private readonly CustomerRepository bayiRepo = new CustomerRepository();
        private readonly int bayiYetkiliId;

        private ComboBox cmbBayi;
        private ComboBox cmbMagaza;
        private TextBox txtAdSoyad;
        private TextBox txtTelefon;
        private TextBox txtEmail;
        private TextBox txtGorev;
        private TextBox txtNotlar;
        private CheckBox chkAktif;
        private Button btnKaydet;
        private Button btnIptal;

        public bool IsSaved { get; private set; }

        public BayiYetkiliModalForm(int bayiYetkiliId = 0)
        {
            this.bayiYetkiliId = bayiYetkiliId;
            BuildLayout();
            Load += BayiYetkiliModalForm_Load;
        }

        private void BuildLayout()
        {
            Text = bayiYetkiliId > 0 ? "Müşteri / Yetkili Düzenle" : "Yeni Müşteri / Yetkili";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ShowIcon = false;
            BackColor = AppColors.Background;
            Width = 680;
            Height = 620;

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(26, 18, 26, 12)
            };

            Label title = new Label
            {
                Text = bayiYetkiliId > 0 ? "Müşteri / Yetkili Düzenle" : "Yeni Müşteri / Yetkili",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            Label subtitle = new Label
            {
                Text = "Bayiye bağlı sipariş veren kişi bilgilerini yönetin.",
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
                Location = new Point(24, 20),
                Size = new Size(620, 390),
                BackColor = AppColors.CardBackground,
                Padding = new Padding(22)
            };

            cmbBayi = CreateComboBox(22, 42, 576);
            cmbBayi.SelectedIndexChanged += CmbBayi_SelectedIndexChanged;
            cmbMagaza = CreateComboBox(22, 110, 576);
            txtAdSoyad = CreateTextBox(22, 178, 280);
            txtTelefon = CreateTextBox(322, 178, 276);
            txtEmail = CreateTextBox(22, 246, 280);
            txtGorev = CreateTextBox(322, 246, 276);
            txtNotlar = CreateTextBox(22, 314, 576);
            chkAktif = new CheckBox
            {
                Text = "Aktif",
                Location = new Point(22, 354),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            AddLabel(form, "Bayi", 22, 18);
            AddLabel(form, "Bağlı Mağaza", 22, 86);
            AddLabel(form, "Ad Soyad", 22, 154);
            AddLabel(form, "Telefon", 322, 154);
            AddLabel(form, "E-Posta", 22, 222);
            AddLabel(form, "Görev", 322, 222);
            AddLabel(form, "Not", 22, 290);

            form.Controls.Add(cmbBayi);
            form.Controls.Add(cmbMagaza);
            form.Controls.Add(txtAdSoyad);
            form.Controls.Add(txtTelefon);
            form.Controls.Add(txtEmail);
            form.Controls.Add(txtGorev);
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

        private void BayiYetkiliModalForm_Load(object sender, EventArgs e)
        {
            LoadBayiler();

            if (bayiYetkiliId > 0)
                LoadYetkili();
        }

        private void LoadBayiler()
        {
            DataTable bayiler = bayiRepo.GetActiveCustomers();

            if (!bayiler.Columns.Contains("BayiGosterim"))
                bayiler.Columns.Add("BayiGosterim", typeof(string));

            foreach (DataRow row in bayiler.Rows)
                row["BayiGosterim"] = GetText(row, "CompanyName", GetText(row, "FullName", "Bayi"));

            cmbBayi.DisplayMember = "BayiGosterim";
            cmbBayi.ValueMember = "CustomerId";
            cmbBayi.DataSource = bayiler;

            LoadMagazalar();
        }

        private void LoadMagazalar()
        {
            int bayiId;
            if (!TryGetSelectedInt(cmbBayi, out bayiId))
                return;

            DataTable magazalar = bayiRepo.GetCustomerStores(bayiId, 1);
            DataTable source = magazalar.Clone();

            DataRow bayiGeneli = source.NewRow();
            if (source.Columns.Contains("CustomerStoreId"))
                bayiGeneli["CustomerStoreId"] = DBNull.Value;
            if (source.Columns.Contains("StoreName"))
                bayiGeneli["StoreName"] = "Bayi geneli";
            source.Rows.Add(bayiGeneli);

            foreach (DataRow row in magazalar.Rows)
                source.ImportRow(row);

            cmbMagaza.DisplayMember = "StoreName";
            cmbMagaza.ValueMember = "CustomerStoreId";
            cmbMagaza.DataSource = source;
        }

        private bool TryGetSelectedInt(ComboBox comboBox, out int value)
        {
            value = 0;

            if (comboBox.SelectedValue == null || comboBox.SelectedValue == DBNull.Value)
                return false;

            return int.TryParse(Convert.ToString(comboBox.SelectedValue), out value);
        }

        private void LoadYetkili()
        {
            DataRow row = yetkiliRepo.GetYetkiliById(bayiYetkiliId);
            if (row == null)
            {
                MessageBox.Show("Müşteri/yetkili bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            cmbBayi.SelectedValue = Convert.ToInt32(row["BayiId"]);
            LoadMagazalar();

            if (row["MagazaId"] != DBNull.Value)
                cmbMagaza.SelectedValue = Convert.ToInt32(row["MagazaId"]);

            txtAdSoyad.Text = GetText(row, "AdSoyad", "");
            txtTelefon.Text = GetText(row, "Telefon", "");
            txtEmail.Text = GetText(row, "Email", "");
            txtGorev.Text = GetText(row, "Gorev", "");
            txtNotlar.Text = GetText(row, "Notlar", "");
            chkAktif.Checked = row["AktifMi"] != DBNull.Value && Convert.ToBoolean(row["AktifMi"]);
        }

        private void CmbBayi_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMagazalar();
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            int bayiId = Convert.ToInt32(cmbBayi.SelectedValue);
            int? magazaId = null;

            if (cmbMagaza.SelectedValue != null && cmbMagaza.SelectedValue != DBNull.Value)
            {
                int parsed;
                if (int.TryParse(Convert.ToString(cmbMagaza.SelectedValue), out parsed) && parsed > 0)
                    magazaId = parsed;
            }

            yetkiliRepo.Kaydet(
                bayiYetkiliId > 0 ? (int?)bayiYetkiliId : null,
                bayiId,
                magazaId,
                txtAdSoyad.Text,
                txtTelefon.Text,
                txtEmail.Text,
                txtGorev.Text,
                txtNotlar.Text,
                chkAktif.Checked);

            IsSaved = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            if (cmbBayi.SelectedValue == null)
            {
                MessageBox.Show("Bayi seçimi zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBayi.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text))
            {
                MessageBox.Show("Ad soyad zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAdSoyad.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) &&
                !Regex.IsMatch(txtEmail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("Geçerli bir e-posta adresi giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtTelefon.Text) && txtTelefon.Text.Trim().Length < 10)
            {
                MessageBox.Show("Telefon numarası en az 10 karakter olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelefon.Focus();
                return false;
            }

            return true;
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

        private TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
        }

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
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
                Height = 36,
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

        private string GetText(DataRow row, string columnName, string defaultValue)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            string value = Convert.ToString(row[columnName]);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
    }
}
