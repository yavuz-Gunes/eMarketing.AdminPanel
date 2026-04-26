using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;
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

        private Label lblImage;
        private TextBox txtImagePath;
        private Button btnBrowseImage;
        private PictureBox picPreview;

        private CheckBox chkIsActive;

        private Panel footerPanel;
        private Button btnCancel;
        private Button btnSave;

        private OpenFileDialog openFileDialog;
        private string selectedSourceImagePath = "";

        public bool IsSaved { get; private set; }

        public ProductModalForm(int productId = 0)
        {
            InitializeComponent();

            _productId = productId;

            BuildLayout();
            Load += ProductModalForm_Load;
        }

        private void ProductModalForm_Load(object sender, EventArgs e)
        {
            EnsureImageFolder();
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
            Width = 620;
            Height = 620;

            openFileDialog = new OpenFileDialog
            {
                Title = "Ürün Görseli Seç",
                Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp;*.webp",
                Multiselect = false
            };

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
                Width = 550,
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
                Width = 550,
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
                Width = 240,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            lblImage = new Label
            {
                Text = "Ürün Görseli",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(24, 306)
            };

            txtImagePath = new TextBox
            {
                Location = new Point(24, 330),
                Width = 420,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = true
            };

            btnBrowseImage = new Button
            {
                Text = "Resim Seç",
                Width = 130,
                Height = 32,
                Location = new Point(444, 328),
                FlatStyle = FlatStyle.Flat
            };
            btnBrowseImage.FlatAppearance.BorderColor = Color.Gainsboro;
            btnBrowseImage.Click += BtnBrowseImage_Click;

            picPreview = new PictureBox
            {
                Location = new Point(24, 376),
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            chkIsActive = new CheckBox
            {
                Text = "Ürün aktif olarak yayınlansın",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(24, 514),
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
                Location = new Point(364, 14),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderColor = Color.Gainsboro;
            btnCancel.Click += (s, e) => Close();

            btnSave = new Button
            {
                Text = "Kaydet",
                Width = 100,
                Height = 36,
                Location = new Point(474, 14),
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
            Controls.Add(picPreview);
            Controls.Add(btnBrowseImage);
            Controls.Add(txtImagePath);
            Controls.Add(lblImage);
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
                txtImagePath.Text = row["ImageUrl"] == DBNull.Value ? "" : row["ImageUrl"].ToString();
                chkIsActive.Checked = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);

                if (row["CategoryId"] != DBNull.Value)
                    cmbCategory.SelectedValue = Convert.ToInt32(row["CategoryId"]);

                LoadPreviewImage(txtImagePath.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün bilgisi yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            selectedSourceImagePath = openFileDialog.FileName;
            txtImagePath.Text = selectedSourceImagePath;
            LoadPreviewImage(selectedSourceImagePath);
        }

        private string GetRuntimeRootPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private string GetRuntimeImagesFolder()
        {
            return Path.Combine(GetRuntimeRootPath(), "Images", "Products");
        }

        private void EnsureImageFolder()
        {
            string imagesFolder = GetRuntimeImagesFolder();

            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);
        }

        private void LoadPreviewImage(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    picPreview.Image = null;
                    return;
                }

                string fullPath = path;

                if (!Path.IsPathRooted(fullPath))
                    fullPath = Path.Combine(GetRuntimeRootPath(), path);

                if (!File.Exists(fullPath))
                {
                    picPreview.Image = null;
                    return;
                }

                using (var temp = Image.FromFile(fullPath))
                {
                    picPreview.Image = new Bitmap(temp);
                }
            }
            catch
            {
                picPreview.Image = null;
            }
        }

        private string SaveSelectedImage()
        {
            if (string.IsNullOrWhiteSpace(selectedSourceImagePath))
                return txtImagePath.Text.Trim();

            string imagesFolder = GetRuntimeImagesFolder();

            if (!Directory.Exists(imagesFolder))
                Directory.CreateDirectory(imagesFolder);

            string extension = Path.GetExtension(selectedSourceImagePath);
            string fileName = Guid.NewGuid().ToString("N") + extension;
            string destinationPath = Path.Combine(imagesFolder, fileName);

            File.Copy(selectedSourceImagePath, destinationPath, true);

            return Path.Combine("Images", "Products", fileName);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string productName = txtProductName.Text.Trim();
                string description = txtDescription.Text.Trim();

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
                string savedImagePath = SaveSelectedImage();

                if (_productId > 0)
                {
                    _productRepo.UpdateProduct(
                        _productId,
                        productName,
                        description,
                        price,
                        stock,
                        savedImagePath,
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
                        savedImagePath,
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