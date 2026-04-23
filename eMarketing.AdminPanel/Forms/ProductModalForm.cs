using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.DataAccess;

namespace eMarketing.AdminPanel.Forms
{
    public partial class ProductModalForm : Form
    {
        private readonly ProductRepository _productRepo = new ProductRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();
        private readonly int _productId;

        private Label lblTitle;

        private Label lblProductName;
        private TextBox txtProductName;

        private Label lblDescription;
        private TextBox txtDescription;

        private Label lblPrice;
        private TextBox txtPrice;

        private Label lblStock;
        private TextBox txtStock;

        private Label lblCategory;
        private ComboBox cmbCategory;

        private Label lblImageUrl;
        private TextBox txtImageUrl;

        private CheckBox chkIsActive;

        private Panel footerPanel;
        private Button btnCancel;
        private Button btnSave;

        public bool IsSaved { get; private set; }

        public ProductModalForm(int productId = 0)
        {
            _productId = productId;

            BuildLayout();
            Load += ProductModalForm_Load;
        }

        private void ProductModalForm_Load(object sender, EventArgs e)
        {
            LoadCategories();

            if (_productId > 0)
                LoadProduct();
        }

        private void BuildLayout()
        {
            Text = _productId > 0 ? "Ürün Düzenle" : "Yeni Ürün";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Width = 560;
            Height = 520;

            lblTitle = new Label
            {
                Text = _productId > 0 ? "Ürün Düzenle" : "Yeni Ürün",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = false,
                Height = 38,
                Dock = DockStyle.Top,
                Padding = new Padding(20, 16, 20, 0)
            };

            lblProductName = new Label
            {
                Text = "Ürün Adı",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 68)
            };

            txtProductName = new TextBox
            {
                Location = new Point(24, 92),
                Width = 490,
                Font = new Font("Segoe UI", 10F)
            };

            lblDescription = new Label
            {
                Text = "Açıklama",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 130)
            };

            txtDescription = new TextBox
            {
                Location = new Point(24, 154),
                Width = 490,
                Height = 72,
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                ScrollBars = ScrollBars.Vertical
            };

            lblPrice = new Label
            {
                Text = "Fiyat",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 238)
            };

            txtPrice = new TextBox
            {
                Location = new Point(24, 262),
                Width = 150,
                Font = new Font("Segoe UI", 10F)
            };

            lblStock = new Label
            {
                Text = "Stok",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(194, 238)
            };

            txtStock = new TextBox
            {
                Location = new Point(194, 262),
                Width = 120,
                Font = new Font("Segoe UI", 10F)
            };

            lblCategory = new Label
            {
                Text = "Kategori",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(334, 238)
            };

            cmbCategory = new ComboBox
            {
                Location = new Point(334, 262),
                Width = 180,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            lblImageUrl = new Label
            {
                Text = "Görsel URL",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 306)
            };

            txtImageUrl = new TextBox
            {
                Location = new Point(24, 330),
                Width = 490,
                Font = new Font("Segoe UI", 10F)
            };

            chkIsActive = new CheckBox
            {
                Text = "Aktif",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(24, 372),
                Checked = true
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
            Controls.Add(chkIsActive);
            Controls.Add(txtImageUrl);
            Controls.Add(lblImageUrl);
            Controls.Add(cmbCategory);
            Controls.Add(lblCategory);
            Controls.Add(txtStock);
            Controls.Add(lblStock);
            Controls.Add(txtPrice);
            Controls.Add(lblPrice);
            Controls.Add(txtDescription);
            Controls.Add(lblDescription);
            Controls.Add(txtProductName);
            Controls.Add(lblProductName);
            Controls.Add(lblTitle);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void LoadCategories()
        {
            try
            {
                DataTable categories = _categoryRepo.GetActiveCategories();

                cmbCategory.DataSource = categories;
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryId";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategoriler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProduct()
        {
            try
            {
                DataRow row = _productRepo.GetProductById(_productId);

                if (row == null)
                {
                    MessageBox.Show("Ürün bulunamadı.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                txtProductName.Text = row["ProductName"]?.ToString();
                txtDescription.Text = row["Description"] == DBNull.Value ? "" : row["Description"].ToString();
                txtPrice.Text = row["Price"] != DBNull.Value
                    ? Convert.ToDecimal(row["Price"]).ToString("0.##", CultureInfo.InvariantCulture)
                    : "";
                txtStock.Text = row["Stock"] != DBNull.Value ? row["Stock"].ToString() : "";
                txtImageUrl.Text = row["ImageUrl"] == DBNull.Value ? "" : row["ImageUrl"].ToString();
                chkIsActive.Checked = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);

                if (row["CategoryId"] != DBNull.Value)
                    cmbCategory.SelectedValue = Convert.ToInt32(row["CategoryId"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün bilgisi yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string productName = txtProductName.Text.Trim();
                string description = txtDescription.Text.Trim();
                string imageUrl = txtImageUrl.Text.Trim();

                if (string.IsNullOrWhiteSpace(productName))
                {
                    MessageBox.Show("Ürün adı boş bırakılamaz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtProductName.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                {
                    if (!decimal.TryParse(txtPrice.Text.Trim(), out price))
                    {
                        MessageBox.Show("Geçerli bir fiyat giriniz.",
                            "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPrice.Focus();
                        return;
                    }
                }

                if (!int.TryParse(txtStock.Text.Trim(), out int stock))
                {
                    MessageBox.Show("Geçerli bir stok giriniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStock.Focus();
                    return;
                }

                if (stock < 0)
                {
                    MessageBox.Show("Stok negatif olamaz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStock.Focus();
                    return;
                }

                if (cmbCategory.SelectedValue == null)
                {
                    MessageBox.Show("Kategori seçiniz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbCategory.Focus();
                    return;
                }

                int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);

                if (_productId > 0)
                {
                    _productRepo.UpdateProduct(
                        _productId,
                        productName,
                        description,
                        price,
                        stock,
                        imageUrl,
                        chkIsActive.Checked,
                        categoryId);
                }
                else
                {
                    _productRepo.InsertProduct(
                        productName,
                        description,
                        price,
                        stock,
                        imageUrl,
                        chkIsActive.Checked,
                        categoryId);
                }

                IsSaved = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün kaydedilirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}