using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public class CustomerStoreModalForm : Form
    {
        private readonly CustomerRepository _repo = new CustomerRepository();

        private readonly int _customerId;
        private readonly int _customerStoreId;

        private Panel headerPanel;
        private Panel bodyPanel;
        private Panel footerPanel;

        private Label lblTitle;
        private Label lblSubtitle;

        private Label lblStoreName;
        private TextBox txtStoreName;

        private Label lblCity;
        private TextBox txtCity;

        private Label lblDistrict;
        private TextBox txtDistrict;

        private Label lblPhone;
        private TextBox txtPhone;

        private Label lblResponsiblePerson;
        private TextBox txtResponsiblePerson;

        private Label lblAddress;
        private TextBox txtAddress;

        private CheckBox chkIsActive;

        private Button btnCancel;
        private Button btnSave;

        public bool IsSaved { get; private set; }

        public CustomerStoreModalForm(int customerId, int customerStoreId = 0)
        {
            _customerId = customerId;
            _customerStoreId = customerStoreId;

            BuildLayout();

            Load += CustomerStoreModalForm_Load;
        }

        private void CustomerStoreModalForm_Load(object sender, EventArgs e)
        {
            if (_customerStoreId > 0)
                LoadCustomerStore();
        }

        private void BuildLayout()
        {
            Text = _customerStoreId > 0 ? "Mağaza Düzenle" : "Yeni Mağaza";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = AppColors.Background;
            Width = 660;
            Height = 600;

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
                Text = _customerStoreId > 0 ? "Mağaza Düzenle" : "Yeni Mağaza",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSubtitle = new Label
            {
                Text = "Müşteriye ait mağaza, şube ve teslimat bilgilerini yönetin.",
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
                Size = new Size(600, 360),
                BackColor = AppColors.CardBackground,
                Padding = new Padding(22)
            };

            lblStoreName = CreateLabel("Mağaza / Şube Adı", 22, 18);
            txtStoreName = CreateTextBox(22, 42, 556);

            lblCity = CreateLabel("Şehir", 22, 86);
            txtCity = CreateTextBox(22, 110, 170);

            lblDistrict = CreateLabel("İlçe", 212, 86);
            txtDistrict = CreateTextBox(212, 110, 170);

            lblPhone = CreateLabel("Telefon", 402, 86);
            txtPhone = CreateTextBox(402, 110, 176);

            lblResponsiblePerson = CreateLabel("Sorumlu Kişi", 22, 154);
            txtResponsiblePerson = CreateTextBox(22, 178, 260);

            lblAddress = CreateLabel("Adres", 22, 222);
            txtAddress = new TextBox
            {
                Location = new Point(22, 246),
                Width = 556,
                Height = 72,
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            chkIsActive = new CheckBox
            {
                Text = "Mağaza aktif olarak kullanılabilir",
                Location = new Point(308, 181),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                Checked = true,
                Cursor = Cursors.Hand
            };

            formPanel.Controls.Add(lblStoreName);
            formPanel.Controls.Add(txtStoreName);
            formPanel.Controls.Add(lblCity);
            formPanel.Controls.Add(txtCity);
            formPanel.Controls.Add(lblDistrict);
            formPanel.Controls.Add(txtDistrict);
            formPanel.Controls.Add(lblPhone);
            formPanel.Controls.Add(txtPhone);
            formPanel.Controls.Add(lblResponsiblePerson);
            formPanel.Controls.Add(txtResponsiblePerson);
            formPanel.Controls.Add(chkIsActive);
            formPanel.Controls.Add(lblAddress);
            formPanel.Controls.Add(txtAddress);

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
            btnCancel.Click += (s, e) => Close();

            btnSave = new Button
            {
                Text = _customerStoreId > 0 ? "Güncelle" : "Kaydet",
                Width = 120,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnSave.FlatAppearance.BorderSize = 0;
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
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.White,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void LoadCustomerStore()
        {
            try
            {
                DataRow row = _repo.GetCustomerStoreById(_customerStoreId);

                if (row == null)
                {
                    MessageBox.Show("Mağaza bulunamadı.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                txtStoreName.Text = GetRowText(row, "StoreName");
                txtCity.Text = GetRowText(row, "City");
                txtDistrict.Text = GetRowText(row, "District");
                txtAddress.Text = GetRowText(row, "Address");
                txtPhone.Text = GetRowText(row, "Phone");
                txtResponsiblePerson.Text = GetRowText(row, "ResponsiblePerson");

                chkIsActive.Checked = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mağaza bilgisi yüklenirken hata: " + ex.Message,
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

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string storeName = txtStoreName.Text.Trim();
                string city = txtCity.Text.Trim();
                string district = txtDistrict.Text.Trim();
                string address = txtAddress.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string responsiblePerson = txtResponsiblePerson.Text.Trim();

                if (!ValidateForm(storeName, city, district, phone, responsiblePerson))
                    return;

                if (_customerStoreId > 0)
                {
                    _repo.UpdateCustomerStore(
                        _customerStoreId,
                        storeName,
                        city,
                        district,
                        address,
                        phone,
                        responsiblePerson,
                        chkIsActive.Checked);
                }
                else
                {
                    _repo.InsertCustomerStore(
                        _customerId,
                        storeName,
                        city,
                        district,
                        address,
                        phone,
                        responsiblePerson,
                        chkIsActive.Checked);
                }

                IsSaved = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mağaza kaydedilirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm(
            string storeName,
            string city,
            string district,
            string phone,
            string responsiblePerson)
        {
            if (_customerId <= 0)
            {
                MessageBox.Show("Mağaza eklemek için önce müşteri seçilmelidir.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(storeName))
            {
                MessageBox.Show("Mağaza adı boş bırakılamaz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStoreName.Focus();
                return false;
            }

            if (!IsValidText(storeName))
            {
                MessageBox.Show("Mağaza adı geçersiz karakter içeriyor.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStoreName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(city) && !IsValidText(city))
            {
                MessageBox.Show("Şehir bilgisi geçersiz karakter içeriyor.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCity.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(district) && !IsValidText(district))
            {
                MessageBox.Show("İlçe bilgisi geçersiz karakter içeriyor.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDistrict.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(responsiblePerson) && !IsValidText(responsiblePerson))
            {
                MessageBox.Show("Sorumlu kişi bilgisi geçersiz karakter içeriyor.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtResponsiblePerson.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(phone) && phone.Length < 10)
            {
                MessageBox.Show("Telefon numarası en az 10 karakter olmalıdır.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPhone.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidText(string value)
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
    }
}