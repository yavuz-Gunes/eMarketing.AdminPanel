using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class CategoriesPage : UserControl
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private Panel gridPanel;

        private Label lblTitle;
        private Label lblSubtitle;

        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private Button btnSearch;
        private Button btnNewCategory;

        private DataGridView dgvCategories;

        private CategoriesCard cTotal;
        private CategoriesCard cActive;
        private CategoriesCard cPassive;
        private CategoriesCard cShown;

        private Timer searchTimer;
        private int hoveredRowIndex = -1;
        private int hoveredColumnIndex = -1;

        public CategoriesPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            searchTimer = new Timer();
            searchTimer.Interval = 350;
            searchTimer.Tick += SearchTimer_Tick;

            BuildLayout();
            Load += CategoriesPage_Load;
        }

        private async void CategoriesPage_Load(object sender, EventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private void BuildLayout()
        {
            BuildHeaderPanel();
            BuildStatsPanel();
            BuildFilterPanel();
            BuildGridPanel();

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
                Text = "Kategoriler",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            lblSubtitle = new Label
            {
                Text = "Kategori kayıtlarını yönetin.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 34)
            };

            btnNewCategory = new Button
            {
                Text = "+ Yeni Kategori",
                Width = 160,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White
            };

            btnNewCategory.FlatAppearance.BorderSize = 0;
            btnNewCategory.Click += BtnNewCategory_Click;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(btnNewCategory);

            headerPanel.Resize += (s, e) =>
            {
                btnNewCategory.Location = new Point(headerPanel.Width - btnNewCategory.Width, 4);
            };
        }

        private void BuildStatsPanel()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = AppColors.Background,
                Padding = new Padding(0, 0, 0, 20)
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

            cTotal = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            cActive = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            cPassive = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 12, 0) };
            cShown = new CategoriesCard { Dock = DockStyle.Fill, Margin = Padding.Empty };

            cTotal.SetData("📁", "Toplam", "0");
            cActive.SetData("✅", "Aktif", "0");
            cPassive.SetData("⛔", "Pasif", "0");
            cShown.SetData("🔎", "Gösterilen", "0");

            grid.Controls.Add(cTotal, 0, 0);
            grid.Controls.Add(cActive, 1, 0);
            grid.Controls.Add(cPassive, 2, 0);
            grid.Controls.Add(cShown, 3, 0);

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
                Width = 300,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 16)
            };

            cmbStatus = new ComboBox
            {
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(332, 16)
            };

            cmbStatus.Items.Add("Aktif");
            cmbStatus.Items.Add("Pasif");
            cmbStatus.Items.Add("Hepsi");
            cmbStatus.SelectedIndex = 0;

            btnSearch = new Button
            {
                Text = "Ara",
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(488, 15)
            };

            btnSearch.FlatAppearance.BorderColor = Color.Gainsboro;

            txtSearch.KeyDown += TxtSearch_KeyDown;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            btnSearch.Click += BtnSearch_Click;

            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(cmbStatus);
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

            dgvCategories = new DataGridView
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
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
            };

            dgvCategories.EnableHeadersVisualStyles = false;
            dgvCategories.ColumnHeadersHeight = 40;
            dgvCategories.RowTemplate.Height = 48;
            dgvCategories.GridColor = Color.Gainsboro;
            dgvCategories.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvCategories.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvCategories.DefaultCellStyle.BackColor = Color.White;

            dgvCategories.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvCategories.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCategories.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvCategories.DefaultCellStyle.SelectionBackColor = Color.FromArgb(250, 251, 253);
            dgvCategories.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            ConfigureGridColumns();

            dgvCategories.CellContentClick += DgvCategories_CellContentClick;
            dgvCategories.CellMouseEnter += DgvCategories_CellMouseEnter;
            dgvCategories.CellMouseLeave += DgvCategories_CellMouseLeave;
            dgvCategories.CellPainting += DgvCategories_CellPainting;

            gridPanel.Controls.Add(dgvCategories);
        }

        private void ConfigureGridColumns()
        {
            dgvCategories.Columns.Clear();

            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "KategoriId",
                DataPropertyName = "KategoriId",
                HeaderText = "ID",
                Visible = false
            });

            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "KategoriAdi",
                DataPropertyName = "KategoriAdi",
                HeaderText = "Kategori Adı",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 48
            });

            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AktifMi",
                DataPropertyName = "AktifMi",
                HeaderText = "Durum",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 16,
                MinimumWidth = 90
            });

            dgvCategories.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OlusturmaTarihi",
                DataPropertyName = "OlusturmaTarihi",
                HeaderText = "Oluşturulma",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 20,
                MinimumWidth = 120,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "dd.MM.yyyy HH:mm"
                }
            });

            dgvCategories.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colEdit",
                HeaderText = "",
                Text = "Düzenle",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 13,
                MinimumWidth = 86
            });

            dgvCategories.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colDelete",
                HeaderText = "",
                Text = "Sil",
                UseColumnTextForButtonValue = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 10,
                MinimumWidth = 70
            });
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                DataTable table = await GetCategoriesAsync(txtSearch.Text.Trim(), GetSelectedStatus());
                dgvCategories.DataSource = table;

                await UpdateStatsAsync(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateStatsAsync(DataTable currentTable)
        {
            try
            {
                DataTable allTable = await GetCategoriesAsync("", -1);

                int totalCount = allTable.Rows.Count;
                int activeCount = 0;
                int passiveCount = 0;

                foreach (DataRow row in allTable.Rows)
                {
                    bool isActive = row["AktifMi"] != DBNull.Value && Convert.ToBoolean(row["AktifMi"]);

                    if (isActive)
                        activeCount++;
                    else
                        passiveCount++;
                }

                int shownCount = currentTable.Rows.Count;

                cTotal.SetData("📁", "Toplam", totalCount.ToString());
                cActive.SetData("✅", "Aktif", activeCount.ToString());
                cPassive.SetData("⛔", "Pasif", passiveCount.ToString());
                cShown.SetData("🔎", "Gösterilen", shownCount.ToString());
            }
            catch
            {
                cTotal.SetData("📁", "Toplam", "0");
                cActive.SetData("✅", "Aktif", "0");
                cPassive.SetData("⛔", "Pasif", "0");
                cShown.SetData("🔎", "Gösterilen", "0");
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

        private Task<DataTable> GetCategoriesAsync(string search, int status)
        {
            return _apiClient.GetCategoriesAsync(search, status);
        }

        private void DgvCategories_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvCategories.Columns[e.ColumnIndex].Name;

            if (columnName == "colEdit" || columnName == "colDelete")
            {
                hoveredRowIndex = e.RowIndex;
                hoveredColumnIndex = e.ColumnIndex;
                dgvCategories.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        private void DgvCategories_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            int oldRow = hoveredRowIndex;
            int oldCol = hoveredColumnIndex;

            hoveredRowIndex = -1;
            hoveredColumnIndex = -1;

            if (oldRow >= 0 && oldCol >= 0)
                dgvCategories.InvalidateCell(oldCol, oldRow);
        }

        private void DgvCategories_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvCategories.Columns[e.ColumnIndex].Name;

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

                int badgeWidth = 72;
                int badgeHeight = 24;

                Rectangle badgeRect = new Rectangle(
                    e.CellBounds.X + (e.CellBounds.Width - badgeWidth) / 2,
                    e.CellBounds.Y + (e.CellBounds.Height - badgeHeight) / 2,
                    badgeWidth,
                    badgeHeight
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
                object activeValue = dgvCategories.Rows[e.RowIndex].Cells["AktifMi"].Value;

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
                    e.CellBounds.X + 8,
                    e.CellBounds.Y + 6,
                    e.CellBounds.Width - 16,
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
                    e.Graphics.DrawString(text, new Font("Segoe UI", 8.5F, FontStyle.Bold), textBrush, buttonRect, sf);
                }

                return;
            }
        }

        private async void DgvCategories_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            string columnName = dgvCategories.Columns[e.ColumnIndex].Name;
            int categoryId = Convert.ToInt32(dgvCategories.Rows[e.RowIndex].Cells["KategoriId"].Value);

            bool isActive = false;
            object activeValue = dgvCategories.Rows[e.RowIndex].Cells["AktifMi"].Value;

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

            if (columnName == "colEdit")
            {
                if (isActive)
                {
                    using (var frm = new CategoryModalForm(categoryId))
                    {
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadCategoriesAsync();
                        }
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        "Bu kategoriyi tekrar aktifleştirmek istiyor musunuz?",
                        "Onay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        await SetCategoryActiveStatusAsync(categoryId, true);
                        await LoadCategoriesAsync();
                    }
                }
            }
            else if (columnName == "colDelete")
            {
                if (isActive)
                {
                    DialogResult result = MessageBox.Show(
                        "Bu kategoriyi pasife çekmek istiyor musunuz?",
                        "Onay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        await SetCategoryActiveStatusAsync(categoryId, false);
                        await LoadCategoriesAsync();
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show(
                        "Bu pasif kategoriyi kalıcı olarak silmek istiyor musunuz?",
                        "Kalıcı Silme",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        if (await DeleteCategoryAsync(categoryId))
                        {
                            await LoadCategoriesAsync();
                        }
                    }
                }
            }
        }

        private async void BtnNewCategory_Click(object sender, EventArgs e)
        {
            using (var frm = new CategoryModalForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadCategoriesAsync();
                }
            }
        }

        private Task SetCategoryActiveStatusAsync(int categoryId, bool isActive)
        {
            return _apiClient.SetCategoryActiveStatusAsync(categoryId, isActive);
        }

        private async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            await _apiClient.DeleteCategoryAsync(categoryId);
            return true;
        }

        private async void BtnSearch_Click(object sender, EventArgs e)
        {
            await LoadCategoriesAsync();
        }

        private async void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                searchTimer.Stop();
                await LoadCategoriesAsync();
                e.SuppressKeyPress = true;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private async void SearchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            await LoadCategoriesAsync();
        }

        private async void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadCategoriesAsync();
        }
    }
}
