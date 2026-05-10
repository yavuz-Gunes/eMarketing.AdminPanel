using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Forms
{
    public partial  class    CustomerModalForm : Form
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();
        private readonly int _customerId;

        private Panel headerPanel;
        private Panel bodyPanel;
        private Panel footerPanel;

        private Label lblTitle;
        private Label lblSubtitle;

        private Label lblFullName;
        private TextBox txtFullName;

        private Label lblCompanyName;
        private TextBox txtCompanyName;

        private Label lblAuthorizedPerson;
        private TextBox txtAuthorizedPerson;

        private Label lblPhone;
        private TextBox txtPhone;

        private Label lblEmail;
        private TextBox txtEmail;

        private Label lblTaxNumber;
        private TextBox txtTaxNumber;

        private Label lblTaxOffice;
        private TextBox txtTaxOffice;

        private Label lblAddress;
        private TextBox txtAddress;

        private Label lblCustomerType;
        private ComboBox cmbCustomerType;

        private CheckBox chkIsActive;

        private Button btnCancel;
        private Button btnSave;

        public bool IsSaved { get; private set; }

        public CustomerModalForm(int customerId = 0)
        {
            _customerId = customerId;

            BuildLayout();

            Load += CustomerModalForm_Load;
        }

        private async void CustomerModalForm_Load(object sender, EventArgs e)
        {
            LoadCustomerTypes();

            if (_customerId > 0)
                await LoadCustomerAsync();
        }

        private void BuildLayout()
        {
            Text = _customerId > 0 ? "Müşteri Düzenle" : "Yeni Müşteri";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = AppColors.Background;
            Width = 720;
            Height = 660;

            BuildHeader();
            BuildBody();
            BuildFooter();

            Controls.Add(bodyPanel);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void BuildHeader()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(26, 18, 26, 12)
            };

            lblTitle = new Label
            {
                Text = _customerId > 0 ? "Müşteri Düzenle" : "Yeni Müşteri",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSubtitle = new Label
            {
                Text = "Firma, yetkili kişi, iletişim ve vergi bilgilerini yönetin.",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(lblTitle);
        }

        private void BuildBody()
        {
            bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Padding = new Padding(24, 20, 24, 16)
            };

            Panel formPanel = new Panel
            {
                Location = new Point(24, 20),
                Size = new Size(660, 440),
                BackColor = AppColors.CardBackground,
                Padding = new Padding(22)
            };

            lblFullName = CreateLabel("Müşteri / Firma Görünen Adı", 22, 18);
            txtFullName = CreateTextBox(22, 42, 290);

            lblCompanyName = CreateLabel("Firma Ünvanı", 334, 18);
            txtCompanyName = CreateTextBox(334, 42, 282);

            lblAuthorizedPerson = CreateLabel("Yetkili Kişi", 22, 86);
            txtAuthorizedPerson = CreateTextBox(22, 110, 290);

            lblCustomerType = CreateLabel("Müşteri Tipi", 334, 86);
            cmbCustomerType = new ComboBox
            {
                Location = new Point(334, 110),
                Width = 282,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            ButtonStyleHelper.ApplyDropdown(cmbCustomerType);

            lblPhone = CreateLabel("Telefon", 22, 154);
            txtPhone = CreateTextBox(22, 178, 190);

            lblEmail = CreateLabel("E-Posta", 234, 154);
            txtEmail = CreateTextBox(234, 178, 382);

            lblTaxNumber = CreateLabel("Vergi No", 22, 222);
            txtTaxNumber = CreateTextBox(22, 246, 190);

            lblTaxOffice = CreateLabel("Vergi Dairesi", 234, 222);
            txtTaxOffice = CreateTextBox(234, 246, 382);

            lblAddress = CreateLabel("Adres", 22, 290);
            txtAddress = new TextBox
            {
                Location = new Point(22, 314),
                Width = 594,
                Height = 70,
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            ButtonStyleHelper.ApplyInput(txtAddress);

            chkIsActive = new CheckBox
            {
                Text = "Müşteri aktif olarak kullanılabilir",
                Location = new Point(22, 398),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                Checked = true,
                Cursor = Cursors.Hand
            };

            formPanel.Controls.Add(lblFullName);
            formPanel.Controls.Add(txtFullName);
            formPanel.Controls.Add(lblCompanyName);
            formPanel.Controls.Add(txtCompanyName);
            formPanel.Controls.Add(lblAuthorizedPerson);
            formPanel.Controls.Add(txtAuthorizedPerson);
            formPanel.Controls.Add(lblCustomerType);
            formPanel.Controls.Add(cmbCustomerType);
            formPanel.Controls.Add(lblPhone);
            formPanel.Controls.Add(txtPhone);
            formPanel.Controls.Add(lblEmail);
            formPanel.Controls.Add(txtEmail);
            formPanel.Controls.Add(lblTaxNumber);
            formPanel.Controls.Add(txtTaxNumber);
            formPanel.Controls.Add(lblTaxOffice);
            formPanel.Controls.Add(txtTaxOffice);
            formPanel.Controls.Add(lblAddress);
            formPanel.Controls.Add(txtAddress);
            formPanel.Controls.Add(chkIsActive);

            bodyPanel.Controls.Add(formPanel);
        }

        private void BuildFooter()
        {
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 74,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(24, 16, 24, 16)
            };

            btnCancel = new Button
            {
                Text = "İptal",
                Width = 110,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AppColors.TextSecondary,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnCancel.FlatAppearance.BorderColor = AppColors.Border;
            ButtonStyleHelper.ApplyOutline(btnCancel);
            btnCancel.Click += (s, e) => Close();

            btnSave = new Button
            {
                Text = _customerId > 0 ? "Güncelle" : "Kaydet",
                Width = 120,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnSave.FlatAppearance.BorderSize = 0;
            ButtonStyleHelper.ApplyPrimary(btnSave);
            btnSave.Click += BtnSave_Click;

            footerPanel.Controls.Add(btnCancel);
            footerPanel.Controls.Add(btnSave);

            footerPanel.Resize += (s, e) =>
            {
                btnSave.Location = new Point(footerPanel.Width - btnSave.Width - 24, 18);
                btnCancel.Location = new Point(btnSave.Left - btnCancel.Width - 12, 18);
            };
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            TextBox textBox = new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.White,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            ButtonStyleHelper.ApplyInput(textBox);
            return textBox;
        }

        private void LoadCustomerTypes()
        {
            cmbCustomerType.Items.Clear();
            cmbCustomerType.Items.Add("Toptan");
            cmbCustomerType.Items.Add("Kurumsal");
            cmbCustomerType.Items.Add("Bireysel");
            cmbCustomerType.SelectedIndex = 0;
        }

        private async Task LoadCustomerAsync()
        {
            try
            {
                DataRow row = await GetCustomerByIdAsync(_customerId);

                if (row == null)
                {
                    MessageBox.Show("Müşteri bulunamadı.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                txtFullName.Text = GetRowText(row, "FullName");
                txtCompanyName.Text = GetRowText(row, "CompanyName");
                txtAuthorizedPerson.Text = GetRowText(row, "AuthorizedPerson");
                txtPhone.Text = GetRowText(row, "Phone");
                txtEmail.Text = GetRowText(row, "Email");
                txtTaxNumber.Text = GetRowText(row, "TaxNumber");
                txtTaxOffice.Text = GetRowText(row, "TaxOffice");
                txtAddress.Text = GetRowText(row, "Address");

                string customerType = GetRowText(row, "CustomerType");

                if (!string.IsNullOrWhiteSpace(customerType) && cmbCustomerType.Items.Contains(customerType))
                    cmbCustomerType.SelectedItem = customerType;
                else
                    cmbCustomerType.SelectedIndex = 0;

                chkIsActive.Checked = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri bilgisi yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private string GetRowText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return "";

            if (row[columnName] == DBNull.Value)
                return "";

            return row[columnName]?.ToString() ?? "";
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string fullName = txtFullName.Text.Trim();
                string companyName = txtCompanyName.Text.Trim();
                string authorizedPerson = txtAuthorizedPerson.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string email = txtEmail.Text.Trim();
                string taxNumber = txtTaxNumber.Text.Trim();
                string taxOffice = txtTaxOffice.Text.Trim();
                string address = txtAddress.Text.Trim();
                string customerType = cmbCustomerType.Text.Trim();

                if (!ValidateForm(fullName, companyName, phone, email, taxNumber))
                    return;

                if (_customerId > 0)
                {
                    await UpdateCustomerAsync(fullName, companyName, authorizedPerson, phone, email, taxNumber, taxOffice, address, customerType);
                }
                else
                {
                    int newCustomerId = await InsertCustomerAsync(fullName, companyName, authorizedPerson, phone, email, taxNumber, taxOffice, address, customerType);

                    await CreateDefaultStoreIfNeededAsync(newCustomerId, fullName, companyName, phone, authorizedPerson, address);
                }

                IsSaved = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri kaydedilirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm(string fullName, string companyName, string phone, string email, string taxNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Müşteri görünen adı boş bırakılamaz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            if (!IsValidName(fullName))
            {
                MessageBox.Show("Müşteri adı geçersiz karakter içeriyor.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(companyName) && !IsValidName(companyName))
            {
                MessageBox.Show("Firma adı geçersiz karakter içeriyor.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompanyName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(phone) && phone.Length < 10)
            {
                MessageBox.Show("Telefon numarası en az 10 karakter olmalıdır.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                MessageBox.Show("Geçerli bir e-posta adresi giriniz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(taxNumber) && taxNumber.Length < 10)
            {
                MessageBox.Show("Vergi numarası en az 10 karakter olmalıdır.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaxNumber.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidName(string value)
        {
            foreach (char c in value)
            {
                if (!char.IsLetterOrDigit(c)
                    && !char.IsWhiteSpace(c)
                    && c != '-'
                    && c != '.'
                    && c != ','
                    && c != '&'
                    && c != '/'
                    && c != '('
                    && c != ')')
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private async Task CreateDefaultStoreIfNeededAsync(
            int customerId,
            string fullName,
            string companyName,
            string phone,
            string responsiblePerson,
            string address)
        {
            if (customerId <= 0)
                return;

            string storeBaseName = !string.IsNullOrWhiteSpace(companyName)
                ? companyName
                : fullName;

            string storeName = storeBaseName + " Ana Mağaza";

            await InsertCustomerStoreAsync(customerId, storeName, address, phone, responsiblePerson);
        }

        private Task<DataRow> GetCustomerByIdAsync(int customerId)
        {
            return _apiClient.GetBayiByIdAsync(customerId);
        }

        private Task<int> InsertCustomerAsync(string fullName, string companyName, string authorizedPerson, string phone, string email, string taxNumber, string taxOffice, string address, string customerType)
        {
            return _apiClient.InsertBayiAsync(fullName, companyName, authorizedPerson, phone, email, taxNumber, taxOffice, address, customerType, chkIsActive.Checked);
        }

        private Task UpdateCustomerAsync(string fullName, string companyName, string authorizedPerson, string phone, string email, string taxNumber, string taxOffice, string address, string customerType)
        {
            return _apiClient.UpdateBayiAsync(_customerId, fullName, companyName, authorizedPerson, phone, email, taxNumber, taxOffice, address, customerType, chkIsActive.Checked);
        }

        private Task InsertCustomerStoreAsync(int customerId, string storeName, string address, string phone, string responsiblePerson)
        {
            return _apiClient.InsertBayiMagazaAsync(customerId, storeName, "", "", address, phone, responsiblePerson, true);
        }
    }
}
