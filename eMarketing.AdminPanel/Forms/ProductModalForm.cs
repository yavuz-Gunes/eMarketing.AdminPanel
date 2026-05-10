using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Forms
{
    public partial class ProductModalForm : Form
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();
        private readonly int _productId;

        private Panel headerPanel;
        private Panel bodyPanel;
        private Panel leftPanel;
        private Panel rightPanel;
        private Panel footerPanel;

        private Label lblTitle;
        private Label lblSubtitle;

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

        private Label lblImageTitle;
        private Label lblImageInfo;
        private TextBox txtImagePath;
        private Button btnBrowseImage;
        private Button btnClearImage;
        private PictureBox picPreview;

        private CheckBox chkIsActive;

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

        private async void ProductModalForm_Load(object sender, EventArgs e)
        {
            EnsureImageFolder();
            await LoadCategoriesAsync();

            if (_productId > 0)
                await LoadProductAsync();
        }

        private void BuildLayout()
        {
            Text = _productId > 0 ? "Ürün Düzenle" : "Yeni Ürün";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = AppColors.Background;
            Width = 760;
            Height = 640;

            openFileDialog = new OpenFileDialog
            {
                Title = "Ürün Görseli Seç",
                Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp;*.webp",
                Multiselect = false
            };

            BuildHeader();
            BuildBody();
            BuildFooter();

            Controls.Clear();
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
                Text = _productId > 0 ? "Ürün Düzenle" : "Yeni Ürün",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = false,
                Height = 34,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSubtitle = new Label
            {
                Text = _productId > 0
                    ? "Ürün bilgilerini, stok durumunu ve görselini güncelleyin."
                    : "Yeni oto yedek parça ürününü sisteme ekleyin.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                AutoSize = false,
                Height = 24,
                Dock = DockStyle.Top,
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

            leftPanel = new Panel
            {
                Location = new Point(24, 20),
                Size = new Size(440, 430),
                BackColor = AppColors.CardBackground,
                Padding = new Padding(20)
            };

            rightPanel = new Panel
            {
                Location = new Point(482, 20),
                Size = new Size(230, 430),
                BackColor = AppColors.CardBackground,
                Padding = new Padding(18)
            };

            BuildLeftForm();
            BuildRightImagePanel();

            bodyPanel.Controls.Add(leftPanel);
            bodyPanel.Controls.Add(rightPanel);
        }

        private void BuildLeftForm()
        {
            lblProductName = CreateLabel("Ürün Adı", 20, 18);
            txtProductName = CreateTextBox(20, 42, 390);
            txtProductName.MaxLength = 200;

            lblDescription = CreateLabel("Açıklama", 20, 84);
            txtDescription = new TextBox
            {
                Location = new Point(20, 108),
                Width = 390,
                Height = 82,
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                ForeColor = AppColors.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPrice = CreateLabel("Fiyat", 20, 204);
            txtPrice = CreateTextBox(20, 228, 120);

            lblStock = CreateLabel("Stok", 160, 204);
            txtStock = CreateTextBox(160, 228, 100);

            lblCategory = CreateLabel("Kategori", 280, 204);
            cmbCategory = new ComboBox
            {
                Location = new Point(280, 228),
                Width = 130,
                Height = 30,
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            chkIsActive = new CheckBox
            {
                Text = "Ürün aktif olarak yayınlansın",
                Location = new Point(20, 286),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent,
                Checked = true,
                Cursor = Cursors.Hand
            };

            Label hintLabel = new Label
            {
                Text = "Not: Stok 5 ve altına düştüğünde ürün kritik stok olarak görünür.",
                Location = new Point(20, 326),
                Size = new Size(390, 42),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            leftPanel.Controls.Add(lblProductName);
            leftPanel.Controls.Add(txtProductName);
            leftPanel.Controls.Add(lblDescription);
            leftPanel.Controls.Add(txtDescription);
            leftPanel.Controls.Add(lblPrice);
            leftPanel.Controls.Add(txtPrice);
            leftPanel.Controls.Add(lblStock);
            leftPanel.Controls.Add(txtStock);
            leftPanel.Controls.Add(lblCategory);
            leftPanel.Controls.Add(cmbCategory);
            leftPanel.Controls.Add(chkIsActive);
            leftPanel.Controls.Add(hintLabel);
        }

        private void BuildRightImagePanel()
        {
            lblImageTitle = new Label
            {
                Text = "Ürün Görseli",
                Location = new Point(18, 18),
                Size = new Size(190, 24),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            lblImageInfo = new Label
            {
                Text = "Ürün görseli seçerek listede önizleme alabilirsiniz.",
                Location = new Point(18, 44),
                Size = new Size(190, 42),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            picPreview = new PictureBox
            {
                Location = new Point(18, 98),
                Size = new Size(190, 160),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 253)
            };

            txtImagePath = new TextBox
            {
                Location = new Point(18, 274),
                Width = 190,
                Font = new Font("Segoe UI", 8.5F),
                ReadOnly = true,
                BackColor = Color.White,
                ForeColor = AppColors.TextSecondary,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnBrowseImage = new Button
            {
                Text = "Resim Seç",
                Location = new Point(18, 316),
                Width = 190,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnBrowseImage.FlatAppearance.BorderSize = 0;
            btnBrowseImage.Click += BtnBrowseImage_Click;

            btnClearImage = new Button
            {
                Text = "Görseli Kaldır",
                Location = new Point(18, 360),
                Width = 190,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AppColors.TextSecondary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnClearImage.FlatAppearance.BorderColor = AppColors.Border;
            btnClearImage.Click += BtnClearImage_Click;

            rightPanel.Controls.Add(lblImageTitle);
            rightPanel.Controls.Add(lblImageInfo);
            rightPanel.Controls.Add(picPreview);
            rightPanel.Controls.Add(txtImagePath);
            rightPanel.Controls.Add(btnBrowseImage);
            rightPanel.Controls.Add(btnClearImage);
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
                Text = _productId > 0 ? "Güncelle" : "Kaydet",
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

        private async Task LoadCategoriesAsync()
        {
            try
            {
                DataTable categories = await GetActiveCategoriesAsync();

                cmbCategory.DisplayMember = "KategoriAdi";
                cmbCategory.ValueMember = "KategoriId";
                cmbCategory.DataSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategoriler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadProductAsync()
        {
            try
            {
                DataRow row = await GetProductByIdAsync(_productId);

                if (row == null)
                {
                    MessageBox.Show("Ürün bulunamadı.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                txtProductName.Text = row["UrunAdi"]?.ToString();
                txtDescription.Text = row["Aciklama"] == DBNull.Value ? "" : row["Aciklama"]?.ToString();

                txtPrice.Text = row["Fiyat"] != DBNull.Value
                    ? Convert.ToDecimal(row["Fiyat"]).ToString("0.##", CultureInfo.InvariantCulture)
                    : "";

                txtStock.Text = row["Stok"] != DBNull.Value ? row["Stok"]?.ToString() : "";
                txtImagePath.Text = row["GorselUrl"] == DBNull.Value ? "" : row["GorselUrl"]?.ToString();
                chkIsActive.Checked = row["AktifMi"] != DBNull.Value && Convert.ToBoolean(row["AktifMi"]);

                if (row["KategoriId"] != DBNull.Value)
                    cmbCategory.SelectedValue = Convert.ToInt32(row["KategoriId"]);

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

        private void BtnClearImage_Click(object sender, EventArgs e)
        {
            selectedSourceImagePath = "";
            txtImagePath.Text = "";

            if (picPreview.Image != null)
            {
                picPreview.Image.Dispose();
                picPreview.Image = null;
            }
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
                if (picPreview.Image != null)
                {
                    picPreview.Image.Dispose();
                    picPreview.Image = null;
                }

                if (string.IsNullOrWhiteSpace(path))
                    return;

                string fullPath = path;

                if (!Path.IsPathRooted(fullPath))
                    fullPath = Path.Combine(GetRuntimeRootPath(), path);

                if (!File.Exists(fullPath))
                    return;

                using (Image temp = Image.FromFile(fullPath))
                {
                    picPreview.Image = new Bitmap(temp);
                }
            }
            catch
            {
                if (picPreview.Image != null)
                {
                    picPreview.Image.Dispose();
                    picPreview.Image = null;
                }
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

            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";

            string fileName = Guid.NewGuid().ToString("N") + extension;
            string destinationPath = Path.Combine(imagesFolder, fileName);

            File.Copy(selectedSourceImagePath, destinationPath, true);

            return Path.Combine("Images", "Products", fileName);
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string productName = txtProductName.Text.Trim();
                string description = txtDescription.Text.Trim();

                if (!ValidateForm(productName, out decimal price, out int stock, out int categoryId))
                    return;

                string savedImagePath = SaveSelectedImage();

                if (_productId > 0)
                {
                    await UpdateProductAsync(
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
                    await InsertProductAsync(
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

        private bool ValidateForm(string productName, out decimal price, out int stock, out int categoryId)
        {
            price = 0;
            stock = 0;
            categoryId = 0;

            if (string.IsNullOrWhiteSpace(productName))
            {
                MessageBox.Show("Ürün adı boş bırakılamaz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return false;
            }

            if (!IsValidProductName(productName))
            {
                MessageBox.Show("Ürün adı yalnızca harf, rakam, boşluk ve izin verilen karakterleri içerebilir.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return false;
            }

            if (!TryParsePrice(txtPrice.Text.Trim(), out price))
            {
                MessageBox.Show("Geçerli bir fiyat giriniz. Örnek: 1250,50",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            if (price <= 0)
            {
                MessageBox.Show("Fiyat sıfırdan büyük olmalıdır.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }

            if (!int.TryParse(txtStock.Text.Trim(), out stock))
            {
                MessageBox.Show("Geçerli bir stok giriniz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return false;
            }

            if (stock < 0)
            {
                MessageBox.Show("Stok negatif olamaz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return false;
            }

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Kategori seçiniz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCategory.Focus();
                return false;
            }

            categoryId = Convert.ToInt32(cmbCategory.SelectedValue);
            return true;
        }

        private bool TryParsePrice(string text, out decimal price)
        {
            if (decimal.TryParse(text, NumberStyles.Any, new CultureInfo("tr-TR"), out price))
                return true;

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                return true;

            return decimal.TryParse(text, out price);
        }

        private bool IsValidProductName(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return false;

            foreach (char c in productName)
            {
                if (!char.IsLetterOrDigit(c)
                    && !char.IsWhiteSpace(c)
                    && c != '-'
                    && c != '('
                    && c != ')'
                    && c != '&'
                    && c != ','
                    && c != '.'
                    && c != '/'
                    && c != '+')
                {
                    return false;
                }
            }

            return true;
        }

        private Task<DataTable> GetActiveCategoriesAsync()
        {
            return _apiClient.GetCategoriesAsync("", 1);
        }

        private Task<DataRow> GetProductByIdAsync(int productId)
        {
            return _apiClient.GetProductByIdAsync(productId);
        }

        private Task InsertProductAsync(string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            return _apiClient.InsertProductAsync(name, description, price, stock, imageUrl, isActive, categoryId);
        }

        private Task UpdateProductAsync(int id, string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            return _apiClient.UpdateProductAsync(id, name, description, price, stock, imageUrl, isActive, categoryId);
        }
    }
}
