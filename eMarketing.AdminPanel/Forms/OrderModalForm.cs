using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Forms
{
    public partial class OrderModalForm : Form
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();

        private Label lblTitle;

        private Label lblMagaza;
        private ComboBox cmbMagaza;

        private Label lblSatisKanali;
        private ComboBox cmbSatisKanali;

        private Label lblCustomerName;
        private TextBox txtCustomerName;

        private Label lblYetkili;
        private ComboBox cmbYetkili;

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

        private async void OrderModalForm_Load(object sender, EventArgs e)
        {
            await LoadMagazalarAsync();
            await LoadProductsAsync();
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
            Height = 610;

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

            lblMagaza = new Label
            {
                Text = "Bayi / Mağaza",
                AutoSize = true,
                Location = new Point(24, 68),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            cmbMagaza = new ComboBox
            {
                Location = new Point(24, 92),
                Width = 490,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbMagaza.SelectedIndexChanged += CmbMagaza_SelectedIndexChanged;
            ButtonStyleHelper.ApplyDropdown(cmbMagaza);

            lblSatisKanali = new Label
            {
                Text = "Satış Tipi",
                AutoSize = true,
                Location = new Point(24, 130),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            cmbSatisKanali = new ComboBox
            {
                Location = new Point(24, 154),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbSatisKanali.Items.Add("Bayi Satışı");
            cmbSatisKanali.SelectedIndex = 0;
            ButtonStyleHelper.ApplyDropdown(cmbSatisKanali);

            lblCustomerName = new Label
            {
                Text = "Bayi Adı",
                AutoSize = true,
                Location = new Point(24, 192),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtCustomerName = new TextBox
            {
                Location = new Point(24, 216),
                Width = 490,
                Font = new Font("Segoe UI", 10F),
                MaxLength = 150,
                ReadOnly = true
            };

            lblYetkili = new Label
            {
                Text = "Sipariş Yetkilisi",
                AutoSize = true,
                Location = new Point(24, 254),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            cmbYetkili = new ComboBox
            {
                Location = new Point(24, 278),
                Width = 490,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            cmbYetkili.SelectedIndexChanged += CmbYetkili_SelectedIndexChanged;
            ButtonStyleHelper.ApplyDropdown(cmbYetkili);

            lblCustomerEmail = new Label
            {
                Text = "E-Posta",
                AutoSize = true,
                Location = new Point(24, 316),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtCustomerEmail = new TextBox
            {
                Location = new Point(24, 340),
                Width = 230,
                Font = new Font("Segoe UI", 10F),
                MaxLength = 200
            };

            lblCustomerPhone = new Label
            {
                Text = "Telefon",
                AutoSize = true,
                Location = new Point(284, 316),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtCustomerPhone = new TextBox
            {
                Location = new Point(284, 340),
                Width = 230,
                Font = new Font("Segoe UI", 10F),
                MaxLength = 30
            };

            txtCustomerPhone.KeyPress += TxtCustomerPhone_KeyPress;
            txtCustomerPhone.Leave += TxtCustomerPhone_Leave;

            lblProduct = new Label
            {
                Text = "Ürün",
                AutoSize = true,
                Location = new Point(24, 378),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            cmbProduct = new ComboBox
            {
                Location = new Point(24, 402),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;
            ButtonStyleHelper.ApplyDropdown(cmbProduct);

            lblQuantity = new Label
            {
                Text = "Adet",
                AutoSize = true,
                Location = new Point(344, 378),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtQuantity = new TextBox
            {
                Location = new Point(344, 402),
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
                Location = new Point(24, 442),
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            txtTotalPrice = new TextBox
            {
                Location = new Point(24, 466),
                Width = 160,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true
            };
            ButtonStyleHelper.ApplyInput(txtCustomerName);
            ButtonStyleHelper.ApplyInput(txtCustomerEmail);
            ButtonStyleHelper.ApplyInput(txtCustomerPhone);
            ButtonStyleHelper.ApplyInput(txtQuantity);
            ButtonStyleHelper.ApplyInput(txtTotalPrice);

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
            ButtonStyleHelper.ApplyOutline(btnCancel);
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
            ButtonStyleHelper.ApplyPrimary(btnSave);
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
            Controls.Add(cmbYetkili);
            Controls.Add(lblYetkili);
            Controls.Add(txtCustomerName);
            Controls.Add(lblCustomerName);
            Controls.Add(cmbSatisKanali);
            Controls.Add(lblSatisKanali);
            Controls.Add(cmbMagaza);
            Controls.Add(lblMagaza);
            Controls.Add(lblTitle);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                DataTable products = await GetProductsForOrderAsync();

                if (!products.Columns.Contains("UrunGosterim"))
                    products.Columns.Add("UrunGosterim", typeof(string));

                foreach (DataRow row in products.Rows)
                {
                    string urunAdi = row.Table.Columns.Contains("UrunAdi") ? Convert.ToString(row["UrunAdi"]) : "Ürün";
                    string stok = row.Table.Columns.Contains("Stok") ? Convert.ToString(row["Stok"]) : "0";
                    row["UrunGosterim"] = urunAdi + " (Stok: " + stok + ")";
                }

                cmbProduct.DisplayMember = "UrunGosterim";
                cmbProduct.ValueMember = "UrunId";
                cmbProduct.DataSource = products;

                RecalculateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Task<DataTable> GetProductsForOrderAsync()
        {
            return _apiClient.GetProductsAsync("", 1, 0);
        }

        private Task<DataTable> GetMagazaSecimListesiAsync()
        {
            return _apiClient.GetMagazaSecimListesiAsync("", true, AppSession.KullaniciId, AppSession.AdminMi);
        }

        private Task<DataTable> GetYetkililerAsync(int bayiId, int magazaId)
        {
            return _apiClient.GetSiparisYetkilileriAsync(magazaId);
        }

        private async Task LoadMagazalarAsync()
        {
            try
            {
                DataTable magazalar = await GetMagazaSecimListesiAsync();

                magazalar = AktifMagazaSecimineGoreFiltrele(magazalar);

                if (magazalar.Rows.Count == 0)
                {
                    cmbMagaza.DataSource = null;
                    MessageBox.Show("Aktif bayi/mağaza sipariş oluşturmak için bulunamadı. Lütfen mağaza seçimini kontrol edin.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!magazalar.Columns.Contains("MagazaGosterim"))
                    magazalar.Columns.Add("MagazaGosterim", typeof(string));

                foreach (DataRow row in magazalar.Rows)
                {
                    row["MagazaGosterim"] =
                        Convert.ToString(row["MusteriAdi"]) + " - " +
                        Convert.ToString(row["MagazaAdi"]);
                }

                cmbMagaza.DisplayMember = "MagazaGosterim";
                cmbMagaza.ValueMember = "MagazaId";
                cmbMagaza.DataSource = magazalar;

                if (AppSession.SeciliMagazaId.HasValue)
                    cmbMagaza.SelectedValue = AppSession.SeciliMagazaId.Value;

                cmbMagaza.Enabled = !SiparisAktifMagazayaKilitli() && magazalar.Rows.Count > 1;
                await MagazaBilgisiniFormaYansitAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable AktifMagazaSecimineGoreFiltrele(DataTable magazalar)
        {
            if (magazalar == null || !SiparisAktifMagazayaKilitli())
                return magazalar;

            DataTable filtered = magazalar.Clone();
            int aktifMagazaId = AppSession.SeciliMagazaId.Value;

            foreach (DataRow row in magazalar.Rows)
            {
                if (row.Table.Columns.Contains("MagazaId")
                    && row["MagazaId"] != DBNull.Value
                    && Convert.ToInt32(row["MagazaId"]) == aktifMagazaId)
                {
                    filtered.ImportRow(row);
                }
            }

            return filtered;
        }

        private bool SiparisAktifMagazayaKilitli()
        {
            return AppSession.SeciliMagazaId.HasValue && !AppSession.TumMagazalar;
        }

        private async void CmbMagaza_SelectedIndexChanged(object sender, EventArgs e)
        {
            await MagazaBilgisiniFormaYansitAsync();
        }

        private async Task MagazaBilgisiniFormaYansitAsync()
        {
            DataRowView rowView = cmbMagaza.SelectedItem as DataRowView;
            if (rowView == null)
                return;

            txtCustomerName.Text = Convert.ToString(rowView["MusteriAdi"]);

            if (rowView.Row.Table.Columns.Contains("Telefon") && rowView["Telefon"] != DBNull.Value)
                txtCustomerPhone.Text = Convert.ToString(rowView["Telefon"]);

            txtCustomerEmail.Text = "";
            await LoadYetkililerAsync(rowView.Row);
        }

        private async Task LoadYetkililerAsync(DataRow magazaRow)
        {
            try
            {
                int bayiId = Convert.ToInt32(magazaRow["MusteriId"]);
                int magazaId = Convert.ToInt32(magazaRow["MagazaId"]);
                DataTable yetkililer = await GetYetkililerAsync(bayiId, magazaId);

                if (!yetkililer.Columns.Contains("YetkiliGosterim"))
                    yetkililer.Columns.Add("YetkiliGosterim", typeof(string));
                if (!yetkililer.Columns.Contains("BayiYetkiliId"))
                    yetkililer.Columns.Add("BayiYetkiliId", typeof(int));
                if (!yetkililer.Columns.Contains("AdSoyad"))
                    yetkililer.Columns.Add("AdSoyad", typeof(string));

                DataTable source = yetkililer.Clone();
                DataRow empty = source.NewRow();
                empty["BayiYetkiliId"] = DBNull.Value;
                empty["AdSoyad"] = "Yetkili seçilmedi";
                empty["YetkiliGosterim"] = "Yetkili seçilmedi";
                source.Rows.Add(empty);

                foreach (DataRow row in yetkililer.Rows)
                {
                    string adSoyad = Convert.ToString(row["AdSoyad"]);
                    string gorev = row.Table.Columns.Contains("YetkiTipiGorunenAd") ? Convert.ToString(row["YetkiTipiGorunenAd"]) : "";
                    row["YetkiliGosterim"] = string.IsNullOrWhiteSpace(gorev)
                        ? adSoyad
                        : adSoyad + " - " + gorev;
                    source.ImportRow(row);
                }

                cmbYetkili.DisplayMember = "YetkiliGosterim";
                cmbYetkili.ValueMember = "BayiYetkiliId";
                cmbYetkili.DataSource = source;
            }
            catch
            {
                cmbYetkili.DataSource = null;
                cmbYetkili.Items.Clear();
                cmbYetkili.Items.Add("Yetkili seçilmedi");
                cmbYetkili.SelectedIndex = 0;
            }
        }

        private void CmbYetkili_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRowView rowView = cmbYetkili.SelectedItem as DataRowView;
            if (rowView == null || rowView["BayiYetkiliId"] == DBNull.Value)
                return;

            if (rowView.Row.Table.Columns.Contains("Email") && rowView["Email"] != DBNull.Value)
                txtCustomerEmail.Text = Convert.ToString(rowView["Email"]);

            if (rowView.Row.Table.Columns.Contains("Telefon") && rowView["Telefon"] != DBNull.Value)
                txtCustomerPhone.Text = Convert.ToString(rowView["Telefon"]);
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

            if (phone.Length < 10 || phone.Length > 30)
            {
                MessageBox.Show("Telefon numarası 10-30 hane aralığında olmalıdır.",
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

        private async void BtnSave_Click(object sender, EventArgs e)
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
                    (customerPhone.Length < 10 || customerPhone.Length > 30))
                {
                    MessageBox.Show("Telefon numarası 10-30 hane aralığında olmalıdır.",
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

                if (cmbMagaza.SelectedValue == null || !(cmbMagaza.SelectedItem is DataRowView magazaRow))
                {
                    MessageBox.Show("Sipariş için bayi/mağaza seçiniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbMagaza.Focus();
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
                int magazaId = Convert.ToInt32(cmbMagaza.SelectedValue);
                int? bayiYetkiliId = GetSelectedYetkiliId();
                if (!bayiYetkiliId.HasValue)
                {
                    MessageBox.Show("Sipariş oluşturmak için aktif bir sipariş yetkilisi seçmelisiniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbYetkili.Focus();
                    return;
                }

                customerName = Convert.ToString(magazaRow["MusteriAdi"]);

                btnSave.Enabled = false;
                await AddOrderAsync(
                    customerName,
                    customerEmail,
                    customerPhone,
                    productId,
                    quantity,
                    totalPrice,
                    magazaId,
                    GetSiparisTipi(),
                    "AdminPanel",
                    bayiYetkiliId);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private string GetSiparisTipi()
        {
            return "Bayi";
        }

        private Task AddOrderAsync(
            string customerName,
            string customerEmail,
            string customerPhone,
            int productId,
            int quantity,
            decimal totalPrice,
            int magazaId,
            string siparisTipi,
            string siparisKaynagi,
            int? bayiYetkiliId)
        {
            return _apiClient.AddOrderAsync(
                customerName,
                customerEmail,
                customerPhone,
                productId,
                quantity,
                totalPrice,
                magazaId,
                siparisTipi,
                siparisKaynagi,
                bayiYetkiliId);
        }

        private int? GetSelectedYetkiliId()
        {
            if (cmbYetkili.SelectedValue == null || cmbYetkili.SelectedValue == DBNull.Value)
                return null;

            int id;
            if (!int.TryParse(Convert.ToString(cmbYetkili.SelectedValue), out id) || id <= 0)
                return null;

            return id;
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
