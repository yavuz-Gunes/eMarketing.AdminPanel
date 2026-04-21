using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using eMarketing.AdminPanel.DataAccess;

namespace eMarketing.AdminPanel.Forms
{
    public partial class FrmProducts : Form
    {
        private readonly ProductRepository _productRepository = new ProductRepository();
        private readonly CategoryRepository _categoryRepository = new CategoryRepository();

        public FrmProducts()
        {
            InitializeComponent();
        }

        private void FrmProducts_Load(object sender, EventArgs e)
        {
            
            ConfigureGrid();
            PopulateCategories();
            PopulateFilterControls();
            ApplyTheme(false);
            LoadProducts();
        }

        private void FrmProducts_Layout(object sender, LayoutEventArgs e)
        {
            // boş bırakılabilir
        }

        private void ConfigureGrid()
        {
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProducts.RowTemplate.Height = 60;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.MultiSelect = false;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadProducts()
        {
            try
            {
                DataTable dt = _productRepository.GetAllProducts();
                if (dt == null)
                    return;

                if (!dt.Columns.Contains("StockStatus"))
                {
                    dt.Columns.Add("StockStatus", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        int stock = row["Stock"] != DBNull.Value ? Convert.ToInt32(row["Stock"]) : 0;

                        if (stock == 0)
                            row["StockStatus"] = "Tükendi";
                        else if (stock < 10)
                            row["StockStatus"] = "Kritik";
                        else
                            row["StockStatus"] = "Yeterli";
                    }
                }

                dgvProducts.DataSource = dt;
                ConfigureGridColumns();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürünler yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void ConfigureGridColumns()
        {
            if (dgvProducts.Columns.Contains("ProductId"))
                dgvProducts.Columns["ProductId"].HeaderText = "ID";

            if (dgvProducts.Columns.Contains("ProductName"))
                dgvProducts.Columns["ProductName"].HeaderText = "Ürün Adı";

            if (dgvProducts.Columns.Contains("Description"))
                dgvProducts.Columns["Description"].HeaderText = "Açıklama";

            if (dgvProducts.Columns.Contains("Price"))
                dgvProducts.Columns["Price"].HeaderText = "Fiyat";

            if (dgvProducts.Columns.Contains("Stock"))
                dgvProducts.Columns["Stock"].HeaderText = "Stok";

            if (dgvProducts.Columns.Contains("ImageUrl"))
                dgvProducts.Columns["ImageUrl"].HeaderText = "Görsel URL";

            if (dgvProducts.Columns.Contains("IsActive"))
                dgvProducts.Columns["IsActive"].HeaderText = "Aktif mi";

            if (dgvProducts.Columns.Contains("Category"))
                dgvProducts.Columns["Category"].HeaderText = "Kategori";

            if (dgvProducts.Columns.Contains("CategoryId"))
                dgvProducts.Columns["CategoryId"].Visible = false;

            if (dgvProducts.Columns.Contains("CreatedDate"))
                dgvProducts.Columns["CreatedDate"].HeaderText = "Oluşturulma Tarihi";

            if (dgvProducts.Columns.Contains("StockStatus"))
            {
                dgvProducts.Columns["StockStatus"].HeaderText = "Stok Durumu";
                dgvProducts.Columns["StockStatus"].DisplayIndex = dgvProducts.Columns.Count - 1;
                dgvProducts.Columns["StockStatus"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvProducts.Columns["StockStatus"].ReadOnly = true;
            }
        }

        private void PopulateCategories()
        {
            DataTable categories = _categoryRepository.GetActiveCategories();

            DataTable formTable = categories.Copy();
            cmbCategory.DataSource = formTable;
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "CategoryId";
            cmbCategory.SelectedIndex = -1;

            DataTable filterTable = categories.Copy();
            DataRow newRow = filterTable.NewRow();
            newRow["CategoryId"] = 0;
            newRow["CategoryName"] = "Tümü";
            filterTable.Rows.InsertAt(newRow, 0);

            cmbFilterCategory.DataSource = filterTable;
            cmbFilterCategory.DisplayMember = "CategoryName";
            cmbFilterCategory.ValueMember = "CategoryId";
            cmbFilterCategory.SelectedValue = 0;
        }

        private void PopulateFilterControls()
        {
            cmbFilterStockStatus.Items.Clear();
            cmbFilterStockStatus.Items.AddRange(new object[] { "Tümü", "Tükendi", "Kritik", "Yeterli" });
            cmbFilterStockStatus.SelectedIndex = 0;

            cmbFilterActive.Items.Clear();
            cmbFilterActive.Items.AddRange(new object[] { "Tümü", "Aktif", "Pasif" });
            cmbFilterActive.SelectedIndex = 0;
        }

        private bool ValidateProductForm(out decimal price, out int stock)
        {
            price = 0;
            stock = 0;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Ürün adı boş bırakılamaz.");
                txtProductName.Focus();
                return false;
            }

            if (cmbCategory.SelectedValue == null || Convert.ToInt32(cmbCategory.SelectedValue) <= 0)
            {
                MessageBox.Show("Lütfen kategori seçin.");
                cmbCategory.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Geçerli bir fiyat girin.");
                txtPrice.Focus();
                return false;
            }

            if (!int.TryParse(txtStock.Text, out stock) || stock < 0)
            {
                MessageBox.Show("Geçerli bir stok değeri girin.");
                txtStock.Focus();
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            txtProductId.Text = string.Empty;
            txtProductName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtPrice.Text = string.Empty;
            txtStock.Text = string.Empty;
            txtImageUrl.Text = string.Empty;
            chkIsActive.Checked = false;

            if (cmbCategory.Items.Count > 0)
                cmbCategory.SelectedIndex = -1;
        }

        private void ClearForm()
        {
            ClearInputs();
        }

        private void FillFormFromCurrentRow()
        {
            try
            {
                if (dgvProducts.CurrentRow == null)
                {
                    ClearForm();
                    return;
                }

                DataRowView drv = dgvProducts.CurrentRow.DataBoundItem as DataRowView;
                if (drv == null)
                {
                    ClearForm();
                    return;
                }

                txtProductId.Text = drv["ProductId"]?.ToString();
                txtProductName.Text = drv["ProductName"]?.ToString();
                txtDescription.Text = drv["Description"]?.ToString();
                txtPrice.Text = drv["Price"]?.ToString();
                txtStock.Text = drv["Stock"]?.ToString();
                txtImageUrl.Text = drv["ImageUrl"]?.ToString();
                chkIsActive.Checked = drv["IsActive"] != DBNull.Value && Convert.ToBoolean(drv["IsActive"]);

                // 🔥 EN KRİTİK KISIM (Kategori)
                if (drv.Row.Table.Columns.Contains("CategoryId") && drv["CategoryId"] != DBNull.Value)
                {
                    cmbCategory.SelectedValue = Convert.ToInt32(drv["CategoryId"]);
                }
                else
                {
                    cmbCategory.SelectedIndex = -1;
                }
            }
            catch
            {
                ClearForm();
            }
        }

        private void UpdateSummary()
        {
            try
            {
                DataTable dt = dgvProducts.DataSource as DataTable;
                if (dt == null)
                {
                    lblCardTotalValue.Text = "0";
                    lblCardActiveValue.Text = "0";
                    lblCardPassiveValue.Text = "0";
                    lblCardLowValue.Text = "0";
                    return;
                }

                DataView dv = dt.DefaultView;
                IEnumerable<DataRow> rows = dv.ToTable().AsEnumerable();

                lblCardTotalValue.Text = rows.Count().ToString();
                lblCardActiveValue.Text = rows.Count(r => r.Field<bool>("IsActive")).ToString();
                lblCardPassiveValue.Text = rows.Count(r => !r.Field<bool>("IsActive")).ToString();
                lblCardLowValue.Text = rows.Count(r => r.Field<int>("Stock") < 10).ToString();
            }
            catch
            {
            }
        }

        private void ApplyFilters()
        {
            try
            {
                DataTable dt = dgvProducts.DataSource as DataTable;
                if (dt == null)
                    return;

                DataView dv = dt.DefaultView;
                List<string> filters = new List<string>();

                if (cmbFilterCategory.SelectedValue != null &&
                    int.TryParse(cmbFilterCategory.SelectedValue.ToString(), out int selectedCategoryId) &&
                    selectedCategoryId != 0)
                {
                    filters.Add($"CategoryId = {selectedCategoryId}");
                }

                if (cmbFilterActive.SelectedItem != null && cmbFilterActive.SelectedItem.ToString() != "Tümü")
                {
                    if (cmbFilterActive.SelectedItem.ToString() == "Aktif")
                        filters.Add("IsActive = true");
                    else
                        filters.Add("IsActive = false");
                }

                if (cmbFilterStockStatus.SelectedItem != null && cmbFilterStockStatus.SelectedItem.ToString() != "Tümü")
                {
                    string stockFilter = cmbFilterStockStatus.SelectedItem.ToString();

                    if (stockFilter == "Tükendi")
                        filters.Add("Stock = 0");
                    else if (stockFilter == "Kritik")
                        filters.Add("Stock >= 1 AND Stock <= 9");
                    else if (stockFilter == "Yeterli")
                        filters.Add("Stock >= 10");
                }

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchText = txtSearch.Text.Trim().Replace("'", "''");
                    filters.Add($"ProductName LIKE '%{searchText}%'");
                }

                dv.RowFilter = string.Join(" AND ", filters);
                UpdateSummary();
            }
            catch
            {
            }
        }

        private void ApplyTheme(bool isDark)
        {
            Color back = isDark ? Color.FromArgb(34, 34, 34) : Color.White;
            Color panel = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(250, 250, 250);
            Color fore = isDark ? Color.White : Color.FromArgb(34, 34, 34);

            BackColor = back;

            foreach (Control control in Controls)
            {
                if (control is Panel)
                    control.BackColor = panel;
                else
                    control.BackColor = back;

                control.ForeColor = fore;
            }

            pnlTopBar.BackColor = isDark ? Color.FromArgb(40, 40, 43) : Color.FromArgb(248, 249, 250);
            pnlProductCard.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.FromArgb(255, 255, 255);
            pnlFilterBar.BackColor = isDark ? Color.FromArgb(40, 40, 43) : Color.FromArgb(245, 245, 245);
            pnlSummaryCard.BackColor = isDark ? Color.FromArgb(37, 37, 38) : Color.FromArgb(250, 250, 250);

            dgvProducts.BackgroundColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            dgvProducts.ForeColor = fore;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = isDark ? Color.FromArgb(40, 40, 40) : Color.FromArgb(250, 250, 250);
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.FromArgb(50, 50, 53) : Color.FromArgb(60, 63, 65);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateProductForm(out decimal price, out int stock))
                    return;

                int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);

                _productRepository.AddProduct(
                    txtProductName.Text.Trim(),
                    txtDescription.Text.Trim(),
                    price,
                    stock,
                    txtImageUrl.Text.Trim(),
                    chkIsActive.Checked,
                    categoryId
                );

                MessageBox.Show("Ürün başarıyla eklendi.");
                LoadProducts();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün eklenirken hata oluştu: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtProductId.Text))
                {
                    MessageBox.Show("Lütfen güncellenecek ürünü seçin.");
                    return;
                }

                if (!ValidateProductForm(out decimal price, out int stock))
                    return;

                int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);

                _productRepository.UpdateProduct(
                    int.Parse(txtProductId.Text),
                    txtProductName.Text.Trim(),
                    txtDescription.Text.Trim(),
                    price,
                    stock,
                    txtImageUrl.Text.Trim(),
                    chkIsActive.Checked,
                    categoryId
                );

                MessageBox.Show("Ürün başarıyla güncellendi.");
                LoadProducts();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün güncellenirken hata oluştu: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtProductId.Text))
                {
                    MessageBox.Show("Lütfen silinecek ürünü seçin.");
                    return;
                }

                DialogResult result = MessageBox.Show(
                    "Bu ürünü silmek istediğinize emin misiniz?",
                    "Silme Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;

                _productRepository.DeleteProduct(int.Parse(txtProductId.Text));

                MessageBox.Show("Ürün silindi.");
                LoadProducts();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ürün silinirken hata oluştu: " + ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void btnNewProduct_Click(object sender, EventArgs e)
        {
            ClearForm();
            txtProductName.Focus();
        }

        private void chkDarkMode_CheckedChanged(object sender, EventArgs e)
        {
            ApplyTheme(chkDarkMode.Checked);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilterCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilterStockStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void cmbFilterActive_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgvProducts.Rows[e.RowIndex].Selected = true;
                FillFormFromCurrentRow();
            }
        }

        private void dgvProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            FillFormFromCurrentRow();
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            FillFormFromCurrentRow();
            UpdateSummary();
        }

        private void dgvProducts_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                object cellValue = dgvProducts.Rows[e.RowIndex].Cells["Stock"].Value;
                int stock = cellValue != null && cellValue != DBNull.Value
                    ? Convert.ToInt32(cellValue)
                    : 0;

                if (stock == 0)
                {
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                }
                else if (stock >= 1 && stock <= 9)
                {
                    dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238);
                }
                else
                {
                    if (e.RowIndex % 2 == 0)
                        dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    else
                        dgvProducts.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
                }
            }
            catch
            {
            }
        }

        private void dgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                string columnName = dgvProducts.Columns[e.ColumnIndex].Name;

                if (columnName == "StockStatus")
                {
                    string value = e.Value?.ToString();

                    if (value == "Tükendi")
                    {
                        e.CellStyle.BackColor = Color.LightGray;
                        e.CellStyle.ForeColor = Color.Black;
                    }
                    else if (value == "Kritik")
                    {
                        e.CellStyle.BackColor = Color.LightSalmon;
                        e.CellStyle.ForeColor = Color.Black;
                    }
                    else if (value == "Yeterli")
                    {
                        e.CellStyle.BackColor = Color.FromArgb(232, 245, 233);
                        e.CellStyle.ForeColor = Color.Black;
                    }
                }

                if (columnName == "Category")
                {
                    e.CellStyle.BackColor = Color.FromArgb(245, 245, 245);
                    e.CellStyle.ForeColor = Color.FromArgb(60, 63, 65);
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            catch
            {
            }
        }

        private void lblCardLowTitle_Click(object sender, EventArgs e)
        {

        }
    }
}