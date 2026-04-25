using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.DataAccess;
using eMarketing.AdminPanel.Forms;

namespace eMarketing.AdminPanel.Pages
{
    public class ProductsPage : UserControl
    {
        private readonly ProductRepository _repo = new ProductRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private Panel gridPanel;

        private Label lblTitle;
        private Label lblSubtitle;

        private Button btnNewProduct;
        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private ComboBox cmbCategory;
        private Button btnSearch;

        private DataGridView dgvProducts;

        private CategoriesCard cTotal;
        private CategoriesCard cActive;
        private CategoriesCard cPassive;
        private CategoriesCard cLowStock;

        private Timer searchTimer;

        private int hoveredRowIndex = -1;
        private int hoveredColumnIndex = -1;

        private Panel imagePreviewPanel;
        private PictureBox imagePreviewBox;

        public ProductsPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            searchTimer = new Timer();
            searchTimer.Interval = 350;
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
            BuildHeaderPanel();
            BuildStatsPanel();
            BuildFilterPanel();
            BuildGridPanel();
            BuildImagePreview();

            Controls.Add(gridPanel);
            Controls.Add(filterPanel);
            Controls.Add(statsPanel);
            Controls.Add(headerPanel);

        }

        private void BuildHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = "Ürünler",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            lblSubtitle = new Label
            {
                Text = "Ürün kayıtlarını yönetin.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 34)
            };

            btnNewProduct = new Button
            {
                Text = "+ Yeni Ürün",
                Width = 150,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White
            };
            btnNewProduct.FlatAppearance.BorderSize = 0;
            btnNewProduct.Click += BtnNewProduct_Click;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(btnNewProduct);

            headerPanel.Resize += (s, e) =>
            {
                btnNewProduct.Location = new Point(headerPanel.Width - btnNewProduct.Width, 4);
            };
        }

        private void BuildStatsPanel()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = AppColors.Background
            };

            var grid = new TableLayoutPanel
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
                Height = 64,
                BackColor = Color.White,
                Padding = new Padding(16, 14, 16, 14)
            };

            txtSearch = new TextBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 16)
            };

            cmbStatus = new ComboBox
            {
                Width = 130,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(282, 16)
            };
            cmbStatus.Items.Add("Aktif");
            cmbStatus.Items.Add("Pasif");
            cmbStatus.Items.Add("Hepsi");
            cmbStatus.SelectedIndex = 0;

            cmbCategory = new ComboBox
            {
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(428, 16)
            };

            btnSearch = new Button
            {
                Text = "Ara",
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(624, 15)
            };
            btnSearch.FlatAppearance.BorderColor = Color.Gainsboro;

            txtSearch.KeyDown += TxtSearch_KeyDown;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            cmbCategory.SelectedIndexChanged += CmbCategory_SelectedIndexChanged;
            btnSearch.Click += BtnSearch_Click;

            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(cmbCategory);
            filterPanel.Controls.Add(btnSearch);
        }

        private void BuildGridPanel()
        {
            gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersHeight = 42;
            dgvProducts.RowTemplate.Height = 48;
            dgvProducts.GridColor = Color.Gainsboro;
            dgvProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProducts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvProducts.DefaultCellStyle.BackColor = Color.White;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            dgvProducts.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            ConfigureGridColumns();

            ConfigureGridColumns();

            dgvProducts.CellFormatting += DgvProducts_CellFormatting;
            dgvProducts.CellContentClick += DgvProducts_CellContentClick;
            dgvProducts.CellPainting += DgvProducts_CellPainting;
            dgvProducts.MouseLeave += DgvProducts_MouseLeave;
            dgvProducts.CellMouseMove += DgvProducts_CellMouseMove;
            

            gridPanel.Controls.Add(dgvProducts);

            gridPanel.Controls.Add(dgvProducts);
        }
        private void ShowImagePreview(string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    HideImagePreview();
                    return;
                }

                string fullPath = imagePath;

                if (!Path.IsPathRooted(fullPath))
                    fullPath = Path.Combine(GetRuntimeRootPath(), imagePath);

                if (!File.Exists(fullPath))
                {
                    HideImagePreview();
                    return;
                }

                using (var temp = Image.FromFile(fullPath))
                {
                    imagePreviewBox.Image?.Dispose();
                    imagePreviewBox.Image = new Bitmap(temp);
                }

                // ÖNEMLİ: konum gridPanel'e göre hesaplanacak
                Point clientPoint = gridPanel.PointToClient(Cursor.Position);

                int x = clientPoint.X + 20;
                int y = clientPoint.Y + 20;

                if (x + imagePreviewPanel.Width > gridPanel.Width)
                    x = gridPanel.Width - imagePreviewPanel.Width - 10;

                if (y + imagePreviewPanel.Height > gridPanel.Height)
                    y = gridPanel.Height - imagePreviewPanel.Height - 10;

                if (x < 0) x = 10;
                if (y < 0) y = 10;

                imagePreviewPanel.Location = new Point(x, y);
                imagePreviewPanel.Visible = true;
                imagePreviewPanel.BringToFront();
            }
            catch
            {
                HideImagePreview();
            }
        }

        private void HideImagePreview()
        {
            if (imagePreviewPanel != null)
                imagePreviewPanel.Visible = false;

            if (imagePreviewBox != null && imagePreviewBox.Image != null)
            {
                imagePreviewBox.Image.Dispose();
                imagePreviewBox.Image = null;
            }
        }
        private void BuildImagePreview()
        {
            imagePreviewPanel = new Panel
            {
                Size = new Size(220, 220),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false
            };

            imagePreviewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            imagePreviewPanel.Controls.Add(imagePreviewBox);

            // ÖNEMLİ: preview ana control'e değil gridPanel içine eklensin
            gridPanel.Controls.Add(imagePreviewPanel);
            imagePreviewPanel.BringToFront();
        }


        private void ConfigureGridColumns()
        {
            dgvProducts.Columns.Clear();

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductId",
                DataPropertyName = "ProductId",
                Visible = false
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ImageUrlHidden",
                DataPropertyName = "ImageUrl",
                Visible = false
            });

            dgvProducts.Columns.Add(new DataGridViewImageColumn
            {
                Name = "ProductImage",
                DataPropertyName = "ProductImage",
                HeaderText = "Görsel",
                Width = 70,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductName",
                DataPropertyName = "ProductName",
                HeaderText = "Ürün Adı",
                Width = 150
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                DataPropertyName = "Description",
                HeaderText = "Açıklama",
                Width = 170
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Price",
                DataPropertyName = "Price",
                HeaderText = "Fiyat",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N2"
                }
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Stock",
                DataPropertyName = "Stock",
                HeaderText = "Stok",
                Width = 60
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                DataPropertyName = "Category",
                HeaderText = "Kategori",
                Width = 120
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IsActive",
                DataPropertyName = "IsActive",
                HeaderText = "Durum",
                Width = 80
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockStatus",
                DataPropertyName = "StockStatus",
                HeaderText = "Stok Durumu",
                Width = 95
            });

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CreatedDate",
                DataPropertyName = "CreatedDate",
                HeaderText = "Oluşturulma",
                Width = 120,
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
                Width = 85
            });

            dgvProducts.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colDelete",
                HeaderText = "",
                Text = "Sil",
                UseColumnTextForButtonValue = true,
                Width = 70
            });
        }

        private void LoadCategoryFilter()
        {
            try
            {
                DataTable categories = _categoryRepo.GetActiveCategories();

                DataTable source = new DataTable();
                source.Columns.Add("CategoryId", typeof(int));
                source.Columns.Add("CategoryName", typeof(string));

                source.Rows.Add(0, "Tüm Kategoriler");

                foreach (DataRow row in categories.Rows)
                {
                    source.Rows.Add(
                        Convert.ToInt32(row["CategoryId"]),
                        row["CategoryName"].ToString()
                    );
                }

                cmbCategory.DataSource = source;
                cmbCategory.DisplayMember = "CategoryName";
                cmbCategory.ValueMember = "CategoryId";
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

                if (!table.Columns.Contains("StockStatus"))
                    table.Columns.Add("StockStatus", typeof(string));

                if (!table.Columns.Contains("ProductImage"))
                    table.Columns.Add("ProductImage", typeof(Image));

                foreach (DataRow row in table.Rows)
                {
                    int stock = row["Stock"] != DBNull.Value ? Convert.ToInt32(row["Stock"]) : 0;

                    if (stock <= 0)
                        row["StockStatus"] = "Tükendi";
                    else if (stock <= 5)
                        row["StockStatus"] = "Kritik";
                    else
                        row["StockStatus"] = "Yeterli";

                    row["ProductImage"] = LoadProductThumbnail(
                        row["ImageUrl"] == DBNull.Value ? "" : row["ImageUrl"].ToString()
                    );
                }

                dgvProducts.DataSource = table;
                UpdateStats(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

                using (var tempImage = Image.FromFile(fullPath))
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
                    bool isActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
                    int stock = row["Stock"] != DBNull.Value ? Convert.ToInt32(row["Stock"]) : 0;

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
                case 0: return 1;
                case 1: return 0;
                default: return -1;
            }
        }

        private void BtnNewProduct_Click(object sender, EventArgs e)
        {
            using (var frm = new ProductModalForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                }
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
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

        private void DgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string columnName = dgvProducts.Columns[e.ColumnIndex].Name;

            if (columnName == "Description" && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.Length > 24)
                {
                    e.Value = text.Substring(0, 24) + "...";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "Category" && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.Length > 16)
                {
                    e.Value = text.Substring(0, 16) + "...";
                    e.FormattingApplied = true;
                }
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

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                HideImagePreview();
                return;
            }

            string currentColumnName = dgvProducts.Columns[e.ColumnIndex].Name;

            if (currentColumnName != "ProductImage")
            {
                HideImagePreview();
                return;
            }

            string imagePath = dgvProducts.Rows[e.RowIndex].Cells["ImageUrlHidden"].Value?.ToString();
            ShowImagePreview(imagePath);
        }

        private void DgvProducts_MouseLeave(object sender, EventArgs e)
        {
            int oldRow = hoveredRowIndex;
            int oldCol = hoveredColumnIndex;

            hoveredRowIndex = -1;
            hoveredColumnIndex = -1;

            if (oldRow >= 0 && oldCol >= 0)
                dgvProducts.InvalidateCell(oldCol, oldRow);

            HideImagePreview();
        }



        private void DgvProducts_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvProducts.Columns[e.ColumnIndex].Name;

            if (columnName == "IsActive")
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
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.FillRectangle(brush, badgeRect);
                    e.Graphics.DrawString(text, new Font("Segoe UI", 8.5F, FontStyle.Bold), textBrush, badgeRect, sf);
                }

                return;
            }

            if (columnName == "StockStatus")
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
                    e.CellBounds.X + (e.CellBounds.Width - 78) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                    78,
                    24
                );

                using (SolidBrush brush = new SolidBrush(backColor))
                using (SolidBrush textBrush = new SolidBrush(foreColor))
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.FillRectangle(brush, badgeRect);
                    e.Graphics.DrawString(text, new Font("Segoe UI", 8.5F, FontStyle.Bold), textBrush, badgeRect, sf);
                }

                return;
            }

            if (columnName == "colEdit" || columnName == "colDelete")
            {
                e.PaintBackground(e.CellBounds, true);
                e.Handled = true;

                bool isEdit = columnName == "colEdit";
                bool isHovered = e.RowIndex == hoveredRowIndex && e.ColumnIndex == hoveredColumnIndex;

                bool rowIsActive = false;
                object activeValue = dgvProducts.Rows[e.RowIndex].Cells["IsActive"].Value;

                if (activeValue != null && activeValue != DBNull.Value)
                {
                    try
                    {
                        rowIsActive = Convert.ToBoolean(activeValue);
                    }
                    catch
                    {
                        string activeText = activeValue.ToString();
                        rowIsActive = activeText == "Aktif" || activeText == "True" || activeText == "true";
                    }
                }

                string text;
                Color baseColor;

                if (isEdit)
                {
                    text = "Düzenle";
                    baseColor = Color.FromArgb(66, 133, 244);
                }
                else
                {
                    text = rowIsActive ? "Pasife Al" : "Aktifleştir";
                    baseColor = rowIsActive
                        ? Color.FromArgb(220, 53, 69)
                        : Color.FromArgb(25, 135, 84);
                }

                Color fillColor = isHovered ? baseColor : Color.White;
                Color borderColor = baseColor;
                Color textColor = isHovered ? Color.White : baseColor;

                Rectangle buttonRect = new Rectangle(
                    e.CellBounds.X + 6,
                    e.CellBounds.Y + 6,
                    e.CellBounds.Width - 12,
                    e.CellBounds.Height - 12
                );

                using (SolidBrush fillBrush = new SolidBrush(fillColor))
                using (Pen borderPen = new Pen(borderColor))
                using (SolidBrush textBrush = new SolidBrush(textColor))
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;

                    e.Graphics.FillRectangle(fillBrush, buttonRect);
                    e.Graphics.DrawRectangle(borderPen, buttonRect);
                    e.Graphics.DrawString(text, new Font("Segoe UI", 8.2F, FontStyle.Bold), textBrush, buttonRect, sf);
                }

                return;
            }
        }
        private void DgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            string columnName = dgvProducts.Columns[e.ColumnIndex].Name;
            int productId = Convert.ToInt32(dgvProducts.Rows[e.RowIndex].Cells["ProductId"].Value);

            if (columnName == "colEdit")
            {
                using (var frm = new ProductModalForm(productId))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        LoadProducts();
                    }
                }
            }
            else if (columnName == "colDelete")
            {
                bool isActive = false;

                var activeValue = dgvProducts.Rows[e.RowIndex].Cells["IsActive"].Value;
                if (activeValue != null)
                {
                    string text = activeValue.ToString();
                    isActive = text == "Aktif" || text == "True" || text == "true";
                }

                string message = isActive
                    ? "Bu ürünü pasife çekmek istiyor musunuz?"
                    : "Bu ürünü tekrar aktife almak istiyor musunuz?";

                DialogResult result = MessageBox.Show(
                    message,
                    "Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _repo.SetProductActiveStatus(productId, !isActive);
                        LoadProducts();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ürün durumu güncellenirken hata: " + ex.Message,
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}