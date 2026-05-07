using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Pages
{
    public class ProductsPage : UserControl, IThemeable
    {
        private readonly ProductRepository _repo = new ProductRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private Panel gridPanel;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblInfo;

        private Button btnNewProduct;
        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private ComboBox cmbCategory;
        private CheckBox chkLowStockOnly;
        private Button btnSearch;
        private Button btnClear;

        private DataGridView dgvProducts;

        private CategoriesCard cTotal;
        private CategoriesCard cActive;
        private CategoriesCard cPassive;
        private CategoriesCard cLowStock;

        private Timer searchTimer;

        private int hoveredRowIndex = -1;
        private int hoveredColumnIndex = -1;

        public ProductsPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            searchTimer = new Timer
            {
                Interval = 350
            };
            searchTimer.Tick += SearchTimer_Tick;

            BuildLayout();
            Load += ProductsPage_Load;
        }

        private void ProductsPage_Load(object sender, EventArgs e)
        {
            LoadCategoryFilter();
            LoadProducts();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            BuildHeaderPanel();
            BuildStatsPanel();
            BuildFilterPanel();
            BuildGridPanel();

            Controls.Add(gridPanel);
            Controls.Add(filterPanel);
            Controls.Add(statsPanel);
            Controls.Add(headerPanel);

            ResumeLayout(true);
        }

        private void BuildHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = "Ürünler",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            lblSubtitle = new Label
            {
                Text = "Oto yedek parça ürünlerini yönetin, stok durumlarını takip edin.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 38)
            };

            btnNewProduct = new Button
            {
                Text = "+ Yeni Ürün",
                Width = 150,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnNewProduct.FlatAppearance.BorderSize = 0;
            btnNewProduct.Click += BtnNewProduct_Click;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(btnNewProduct);

            headerPanel.Resize += (s, e) =>
            {
                btnNewProduct.Location = new Point(headerPanel.Width - btnNewProduct.Width, 6);
            };
        }

        private void BuildStatsPanel()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 124,
                BackColor = AppColors.Background
            };

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cTotal = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cActive = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cPassive = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cLowStock = new CategoriesCard { Dock = DockStyle.Fill, Margin = Padding.Empty };

            cTotal.SetData("📦", "Toplam", "0");
            cActive.SetData("✅", "Aktif", "0");
            cPassive.SetData("⛔", "Pasif", "0");
            cLowStock.SetData("⚠", "Kritik Stok", "0");

            grid.Controls.Add(cTotal, 0, 0);
            grid.Controls.Add(cActive, 1, 0);
            grid.Controls.Add(cPassive, 2, 0);
            grid.Controls.Add(cLowStock, 3, 0);

            statsPanel.Controls.Add(grid);
        }

        private void BuildFilterPanel()
        {
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 74,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(16, 14, 16, 14)
            };

            txtSearch = new TextBox
            {
                Width = 260,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 20)
            };

            cmbStatus = new ComboBox
            {
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            cmbStatus.Items.Add("Aktif");
            cmbStatus.Items.Add("Pasif");
            cmbStatus.Items.Add("Hepsi");
            cmbStatus.SelectedIndex = 0;

            cmbCategory = new ComboBox
            {
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            chkLowStockOnly = new CheckBox
            {
                Text = "Kritik stok",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            btnSearch = new Button
            {
                Text = "Ara",
                Width = 88,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnSearch.FlatAppearance.BorderSize = 0;

            btnClear = new Button
            {
                Text = "Temizle",
                Width = 92,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AppColors.TextSecondary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnClear.FlatAppearance.BorderColor = AppColors.Border;

            lblInfo = new Label
            {
                Text = "0 kayıt",
                Width = 180,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            txtSearch.KeyDown += TxtSearch_KeyDown;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            chkLowStockOnly.CheckedChanged += ChkLowStockOnly_CheckedChanged;
            btnSearch.Click += BtnSearch_Click;
            btnClear.Click += BtnClear_Click;

            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(cmbCategory);
            filterPanel.Controls.Add(chkLowStockOnly);
            filterPanel.Controls.Add(btnSearch);
            filterPanel.Controls.Add(btnClear);
            filterPanel.Controls.Add(lblInfo);

            filterPanel.Resize += (s, e) => PlaceFilterControls();
            PlaceFilterControls();
        }

        private void PlaceFilterControls()
        {
            if (filterPanel == null)
                return;

            int x = 16;
            int y = 20;

            txtSearch.Location = new Point(x, y);
            x += txtSearch.Width + 14;

            cmbStatus.Location = new Point(x, y);
            x += cmbStatus.Width + 14;

            cmbCategory.Location = new Point(x, y);
            x += cmbCategory.Width + 14;

            chkLowStockOnly.Location = new Point(x, y + 6);
            x += chkLowStockOnly.Width + 14;

            btnSearch.Location = new Point(x, y - 2);
            x += btnSearch.Width + 10;

            btnClear.Location = new Point(x, y - 2);

            lblInfo.Location = new Point(filterPanel.Width - lblInfo.Width - 16, y);
            lblInfo.Visible = lblInfo.Left > btnClear.Right + 20;
        }

        private void BuildGridPanel()
        {
            gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(12)
            };

            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = AppColors.CardBackground,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
            };

            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersHeight = 42;
            dgvProducts.RowTemplate.Height = 50;
            dgvProducts.GridColor = AppColors.Border;
            dgvProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProducts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.WhiteSmoke;

            dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvProducts.DefaultCellStyle.BackColor = Color.White;
            dgvProducts.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            dgvProducts.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);

            ConfigureGridColumns();

            dgvProducts.CellFormatting += DgvProducts_CellFormatting;
            dgvProducts.CellContentClick += DgvProducts_CellContentClick;
            dgvProducts.CellPainting += DgvProducts_CellPainting;
            dgvProducts.CellMouseMove += DgvProducts_CellMouseMove;
            dgvProducts.CellDoubleClick += DgvProducts_CellDoubleClick;
            dgvProducts.MouseLeave += DgvProducts_MouseLeave;

            gridPanel.Controls.Add(dgvProducts);
        }

        private void ConfigureGridColumns()
        {
            dgvProducts.Columns.Clear();

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UrunId",
                DataPropertyName = "UrunId",
                Visible = false
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GorselUrlHidden",
                DataPropertyName = "GorselUrl",
                Visible = false
            });

            dgvProducts.Columns.Add(new DataGridViewImageColumn
            {
                Name = "UrunGorsel",
                DataPropertyName = "UrunGorsel",
                HeaderText = "Görsel",
                Width = 70,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UrunAdi",
                DataPropertyName = "UrunAdi",
                HeaderText = "Ürün Adı",
                Width = 170
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Aciklama",
                DataPropertyName = "Aciklama",
                HeaderText = "Açıklama",
                Width = 190
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Fiyat",
                DataPropertyName = "Fiyat",
                HeaderText = "Fiyat",
                Width = 95,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Stok",
                DataPropertyName = "Stok",
                HeaderText = "Stok",
                Width = 65,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "KategoriAdi",
                DataPropertyName = "KategoriAdi",
                HeaderText = "Kategori",
                Width = 130
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AktifMi",
                DataPropertyName = "AktifMi",
                HeaderText = "Durum",
                Width = 90
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StokDurumu",
                DataPropertyName = "StokDurumu",
                HeaderText = "Stok Durumu",
                Width = 105
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OlusturmaTarihi",
                DataPropertyName = "OlusturmaTarihi",
                HeaderText = "Oluşturulma",
                Width = 125,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "dd.MM.yyyy"
                }
            });

            dgvProducts.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colEdit",
                HeaderText = "",
                Text = "Düzenle",
                UseColumnTextForButtonValue = true,
                Width = 92
            });

            dgvProducts.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colDelete",
                HeaderText = "",
                Text = "Sil",
                UseColumnTextForButtonValue = true,
                Width = 98
            });

            ApplyGridColumnSizing();
        }

        private void ApplyGridColumnSizing()
        {
            SetFill("UrunGorsel", 7, 48);
            SetFill("UrunAdi", 20, 120);
            SetFill("Aciklama", 18, 110);
            SetFill("Fiyat", 9, 78);
            SetFill("Stok", 6, 55);
            SetFill("KategoriAdi", 12, 90);
            SetFill("AktifMi", 8, 72);
            SetFill("StokDurumu", 9, 82);
            SetFill("OlusturmaTarihi", 10, 88);
            SetFill("colEdit", 8, 72);
            SetFill("colDelete", 8, 72);
        }

        private void SetFill(string columnName, float fillWeight, int minWidth)
        {
            if (!dgvProducts.Columns.Contains(columnName))
                return;

            DataGridViewColumn column = dgvProducts.Columns[columnName];
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.FillWeight = fillWeight;
            column.MinimumWidth = minWidth;
        }

        private void LoadCategoryFilter()
        {
            try
            {
                cmbCategory.SelectedIndexChanged -= CmbCategory_SelectedIndexChanged;

                DataTable categories = _categoryRepo.GetActiveCategories();

                DataTable source = new DataTable();
                source.Columns.Add("KategoriId", typeof(int));
                source.Columns.Add("KategoriAdi", typeof(string));

                source.Rows.Add(0, "Tüm Kategoriler");

                foreach (DataRow row in categories.Rows)
                {
                    source.Rows.Add(
                        Convert.ToInt32(row["KategoriId"]),
                        row["KategoriAdi"]?.ToString()
                    );
                }

                cmbCategory.DisplayMember = "KategoriAdi";
                cmbCategory.ValueMember = "KategoriId";
                cmbCategory.DataSource = source;
                cmbCategory.SelectedValue = 0;

                cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategori filtresi yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                int categoryId = 0;

                if (cmbCategory.SelectedValue != null && cmbCategory.SelectedValue is int)
                    categoryId = (int)cmbCategory.SelectedValue;

                DataTable table = _repo.GetProducts(txtSearch.Text.Trim(), GetSelectedStatus(), categoryId);

                PrepareProductTable(table);

                DataTable displayTable = chkLowStockOnly.Checked
                    ? FilterCriticalStockProducts(table)
                    : table;

                dgvProducts.DataSource = displayTable;

                UpdateStats(table);
                UpdateInfoLabel(displayTable.Rows.Count, table.Rows.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrepareProductTable(DataTable table)
        {
            if (!table.Columns.Contains("StokDurumu"))
                table.Columns.Add("StokDurumu", typeof(string));

            if (!table.Columns.Contains("UrunGorsel"))
                table.Columns.Add("UrunGorsel", typeof(Image));

            foreach (DataRow row in table.Rows)
            {
                int stock = row["Stok"] != DBNull.Value ? Convert.ToInt32(row["Stok"]) : 0;

                if (stock <= 0)
                    row["StokDurumu"] = "Tükendi";
                else if (stock <= 5)
                    row["StokDurumu"] = "Kritik";
                else
                    row["StokDurumu"] = "Yeterli";

                row["UrunGorsel"] = LoadProductThumbnail(
                    row["GorselUrl"] == DBNull.Value ? "" : row["GorselUrl"]?.ToString()
                );
            }
        }

        private DataTable FilterCriticalStockProducts(DataTable table)
        {
            DataTable filtered = table.Clone();

            foreach (DataRow row in table.Rows)
            {
                int stock = row["Stok"] != DBNull.Value ? Convert.ToInt32(row["Stok"]) : 0;

                if (stock >= 1 && stock <= 5)
                    filtered.ImportRow(row);
            }

            return filtered;
        }

        private void UpdateInfoLabel(int displayCount, int totalCount)
        {
            if (lblInfo == null)
                return;

            if (chkLowStockOnly.Checked)
                lblInfo.Text = displayCount + " kritik kayıt";
            else
                lblInfo.Text = displayCount + " kayıt";
        }

        private string GetRuntimeRootPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private Image LoadProductThumbnail(string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                    return null;

                string fullPath = imagePath;

                if (!Path.IsPathRooted(fullPath))
                    fullPath = Path.Combine(GetRuntimeRootPath(), imagePath);

                if (!File.Exists(fullPath))
                    return null;

                using (Image tempImage = Image.FromFile(fullPath))
                {
                    return new Bitmap(tempImage, new Size(40, 40));
                }
            }
            catch
            {
                return null;
            }
        }

        private void UpdateStats(DataTable currentTable)
        {
            try
            {
                DataTable allTable = _repo.GetProducts("", -1, 0);

                int totalCount = allTable.Rows.Count;
                int activeCount = 0;
                int passiveCount = 0;
                int lowStockCount = 0;

                foreach (DataRow row in allTable.Rows)
                {
                    bool isActive = row["AktifMi"] != DBNull.Value && Convert.ToBoolean(row["AktifMi"]);
                    int stock = row["Stok"] != DBNull.Value ? Convert.ToInt32(row["Stok"]) : 0;

                    if (isActive)
                        activeCount++;
                    else
                        passiveCount++;

                    if (stock >= 1 && stock <= 5)
                        lowStockCount++;
                }

                cTotal.SetData("📦", "Toplam", totalCount.ToString());
                cActive.SetData("✅", "Aktif", activeCount.ToString());
                cPassive.SetData("⛔", "Pasif", passiveCount.ToString());
                cLowStock.SetData("⚠", "Kritik Stok", lowStockCount.ToString());
            }
            catch
            {
                cTotal.SetData("📦", "Toplam", "0");
                cActive.SetData("✅", "Aktif", "0");
                cPassive.SetData("⛔", "Pasif", "0");
                cLowStock.SetData("⚠", "Kritik Stok", "0");
            }
        }

        private int GetSelectedStatus()
        {
            switch (cmbStatus.SelectedIndex)
            {
                case 0:
                    return 1;

                case 1:
                    return 0;

                default:
                    return -1;
            }
        }

        private bool GetRowActiveStatus(int rowIndex)
        {
            bool isActive = false;
            object activeValue = dgvProducts.Rows[rowIndex].Cells["AktifMi"].Value;

            if (activeValue != null && activeValue != DBNull.Value)
            {
                try
                {
                    isActive = Convert.ToBoolean(activeValue);
                }
                catch
                {
                    string activeText = activeValue.ToString();
                    isActive = activeText == "Aktif" || activeText == "True" || activeText == "true";
                }
            }

            return isActive;
        }

        private int GetRowProductId(int rowIndex)
        {
            return Convert.ToInt32(dgvProducts.Rows[rowIndex].Cells["UrunId"].Value);
        }

        private void OpenProductEditForm(int productId)
        {
            using (ProductModalForm frm = new ProductModalForm(productId))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                    LoadProducts();
            }
        }

        private void BtnNewProduct_Click(object sender, EventArgs e)
        {
            using (ProductModalForm frm = new ProductModalForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                    LoadProducts();
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            searchTimer.Stop();
            LoadProducts();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            searchTimer.Stop();

            txtSearch.Clear();
            cmbStatus.SelectedIndex = 0;

            if (cmbCategory.DataSource != null)
                cmbCategory.SelectedValue = 0;

            chkLowStockOnly.Checked = false;

            LoadProducts();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                searchTimer.Stop();
                LoadProducts();
                e.SuppressKeyPress = true;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            LoadProducts();
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void CmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCategory.DataSource == null)
                return;

            if (cmbCategory.SelectedValue == null)
                return;

            if (!(cmbCategory.SelectedValue is int))
                return;

            LoadProducts();
        }

        private void ChkLowStockOnly_CheckedChanged(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void DgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvProducts.Columns[e.ColumnIndex].Name;

            if (columnName == "Aciklama" && e.Value != null)
            {
                string text = e.Value.ToString();

                if (text.Length > 28)
                {
                    e.Value = text.Substring(0, 28) + "...";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "KategoriAdi" && e.Value != null)
            {
                string text = e.Value.ToString();

                if (text.Length > 18)
                {
                    e.Value = text.Substring(0, 18) + "...";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "Fiyat" && e.Value != null && e.Value != DBNull.Value)
            {
                decimal price;

                if (decimal.TryParse(e.Value.ToString(), out price))
                {
                    e.Value = price.ToString("N2") + " ₺";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "Stok")
            {
                int stock = 0;

                if (e.Value != null && e.Value != DBNull.Value)
                    int.TryParse(e.Value.ToString(), out stock);

                if (stock <= 0)
                    e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                else if (stock <= 5)
                    e.CellStyle.ForeColor = Color.FromArgb(194, 65, 12);
                else
                    e.CellStyle.ForeColor = AppColors.TextPrimary;

                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }
        }

        private void DgvProducts_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            int newRow = -1;
            int newCol = -1;

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string columnName = dgvProducts.Columns[e.ColumnIndex].Name;

                if (columnName == "colEdit" || columnName == "colDelete")
                {
                    newRow = e.RowIndex;
                    newCol = e.ColumnIndex;
                }
            }

            if (newRow != hoveredRowIndex || newCol != hoveredColumnIndex)
            {
                int oldRow = hoveredRowIndex;
                int oldCol = hoveredColumnIndex;

                hoveredRowIndex = newRow;
                hoveredColumnIndex = newCol;

                if (oldRow >= 0 && oldCol >= 0)
                    dgvProducts.InvalidateCell(oldCol, oldRow);

                if (hoveredRowIndex >= 0 && hoveredColumnIndex >= 0)
                    dgvProducts.InvalidateCell(hoveredColumnIndex, hoveredRowIndex);
            }

        }

        private void DgvProducts_MouseLeave(object sender, EventArgs e)
        {
            int oldRow = hoveredRowIndex;
            int oldCol = hoveredColumnIndex;

            hoveredRowIndex = -1;
            hoveredColumnIndex = -1;

            if (oldRow >= 0 && oldCol >= 0)
                dgvProducts.InvalidateCell(oldCol, oldRow);

        }

        private void DgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvProducts.Columns[e.ColumnIndex].Name;

            if (columnName == "colEdit" || columnName == "colDelete")
                return;

            int productId = GetRowProductId(e.RowIndex);
            OpenProductEditForm(productId);
        }

        private void DgvProducts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvProducts.Columns[e.ColumnIndex].Name;

            if (columnName == "AktifMi")
            {
                e.PaintBackground(e.CellBounds, true);
                e.Handled = true;

                bool isActive = false;

                if (e.Value != null && e.Value != DBNull.Value)
                    isActive = Convert.ToBoolean(e.Value);

                string text = isActive ? "Aktif" : "Pasif";

                Color backColor = isActive
                    ? Color.FromArgb(232, 245, 233)
                    : Color.FromArgb(245, 245, 245);

                Color foreColor = isActive
                    ? Color.FromArgb(46, 125, 50)
                    : Color.FromArgb(97, 97, 97);

                Rectangle badgeRect = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - 72) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                    72,
                    24
                );

                using (SolidBrush brush = new SolidBrush(backColor))
                using (SolidBrush textBrush = new SolidBrush(foreColor))
                using (StringFormat sf = new StringFormat())
                using (Font font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.FillRectangle(brush, badgeRect);
                    e.Graphics.DrawString(text, font, textBrush, badgeRect, sf);
                }

                return;
            }

            if (columnName == "StokDurumu")
            {
                e.PaintBackground(e.CellBounds, true);
                e.Handled = true;

                string text = e.Value?.ToString() ?? "";

                Color backColor = Color.FromArgb(243, 244, 246);
                Color foreColor = Color.FromArgb(75, 85, 99);

                if (text == "Tükendi")
                {
                    backColor = Color.FromArgb(254, 242, 242);
                    foreColor = Color.FromArgb(185, 28, 28);
                }
                else if (text == "Kritik")
                {
                    backColor = Color.FromArgb(255, 247, 237);
                    foreColor = Color.FromArgb(194, 65, 12);
                }
                else if (text == "Yeterli")
                {
                    backColor = Color.FromArgb(236, 253, 245);
                    foreColor = Color.FromArgb(21, 128, 61);
                }

                Rectangle badgeRect = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - 82) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                    82,
                    24
                );

                using (SolidBrush brush = new SolidBrush(backColor))
                using (SolidBrush textBrush = new SolidBrush(foreColor))
                using (StringFormat sf = new StringFormat())
                using (Font font = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.FillRectangle(brush, badgeRect);
                    e.Graphics.DrawString(text, font, textBrush, badgeRect, sf);
                }

                return;
            }

            if (columnName == "colEdit" || columnName == "colDelete")
            {
                e.PaintBackground(e.CellBounds, true);
                e.Handled = true;

                bool isEdit = columnName == "colEdit";
                bool isHovered = e.RowIndex == hoveredRowIndex && e.ColumnIndex == hoveredColumnIndex;
                bool rowIsActive = GetRowActiveStatus(e.RowIndex);

                string text;
                Color baseColor;

                if (isEdit)
                {
                    text = rowIsActive ? "Düzenle" : "Aktifleştir";
                    baseColor = rowIsActive
                        ? Color.FromArgb(66, 133, 244)
                        : Color.FromArgb(25, 135, 84);
                }
                else
                {
                    text = rowIsActive ? "Pasife Al" : "Sil";
                    baseColor = rowIsActive
                        ? Color.FromArgb(220, 53, 69)
                        : Color.FromArgb(108, 117, 125);
                }

                Color fillColor = isHovered ? baseColor : Color.White;
                Color borderColor = baseColor;
                Color textColor = isHovered ? Color.White : baseColor;

                Rectangle buttonRect = new Rectangle(
                    e.CellBounds.X + 6,
                    e.CellBounds.Y + 7,
                    e.CellBounds.Width - 12,
                    e.CellBounds.Height - 14
                );

                using (SolidBrush fillBrush = new SolidBrush(fillColor))
                using (Pen borderPen = new Pen(borderColor))
                using (SolidBrush textBrush = new SolidBrush(textColor))
                using (StringFormat sf = new StringFormat())
                using (Font font = new Font("Segoe UI", 8.2F, FontStyle.Bold))
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.FillRectangle(fillBrush, buttonRect);
                    e.Graphics.DrawRectangle(borderPen, buttonRect);
                    e.Graphics.DrawString(text, font, textBrush, buttonRect, sf);
                }

                return;
            }
        }

        private void DgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvProducts.Columns[e.ColumnIndex].Name;
            int productId = GetRowProductId(e.RowIndex);
            bool isActive = GetRowActiveStatus(e.RowIndex);

            if (columnName == "colEdit")
            {
                if (isActive)
                {
                    OpenProductEditForm(productId);
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        "Bu ürünü tekrar aktifleştirmek istiyor musunuz?",
                        "Onay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repo.SetProductActiveStatus(productId, true);
                        LoadProducts();
                    }
                }
            }
            else if (columnName == "colDelete")
            {
                if (isActive)
                {
                    DialogResult result = MessageBox.Show(
                        "Bu ürünü pasife çekmek istiyor musunuz?",
                        "Onay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _repo.SetProductActiveStatus(productId, false);
                        LoadProducts();
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        "Bu pasif ürünü kalıcı olarak silmek istiyor musunuz?",
                        "Kalıcı Silme",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        if (_repo.DeleteProduct(productId, out string message))
                        {
                            LoadProducts();
                        }
                        else
                        {
                            MessageBox.Show(message,
                                "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;

            if (headerPanel != null)
                headerPanel.BackColor = AppColors.Background;

            if (statsPanel != null)
                statsPanel.BackColor = AppColors.Background;

            if (filterPanel != null)
                filterPanel.BackColor = AppColors.CardBackground;

            if (gridPanel != null)
                gridPanel.BackColor = AppColors.CardBackground;

            if (lblTitle != null)
                lblTitle.ForeColor = AppColors.TextPrimary;

            if (lblSubtitle != null)
                lblSubtitle.ForeColor = AppColors.TextSecondary;

            if (lblInfo != null)
                lblInfo.ForeColor = AppColors.TextSecondary;

            if (chkLowStockOnly != null)
            {
                chkLowStockOnly.ForeColor = AppColors.TextSecondary;
                chkLowStockOnly.BackColor = Color.Transparent;
            }

            if (btnNewProduct != null)
            {
                btnNewProduct.BackColor = AppColors.Primary;
                btnNewProduct.ForeColor = Color.White;
            }

            if (btnSearch != null)
            {
                btnSearch.BackColor = AppColors.Primary;
                btnSearch.ForeColor = Color.White;
            }

            if (btnClear != null)
            {
                btnClear.BackColor = AppColors.CardBackground;
                btnClear.ForeColor = AppColors.TextSecondary;
                btnClear.FlatAppearance.BorderColor = AppColors.Border;
            }

            if (dgvProducts != null)
            {
                dgvProducts.BackgroundColor = AppColors.CardBackground;
                dgvProducts.GridColor = AppColors.Border;
                dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvProducts.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvProducts.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }

            Invalidate(true);
            Refresh();
        }
    }
}
