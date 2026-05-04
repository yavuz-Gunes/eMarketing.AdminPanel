using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Pages
{
    public class CustomersPage : UserControl, IThemeable
    {
        private readonly CustomerRepository _repo = new CustomerRepository();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private SplitContainer contentSplit;

        private Panel customersPanel;
        private Panel storesPanel;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblCustomerInfo;
        private Label lblStoresTitle;
        private Label lblStoresSubtitle;

        private Button btnNewCustomer;
        private Button btnNewStore;

        private TextBox txtSearch;
        private ComboBox cmbStatus;
        private Button btnSearch;
        private Button btnClear;

        private DataGridView dgvCustomers;
        private DataGridView dgvStores;

        private CategoriesCard cTotal;
        private CategoriesCard cActive;
        private CategoriesCard cPassive;
        private CategoriesCard cStores;
        private CategoriesCard cRevenue;

        private Timer searchTimer;

        private DataTable customersTable;
        private DataTable storesTable;

        private int selectedCustomerId = 0;

        private int hoveredCustomerRowIndex = -1;
        private int hoveredCustomerColumnIndex = -1;

        private int hoveredStoreRowIndex = -1;
        private int hoveredStoreColumnIndex = -1;

        public CustomersPage()
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
            Load += CustomersPage_Load;
        }

        private void CustomersPage_Load(object sender, EventArgs e)
        {
            LoadCustomers();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            BuildHeaderPanel();
            BuildStatsPanel();
            BuildFilterPanel();
            BuildContentArea();

            Controls.Add(contentSplit);
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
                Text = "Müşteriler",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            lblSubtitle = new Label
            {
                Text = "Firma müşterilerini, mağaza/şube bilgilerini ve sipariş ilişkilerini yönetin.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 38)
            };

            btnNewCustomer = new Button
            {
                Text = "+ Yeni Müşteri",
                Width = 160,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnNewCustomer.FlatAppearance.BorderSize = 0;
            btnNewCustomer.Click += BtnNewCustomer_Click;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(btnNewCustomer);

            headerPanel.Resize += (s, e) =>
            {
                btnNewCustomer.Location = new Point(headerPanel.Width - btnNewCustomer.Width, 6);
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
                ColumnCount = 5,
                RowCount = 1,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cTotal = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cActive = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cPassive = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cStores = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cRevenue = new CategoriesCard { Dock = DockStyle.Fill, Margin = Padding.Empty };

            cTotal.SetData("🏢", "Toplam", "0");
            cActive.SetData("✅", "Aktif", "0");
            cPassive.SetData("⛔", "Pasif", "0");
            cStores.SetData("🏬", "Mağaza", "0");
            cRevenue.SetData("₺", "Ciro", "0");

            grid.Controls.Add(cTotal, 0, 0);
            grid.Controls.Add(cActive, 1, 0);
            grid.Controls.Add(cPassive, 2, 0);
            grid.Controls.Add(cStores, 3, 0);
            grid.Controls.Add(cRevenue, 4, 0);

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
                Width = 320,
                Font = new Font("Segoe UI", 10F)
            };

            cmbStatus = new ComboBox
            {
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            cmbStatus.Items.Add("Hepsi");
            cmbStatus.Items.Add("Aktif");
            cmbStatus.Items.Add("Pasif");
            cmbStatus.SelectedIndex = 0;

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

            lblCustomerInfo = new Label
            {
                Text = "0 kayıt",
                Width = 180,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            btnSearch.Click += BtnSearch_Click;
            btnClear.Click += BtnClear_Click;

            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(cmbStatus);
            filterPanel.Controls.Add(btnSearch);
            filterPanel.Controls.Add(btnClear);
            filterPanel.Controls.Add(lblCustomerInfo);

            filterPanel.Resize += (s, e) => PlaceFilterControls();
            PlaceFilterControls();
        }

        private void PlaceFilterControls()
        {
            int x = 16;
            int y = 20;

            txtSearch.Location = new Point(x, y);
            x += txtSearch.Width + 14;

            cmbStatus.Location = new Point(x, y);
            x += cmbStatus.Width + 14;

            btnSearch.Location = new Point(x, y - 2);
            x += btnSearch.Width + 10;

            btnClear.Location = new Point(x, y - 2);

            lblCustomerInfo.Location = new Point(filterPanel.Width - lblCustomerInfo.Width - 16, y);
            lblCustomerInfo.Visible = lblCustomerInfo.Left > btnClear.Right + 20;
        }

        private void BuildContentArea()
        {
            contentSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BackColor = AppColors.Background
            };

            BuildCustomersPanel();
            BuildStoresPanel();

            contentSplit.Panel1.Controls.Add(customersPanel);
            contentSplit.Panel2.Controls.Add(storesPanel);

            contentSplit.HandleCreated += (s, e) => SetSafeSplitterDistance();
            contentSplit.SizeChanged += (s, e) => SetSafeSplitterDistance();
        }
        private void SetSafeSplitterDistance()
        {
            if (contentSplit == null)
                return;

            if (contentSplit.Height <= 0)
                return;

            int totalHeight = contentSplit.Height;

            int panel1Min = 180;
            int panel2Min = 140;

            if (totalHeight <= panel1Min + panel2Min + contentSplit.SplitterWidth)
                return;

            int maxDistance = totalHeight - panel2Min - contentSplit.SplitterWidth;
            int desiredDistance = 360;

            if (desiredDistance < panel1Min)
                desiredDistance = panel1Min;

            if (desiredDistance > maxDistance)
                desiredDistance = maxDistance;

            try
            {
                contentSplit.SplitterDistance = desiredDistance;
            }
            catch
            {
                // İlk yüklenme anında ölçü oturmazsa sessiz geçsin.
            }
        }

        private void BuildCustomersPanel()
        {
            customersPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(12)
            };

            dgvCustomers = new DataGridView
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };

            ApplyGridStyle(dgvCustomers);
            ConfigureCustomerGridColumns();

            dgvCustomers.CellClick += DgvCustomers_CellClick;
            dgvCustomers.CellDoubleClick += DgvCustomers_CellDoubleClick;
            dgvCustomers.CellFormatting += DgvCustomers_CellFormatting;
            dgvCustomers.CellPainting += DgvCustomers_CellPainting;
            dgvCustomers.CellMouseMove += DgvCustomers_CellMouseMove;
            dgvCustomers.MouseLeave += DgvCustomers_MouseLeave;

            customersPanel.Controls.Add(dgvCustomers);
        }

        private void BuildStoresPanel()
        {
            storesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(12)
            };

            Panel storeHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = AppColors.CardBackground
            };

            lblStoresTitle = new Label
            {
                Text = "Müşteri Mağazaları / Şubeleri",
                Location = new Point(0, 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            lblStoresSubtitle = new Label
            {
                Text = "Bir müşteri seçildiğinde mağaza/şube kayıtları burada görünür.",
                Location = new Point(2, 28),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppColors.TextSecondary
            };

            btnNewStore = new Button
            {
                Text = "+ Yeni Mağaza",
                Width = 145,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnNewStore.FlatAppearance.BorderSize = 0;
            btnNewStore.Click += BtnNewStore_Click;

            storeHeader.Controls.Add(lblStoresTitle);
            storeHeader.Controls.Add(lblStoresSubtitle);
            storeHeader.Controls.Add(btnNewStore);

            storeHeader.Resize += (s, e) =>
            {
                btnNewStore.Location = new Point(storeHeader.Width - btnNewStore.Width, 8);
            };

            dgvStores = new DataGridView
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };

            ApplyGridStyle(dgvStores);
            ConfigureStoreGridColumns();

            dgvStores.CellClick += DgvStores_CellClick;
            dgvStores.CellDoubleClick += DgvStores_CellDoubleClick;
            dgvStores.CellPainting += DgvStores_CellPainting;
            dgvStores.CellMouseMove += DgvStores_CellMouseMove;
            dgvStores.MouseLeave += DgvStores_MouseLeave;

            storesPanel.Controls.Add(dgvStores);
            storesPanel.Controls.Add(storeHeader);
        }

        private void ApplyGridStyle(DataGridView grid)
        {
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 42;
            grid.RowTemplate.Height = 48;
            grid.GridColor = AppColors.Border;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.WhiteSmoke;

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            grid.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        }

        private void ConfigureCustomerGridColumns()
        {
            dgvCustomers.Columns.Clear();

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerId",
                DataPropertyName = "CustomerId",
                Visible = false
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CompanyName",
                DataPropertyName = "CompanyName",
                HeaderText = "Firma",
                Width = 170
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                DataPropertyName = "FullName",
                HeaderText = "Görünen Ad",
                Width = 150
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AuthorizedPerson",
                DataPropertyName = "AuthorizedPerson",
                HeaderText = "Yetkili",
                Width = 130
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                DataPropertyName = "Phone",
                HeaderText = "Telefon",
                Width = 120
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Email",
                DataPropertyName = "Email",
                HeaderText = "E-Posta",
                Width = 170
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerType",
                DataPropertyName = "CustomerType",
                HeaderText = "Tip",
                Width = 90
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StoreCount",
                DataPropertyName = "StoreCount",
                HeaderText = "Mağaza",
                Width = 75
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderCount",
                DataPropertyName = "OrderCount",
                HeaderText = "Sipariş",
                Width = 75
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalRevenue",
                DataPropertyName = "TotalRevenue",
                HeaderText = "Ciro",
                Width = 110
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IsActive",
                DataPropertyName = "IsActive",
                HeaderText = "Durum",
                Width = 90
            });

            dgvCustomers.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colCustomerEdit",
                HeaderText = "",
                Text = "Düzenle",
                UseColumnTextForButtonValue = true,
                Width = 92
            });

            dgvCustomers.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colCustomerStatus",
                HeaderText = "",
                Text = "Durum",
                UseColumnTextForButtonValue = true,
                Width = 100
            });
        }

        private void ConfigureStoreGridColumns()
        {
            dgvStores.Columns.Clear();

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerStoreId",
                DataPropertyName = "CustomerStoreId",
                Visible = false
            });

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerId",
                DataPropertyName = "CustomerId",
                Visible = false
            });

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StoreName",
                DataPropertyName = "StoreName",
                HeaderText = "Mağaza / Şube",
                Width = 220
            });

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "City",
                DataPropertyName = "City",
                HeaderText = "Şehir",
                Width = 110
            });

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "District",
                DataPropertyName = "District",
                HeaderText = "İlçe",
                Width = 110
            });

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ResponsiblePerson",
                DataPropertyName = "ResponsiblePerson",
                HeaderText = "Sorumlu",
                Width = 140
            });

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                DataPropertyName = "Phone",
                HeaderText = "Telefon",
                Width = 120
            });

            dgvStores.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IsActive",
                DataPropertyName = "IsActive",
                HeaderText = "Durum",
                Width = 90
            });

            dgvStores.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colStoreEdit",
                HeaderText = "",
                Text = "Düzenle",
                UseColumnTextForButtonValue = true,
                Width = 92
            });

            dgvStores.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colStoreStatus",
                HeaderText = "",
                Text = "Durum",
                UseColumnTextForButtonValue = true,
                Width = 100
            });
        }

        private void LoadCustomers()
        {
            try
            {
                customersTable = _repo.GetCustomers(txtSearch.Text.Trim(), GetSelectedStatus());
                dgvCustomers.DataSource = customersTable;

                UpdateStats(customersTable);
                UpdateInfoLabel(customersTable.Rows.Count);

                selectedCustomerId = 0;
                dgvStores.DataSource = null;
                lblStoresSubtitle.Text = "Bir müşteri seçildiğinde mağaza/şube kayıtları burada görünür.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteriler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStores(int customerId)
        {
            try
            {
                selectedCustomerId = customerId;
                storesTable = _repo.GetCustomerStores(customerId, -1);
                dgvStores.DataSource = storesTable;

                lblStoresSubtitle.Text = "Seçili müşteriye ait " + storesTable.Rows.Count + " mağaza/şube kaydı listeleniyor.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müşteri mağazaları yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetSelectedStatus()
        {
            if (cmbStatus.SelectedIndex == 1)
                return 1;

            if (cmbStatus.SelectedIndex == 2)
                return 0;

            return -1;
        }

        private void UpdateStats(DataTable table)
        {
            int total = 0;
            int active = 0;
            int passive = 0;
            int storeCount = 0;
            decimal revenue = 0;

            if (table != null)
            {
                total = table.Rows.Count;

                foreach (DataRow row in table.Rows)
                {
                    bool isActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);

                    if (isActive)
                        active++;
                    else
                        passive++;

                    if (table.Columns.Contains("StoreCount") && row["StoreCount"] != DBNull.Value)
                        storeCount += Convert.ToInt32(row["StoreCount"]);

                    if (table.Columns.Contains("TotalRevenue") && row["TotalRevenue"] != DBNull.Value)
                        revenue += Convert.ToDecimal(row["TotalRevenue"]);
                }
            }

            cTotal.SetData("🏢", "Toplam", total.ToString());
            cActive.SetData("✅", "Aktif", active.ToString());
            cPassive.SetData("⛔", "Pasif", passive.ToString());
            cStores.SetData("🏬", "Mağaza", storeCount.ToString());
            cRevenue.SetData("₺", "Ciro", revenue.ToString("N0", new CultureInfo("tr-TR")));
        }

        private void UpdateInfoLabel(int count)
        {
            lblCustomerInfo.Text = count + " kayıt";
        }

        private int GetCustomerIdFromRow(int rowIndex)
        {
            if (rowIndex < 0)
                return 0;

            object value = dgvCustomers.Rows[rowIndex].Cells["CustomerId"].Value;

            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToInt32(value);
        }

        private int GetCustomerStoreIdFromRow(int rowIndex)
        {
            if (rowIndex < 0)
                return 0;

            object value = dgvStores.Rows[rowIndex].Cells["CustomerStoreId"].Value;

            if (value == null || value == DBNull.Value)
                return 0;

            return Convert.ToInt32(value);
        }

        private bool GetGridRowActiveStatus(DataGridView grid, int rowIndex)
        {
            if (rowIndex < 0)
                return false;

            object value = grid.Rows[rowIndex].Cells["IsActive"].Value;

            if (value == null || value == DBNull.Value)
                return false;

            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                string text = value.ToString();
                return text == "Aktif" || text == "True" || text == "true";
            }
        }

        private void BtnNewCustomer_Click(object sender, EventArgs e)
        {
            using (CustomerModalForm frm = new CustomerModalForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                    LoadCustomers();
            }
        }

        private void BtnNewStore_Click(object sender, EventArgs e)
        {
            if (selectedCustomerId <= 0)
            {
                MessageBox.Show("Mağaza eklemek için önce müşteri seçmelisiniz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (CustomerStoreModalForm frm = new CustomerStoreModalForm(selectedCustomerId))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadStores(selectedCustomerId);
                    LoadCustomers();
                }
            }
        }

        private void DgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            int customerId = GetCustomerIdFromRow(e.RowIndex);

            if (customerId <= 0)
                return;

            selectedCustomerId = customerId;
            LoadStores(customerId);

            string columnName = dgvCustomers.Columns[e.ColumnIndex].Name;

            if (columnName == "colCustomerEdit")
            {
                OpenCustomerForm(customerId);
            }
            else if (columnName == "colCustomerStatus")
            {
                ToggleCustomerStatus(e.RowIndex, customerId);
            }
        }

        private void DgvCustomers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvCustomers.Columns[e.ColumnIndex].Name;

            if (columnName == "colCustomerEdit" || columnName == "colCustomerStatus")
                return;

            int customerId = GetCustomerIdFromRow(e.RowIndex);

            if (customerId > 0)
                OpenCustomerForm(customerId);
        }

        private void OpenCustomerForm(int customerId)
        {
            using (CustomerModalForm frm = new CustomerModalForm(customerId))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadCustomers();

                    if (customerId > 0)
                        LoadStores(customerId);
                }
            }
        }

        private void ToggleCustomerStatus(int rowIndex, int customerId)
        {
            bool isActive = GetGridRowActiveStatus(dgvCustomers, rowIndex);
            string message = isActive
                ? "Bu müşteriyi pasife almak istiyor musunuz?"
                : "Bu müşteriyi tekrar aktifleştirmek istiyor musunuz?";

            DialogResult result = MessageBox.Show(
                message,
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            _repo.SetCustomerActiveStatus(customerId, !isActive);
            LoadCustomers();
        }

        private void DgvStores_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvStores.Columns[e.ColumnIndex].Name;
            int customerStoreId = GetCustomerStoreIdFromRow(e.RowIndex);

            if (customerStoreId <= 0)
                return;

            if (columnName == "colStoreEdit")
            {
                OpenStoreForm(customerStoreId);
            }
            else if (columnName == "colStoreStatus")
            {
                ToggleStoreStatus(e.RowIndex, customerStoreId);
            }
        }

        private void DgvStores_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvStores.Columns[e.ColumnIndex].Name;

            if (columnName == "colStoreEdit" || columnName == "colStoreStatus")
                return;

            int customerStoreId = GetCustomerStoreIdFromRow(e.RowIndex);

            if (customerStoreId > 0)
                OpenStoreForm(customerStoreId);
        }

        private void OpenStoreForm(int customerStoreId)
        {
            if (selectedCustomerId <= 0)
                return;

            using (CustomerStoreModalForm frm = new CustomerStoreModalForm(selectedCustomerId, customerStoreId))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadStores(selectedCustomerId);
                    LoadCustomers();
                }
            }
        }

        private void ToggleStoreStatus(int rowIndex, int customerStoreId)
        {
            bool isActive = GetGridRowActiveStatus(dgvStores, rowIndex);
            string message = isActive
                ? "Bu mağazayı pasife almak istiyor musunuz?"
                : "Bu mağazayı tekrar aktifleştirmek istiyor musunuz?";

            DialogResult result = MessageBox.Show(
                message,
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            _repo.SetCustomerStoreActiveStatus(customerStoreId, !isActive);

            if (selectedCustomerId > 0)
                LoadStores(selectedCustomerId);

            LoadCustomers();
        }

        private void DgvCustomers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvCustomers.Columns[e.ColumnIndex].Name;

            if (columnName == "TotalRevenue" && e.Value != null && e.Value != DBNull.Value)
            {
                decimal value;

                if (decimal.TryParse(e.Value.ToString(), out value))
                {
                    e.Value = value.ToString("N2", new CultureInfo("tr-TR")) + " ₺";
                    e.FormattingApplied = true;
                }
            }

            if ((columnName == "CompanyName" || columnName == "FullName" || columnName == "Email") && e.Value != null)
            {
                string text = e.Value.ToString();

                if (text.Length > 22)
                {
                    e.Value = text.Substring(0, 22) + "...";
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvCustomers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            PaintGridSpecialCells(
                dgvCustomers,
                e,
                "colCustomerEdit",
                "colCustomerStatus",
                hoveredCustomerRowIndex,
                hoveredCustomerColumnIndex);
        }

        private void DgvStores_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            PaintGridSpecialCells(
                dgvStores,
                e,
                "colStoreEdit",
                "colStoreStatus",
                hoveredStoreRowIndex,
                hoveredStoreColumnIndex);
        }

        private void PaintGridSpecialCells(
            DataGridView grid,
            DataGridViewCellPaintingEventArgs e,
            string editColumnName,
            string statusColumnName,
            int hoveredRow,
            int hoveredCol)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = grid.Columns[e.ColumnIndex].Name;

            if (columnName == "IsActive")
            {
                PaintStatusBadge(grid, e);
                return;
            }

            if (columnName == editColumnName || columnName == statusColumnName)
            {
                PaintActionButton(grid, e, editColumnName, statusColumnName, hoveredRow, hoveredCol);
                return;
            }
        }

        private void PaintStatusBadge(DataGridView grid, DataGridViewCellPaintingEventArgs e)
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
        }

        private void PaintActionButton(
            DataGridView grid,
            DataGridViewCellPaintingEventArgs e,
            string editColumnName,
            string statusColumnName,
            int hoveredRow,
            int hoveredCol)
        {
            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            string columnName = grid.Columns[e.ColumnIndex].Name;

            bool isEdit = columnName == editColumnName;
            bool isHovered = e.RowIndex == hoveredRow && e.ColumnIndex == hoveredCol;
            bool rowIsActive = GetGridRowActiveStatus(grid, e.RowIndex);

            string text;
            Color baseColor;

            if (isEdit)
            {
                text = "Düzenle";
                baseColor = AppColors.Primary;
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
        }

        private void DgvCustomers_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            HandleHover(
                dgvCustomers,
                e,
                "colCustomerEdit",
                "colCustomerStatus",
                ref hoveredCustomerRowIndex,
                ref hoveredCustomerColumnIndex);
        }

        private void DgvStores_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            HandleHover(
                dgvStores,
                e,
                "colStoreEdit",
                "colStoreStatus",
                ref hoveredStoreRowIndex,
                ref hoveredStoreColumnIndex);
        }

        private void HandleHover(
            DataGridView grid,
            DataGridViewCellMouseEventArgs e,
            string editColumnName,
            string statusColumnName,
            ref int hoveredRow,
            ref int hoveredCol)
        {
            int newRow = -1;
            int newCol = -1;

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string columnName = grid.Columns[e.ColumnIndex].Name;

                if (columnName == editColumnName || columnName == statusColumnName)
                {
                    newRow = e.RowIndex;
                    newCol = e.ColumnIndex;
                }
            }

            if (newRow != hoveredRow || newCol != hoveredCol)
            {
                int oldRow = hoveredRow;
                int oldCol = hoveredCol;

                hoveredRow = newRow;
                hoveredCol = newCol;

                if (oldRow >= 0 && oldCol >= 0)
                    grid.InvalidateCell(oldCol, oldRow);

                if (hoveredRow >= 0 && hoveredCol >= 0)
                    grid.InvalidateCell(hoveredCol, hoveredRow);
            }
        }

        private void DgvCustomers_MouseLeave(object sender, EventArgs e)
        {
            ClearHover(dgvCustomers, ref hoveredCustomerRowIndex, ref hoveredCustomerColumnIndex);
        }

        private void DgvStores_MouseLeave(object sender, EventArgs e)
        {
            ClearHover(dgvStores, ref hoveredStoreRowIndex, ref hoveredStoreColumnIndex);
        }

        private void ClearHover(DataGridView grid, ref int hoveredRow, ref int hoveredCol)
        {
            int oldRow = hoveredRow;
            int oldCol = hoveredCol;

            hoveredRow = -1;
            hoveredCol = -1;

            if (oldRow >= 0 && oldCol >= 0)
                grid.InvalidateCell(oldCol, oldRow);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            LoadCustomers();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                searchTimer.Stop();
                LoadCustomers();
                e.SuppressKeyPress = true;
            }
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCustomers();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            searchTimer.Stop();
            LoadCustomers();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            searchTimer.Stop();

            txtSearch.Clear();
            cmbStatus.SelectedIndex = 0;

            LoadCustomers();
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

            if (customersPanel != null)
                customersPanel.BackColor = AppColors.CardBackground;

            if (storesPanel != null)
                storesPanel.BackColor = AppColors.CardBackground;

            if (contentSplit != null)
                contentSplit.BackColor = AppColors.Background;

            if (lblTitle != null)
                lblTitle.ForeColor = AppColors.TextPrimary;

            if (lblSubtitle != null)
                lblSubtitle.ForeColor = AppColors.TextSecondary;

            if (lblCustomerInfo != null)
                lblCustomerInfo.ForeColor = AppColors.TextSecondary;

            if (lblStoresTitle != null)
                lblStoresTitle.ForeColor = AppColors.TextPrimary;

            if (lblStoresSubtitle != null)
                lblStoresSubtitle.ForeColor = AppColors.TextSecondary;

            if (btnNewCustomer != null)
            {
                btnNewCustomer.BackColor = AppColors.Primary;
                btnNewCustomer.ForeColor = Color.White;
            }

            if (btnNewStore != null)
            {
                btnNewStore.BackColor = AppColors.Primary;
                btnNewStore.ForeColor = Color.White;
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

            if (dgvCustomers != null)
            {
                dgvCustomers.BackgroundColor = AppColors.CardBackground;
                dgvCustomers.GridColor = AppColors.Border;
                dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvCustomers.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvCustomers.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }

            if (dgvStores != null)
            {
                dgvStores.BackgroundColor = AppColors.CardBackground;
                dgvStores.GridColor = AppColors.Border;
                dgvStores.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvStores.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvStores.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }

            Invalidate(true);
            Refresh();
        }
    }
}