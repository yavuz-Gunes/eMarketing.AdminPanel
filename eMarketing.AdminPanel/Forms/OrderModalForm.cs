using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public partial class OrderModalForm : Form
    {
        private readonly OrderRepository _orderRepo = new OrderRepository();
        private readonly ProductRepository _productRepo = new ProductRepository();

        private Label lblTitle;

        private Label lblCustomerName;
        private TextBox txtCustomerName;

        private Label lblCustomerEmail;
        private TextBox txtCustomerEmail;

        private Label lblCustomerPhone;
        private TextBox txtCustomerPhone;

        private Label lblProduct;
        private ComboBox cmbProduct;

        private Label lblQuantity;
        private TextBox txtQuantity;

        private Label lblTotalPrice;
        private TextBox txtTotalPrice;

        private Panel footerPanel;
        private Button btnCancel;
        private Button btnSave;

        public OrderModalForm()
        {
            InitializeComponent();
            BuildLayout();
            Load += OrderModalForm_Load;
        }

        private void OrderModalForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void BuildLayout()
        {
            Text = "Yeni Sipariş";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Width = 560;
            Height = 470;

            lblTitle = new Label
            {
                Text = "Yeni Sipariş",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = false,
                Height = 38,
                Dock = DockStyle.Top,
                Padding = new Padding(20, 16, 20, 0)
            };

            lblCustomerName = new Label
            {
                Text = "Müşteri Adı",
                AutoSize = true,
                Location = new Point(24, 68),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtCustomerName = new TextBox
            {
                Location = new Point(24, 92),
                Width = 490,
                Font = new Font("Segoe UI", 10F),
                MaxLength = 150
            };

            lblCustomerEmail = new Label
            {
                Text = "E-Posta",
                AutoSize = true,
                Location = new Point(24, 130),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtCustomerEmail = new TextBox
            {
                Location = new Point(24, 154),
                Width = 490,
                Font = new Font("Segoe UI", 10F),
                MaxLength = 200
            };

            lblCustomerPhone = new Label
            {
                Text = "Telefon",
                AutoSize = true,
                Location = new Point(24, 192),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtCustomerPhone = new TextBox
            {
                Location = new Point(24, 216),
                Width = 490,
                Font = new Font("Segoe UI", 10F),
                MaxLength = 11
            };

            txtCustomerPhone.KeyPress += TxtCustomerPhone_KeyPress;
            txtCustomerPhone.Leave += TxtCustomerPhone_Leave;

            lblProduct = new Label
            {
                Text = "Ürün",
                AutoSize = true,
                Location = new Point(24, 254),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            cmbProduct = new ComboBox
            {
                Location = new Point(24, 278),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;

            lblQuantity = new Label
            {
                Text = "Adet",
                AutoSize = true,
                Location = new Point(344, 254),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtQuantity = new TextBox
            {
                Location = new Point(344, 278),
                Width = 80,
                Font = new Font("Segoe UI", 10F),
                Text = "1"
            };

            txtQuantity.TextChanged += TxtQuantity_TextChanged;
            txtQuantity.KeyPress += TxtQuantity_KeyPress;
            txtQuantity.Leave += TxtQuantity_Leave;

            lblTotalPrice = new Label
            {
                Text = "Toplam Tutar",
                AutoSize = true,
                Location = new Point(24, 318),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtTotalPrice = new TextBox
            {
                Location = new Point(24, 342),
                Width = 160,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true
            };

            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.White
            };

            btnCancel = new Button
            {
                Text = "İptal",
                Width = 100,
                Height = 36,
                Location = new Point(304, 14),
                FlatStyle = FlatStyle.Flat
            };

            btnCancel.FlatAppearance.BorderColor = Color.Gainsboro;
            btnCancel.Click += (s, e) => Close();

            btnSave = new Button
            {
                Text = "Kaydet",
                Width = 100,
                Height = 36,
                Location = new Point(414, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White
            };

            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            footerPanel.Controls.Add(btnCancel);
            footerPanel.Controls.Add(btnSave);

            Controls.Add(footerPanel);
            Controls.Add(txtTotalPrice);
            Controls.Add(lblTotalPrice);
            Controls.Add(txtQuantity);
            Controls.Add(lblQuantity);
            Controls.Add(cmbProduct);
            Controls.Add(lblProduct);
            Controls.Add(txtCustomerPhone);
            Controls.Add(lblCustomerPhone);
            Controls.Add(txtCustomerEmail);
            Controls.Add(lblCustomerEmail);
            Controls.Add(txtCustomerName);
            Controls.Add(lblCustomerName);
            Controls.Add(lblTitle);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadProducts()
        {
            try
            {
                DataTable products = _productRepo.GetProductsForOrder();

                cmbProduct.DisplayMember = "UrunGosterim";
                cmbProduct.ValueMember = "UrunId";
                cmbProduct.DataSource = products;

                RecalculateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecalculateTotal();
        }

        private void TxtQuantity_TextChanged(object sender, EventArgs e)
        {
            RecalculateTotal();
        }

        private void TxtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void TxtQuantity_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                txtQuantity.Text = "1";
                RecalculateTotal();
                return;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Adet alanına yalnızca 0'dan büyük tam sayı giriniz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtQuantity.Text = "1";
                txtQuantity.Focus();
                RecalculateTotal();
            }
        }

        private void TxtCustomerPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void TxtCustomerPhone_Leave(object sender, EventArgs e)
        {
            string phone = txtCustomerPhone.Text.Trim();

            if (string.IsNullOrWhiteSpace(phone))
                return;

            if (phone.Length < 10 || phone.Length > 11)
            {
                MessageBox.Show("Telefon numarası 10 veya 11 haneli olmalıdır.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCustomerPhone.Focus();
            }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private void RecalculateTotal()
        {
            try
            {
                if (!(cmbProduct.SelectedItem is DataRowView rowView))
                {
                    txtTotalPrice.Text = "0.00";
                    return;
                }

                decimal price = Convert.ToDecimal(rowView["Fiyat"]);
                int stock = rowView["Stok"] != DBNull.Value ? Convert.ToInt32(rowView["Stok"]) : 0;

                int quantity = 1;
                int.TryParse(txtQuantity.Text.Trim(), out quantity);

                if (quantity <= 0)
                    quantity = 1;

                if (stock > 0 && quantity > stock)
                {
                    txtTotalPrice.Text = "0.00";
                    return;
                }

                decimal total = price * quantity;
                txtTotalPrice.Text = total.ToString("N2", CultureInfo.InvariantCulture);
            }
            catch
            {
                txtTotalPrice.Text = "0.00";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string customerName = txtCustomerName.Text.Trim();
                string customerEmail = txtCustomerEmail.Text.Trim();
                string customerPhone = txtCustomerPhone.Text.Trim();

                if (string.IsNullOrWhiteSpace(customerName))
                {
                    MessageBox.Show("Müşteri adı boş bırakılamaz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomerName.Focus();
                    return;
                }

                if (customerName.Length < 2)
                {
                    MessageBox.Show("Müşteri adı en az 2 karakter olmalıdır.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomerName.Focus();
                    return;
                }

                if (!IsValidCustomerName(customerName))
                {
                    MessageBox.Show("Müşteri adı yalnızca harf, boşluk ve izin verilen karakterleri içerebilir.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomerName.Focus();
                    return;
                }

                if (!IsValidEmail(customerEmail))
                {
                    MessageBox.Show("Geçerli bir e-posta adresi giriniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomerEmail.Focus();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(customerPhone) &&
                    (customerPhone.Length < 10 || customerPhone.Length > 11))
                {
                    MessageBox.Show("Telefon numarası 10 veya 11 haneli olmalıdır.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCustomerPhone.Focus();
                    return;
                }

                if (cmbProduct.SelectedValue == null || !(cmbProduct.SelectedItem is DataRowView rowView))
                {
                    MessageBox.Show("Ürün seçiniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbProduct.Focus();
                    return;
                }

                if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity))
                {
                    MessageBox.Show("Adet alanına yalnızca sayı giriniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtQuantity.Focus();
                    return;
                }

                if (quantity <= 0)
                {
                    MessageBox.Show("Adet 0'dan büyük olmalıdır.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtQuantity.Focus();
                    return;
                }

                int stock = rowView["Stok"] != DBNull.Value ? Convert.ToInt32(rowView["Stok"]) : 0;

                if (quantity > stock)
                {
                    MessageBox.Show("Girilen adet mevcut stoktan fazla olamaz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtQuantity.Focus();
                    return;
                }

                decimal price = Convert.ToDecimal(rowView["Fiyat"]);
                decimal totalPrice = price * quantity;
                int productId = Convert.ToInt32(cmbProduct.SelectedValue);

                _orderRepo.AddOrder(
                    customerName,
                    customerEmail,
                    customerPhone,
                    productId,
                    quantity,
                    totalPrice);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş kaydedilirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsValidCustomerName(string customerName)
        {
            if (string.IsNullOrWhiteSpace(customerName))
                return false;

            foreach (char c in customerName)
            {
                if (!char.IsLetter(c)
                    && !char.IsWhiteSpace(c)
                    && c != '-'
                    && c != '.'
                    && c != '\'')
                {
                    return false;
                }
            }

            return true;
        }
    }
}