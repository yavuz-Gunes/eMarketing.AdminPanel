using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class OrdersPage : UserControl, IThemeable
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private Panel gridPanel;
        private Panel footerPanel;

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblInfo;

        private Button btnNewOrder;

        private CategoriesCard cTotal;
        private CategoriesCard cPreparing;
        private CategoriesCard cShipped;
        private CategoriesCard cDelivered;
        private CategoriesCard cCancelled;

        private TextBox txtSearch;
        private ComboBox cmbFilterStatus;
        private Button btnSearch;
        private Button btnClear;

        private DataGridView dgvOrders;

        private Label lblSelectedOrder;
        private Label lblSelectedSummary;
        private Label lblSelectedBayi;
        private Label lblSelectedProduct;
        private Label lblSelectedAmount;
        private TextBox txtOrderId;
        private ComboBox cmbStatus;
        private Button btnUpdateStatus;
        private Button btnOpenDetail;

        private DataTable ordersTable;
        private Timer searchTimer;

        private int hoveredRowIndex = -1;
        private int hoveredColumnIndex = -1;

        private bool AdminModu
        {
            get { return AppSession.AdminMi; }
        }

        public OrdersPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            searchTimer = new Timer
            {
                Interval = 300
            };
            searchTimer.Tick += SearchTimer_Tick;

            BuildLayout();
            Load += OrdersPage_Load;
        }

        private async void OrdersPage_Load(object sender, EventArgs e)
        {
            LoadStatuses();
            await LoadOrderSummaryAsync();
            await LoadOrdersAsync();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            BuildHeaderPanel();
            BuildStatsPanel();
            BuildFilterPanel();
            BuildGridPanel();
            BuildFooterPanel();

            Controls.Add(gridPanel);
            Controls.Add(footerPanel);
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
                Height = AdminModu ? 58 : 0,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = AdminModu ? "Siparişler" : "Verilen Siparişler",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            lblSubtitle = new Label
            {
                Text = AdminModu
                    ? "Sipariş kayıtlarını görüntüleyin, detayları inceleyin ve durumlarını yönetin."
                    : "Merkez tarafından hazırlanan siparişlerinizin durumunu takip edin.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 38)
            };

            btnNewOrder = new Button
            {
                Text = "+ Yeni Sipariş",
                Width = 150,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnNewOrder.FlatAppearance.BorderSize = 0;
            ButtonStyleHelper.ApplyPrimary(btnNewOrder);
            btnNewOrder.Click += BtnNewOrder_Click;

            if (AdminModu)
                headerPanel.Controls.Add(btnNewOrder);

            headerPanel.Resize += (s, e) =>
            {
                if (AdminModu)
                    btnNewOrder.Location = new Point(headerPanel.Width - btnNewOrder.Width, 6);
            };
        }

        private void BuildStatsPanel()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 142,
                BackColor = AppColors.Background,
                Padding = new Padding(0, 0, 0, 18)
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
            cPreparing = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cShipped = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cDelivered = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cCancelled = new CategoriesCard { Dock = DockStyle.Fill, Margin = Padding.Empty };

            cTotal.SetData("🧾", "Toplam", "0");
            cPreparing.SetData("🟠", "Hazırlanıyor", "0");
            cShipped.SetData("🚚", "Kargoda", "0");
            cDelivered.SetData("✅", AdminModu ? "Teslim" : "Teslim Alındı", "0");
            cCancelled.SetData("❌", "İptal", "0");

            grid.Controls.Add(cTotal, 0, 0);
            grid.Controls.Add(cPreparing, 1, 0);
            grid.Controls.Add(cShipped, 2, 0);
            grid.Controls.Add(cDelivered, 3, 0);
            grid.Controls.Add(cCancelled, 4, 0);

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
                Width = 300,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(16, 20)
            };

            cmbFilterStatus = new ComboBox
            {
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
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
            ButtonStyleHelper.ApplyPrimary(btnSearch);

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
            ButtonStyleHelper.ApplyOutline(btnClear);

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
            DataGridViewStyleHelper.UpdateCountLabel(lblInfo, 0, 0);

            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            cmbFilterStatus.SelectedIndexChanged += CmbFilterStatus_SelectedIndexChanged;
            btnSearch.Click += BtnSearch_Click;
            btnClear.Click += BtnClear_Click;

            filterPanel.Controls.Add(txtSearch);
            filterPanel.Controls.Add(cmbFilterStatus);
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

            cmbFilterStatus.Location = new Point(x, y);
            x += cmbFilterStatus.Width + 14;

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

            dgvOrders = new DataGridView
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
                ScrollBars = ScrollBars.Both
            };

            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersHeight = 44;
            dgvOrders.RowTemplate.Height = 48;
            dgvOrders.GridColor = AppColors.Border;
            dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvOrders.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            dgvOrders.ColumnHeadersDefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            dgvOrders.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvOrders.DefaultCellStyle.BackColor = AppColors.CardBackground;
            dgvOrders.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            dgvOrders.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            dgvOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            DataGridViewStyleHelper.ApplyModernGrid(dgvOrders);

            ConfigureOrderGridColumns();

            dgvOrders.CellClick += DgvOrders_CellClick;
            dgvOrders.CellContentClick += DgvOrders_CellContentClick;
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;
            dgvOrders.CellPainting += DgvOrders_CellPainting;
            dgvOrders.CellMouseMove += DgvOrders_CellMouseMove;
            dgvOrders.CellDoubleClick += DgvOrders_CellDoubleClick;
            dgvOrders.DataBindingComplete += (sender, e) => RefreshOrderCountLabel();
            dgvOrders.MouseLeave += DgvOrders_MouseLeave;

            gridPanel.Controls.Add(dgvOrders);
        }

        private void ConfigureOrderGridColumns()
        {
            dgvOrders.Columns.Clear();

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SiparisId",
                DataPropertyName = "SiparisId",
                Visible = false
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UrunId",
                DataPropertyName = "UrunId",
                Visible = false
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SiparisNo",
                DataPropertyName = "SiparisId",
                HeaderText = AdminModu ? "ID" : "Sipariş",
                Width = 70
            });

            if (AdminModu)
            {
                dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "MusteriAdi",
                    DataPropertyName = "MusteriAdi",
                    HeaderText = "Bayi",
                    Width = 150
                });

                dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "MagazaAdi",
                    DataPropertyName = "MagazaAdi",
                    HeaderText = "Mağaza",
                    Width = 150
                });

                dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "YetkiliAdi",
                    DataPropertyName = "YetkiliAdi",
                    HeaderText = "Yetkili",
                    Width = 130
                });
            }
            else
            {
                dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "MagazaAdi",
                    DataPropertyName = "MagazaAdi",
                    HeaderText = "Mağaza",
                    Width = 170
                });
            }

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UrunAdi",
                DataPropertyName = "UrunAdi",
                HeaderText = "Ürün",
                Width = 180
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Adet",
                DataPropertyName = "Adet",
                HeaderText = "Adet",
                Width = 65,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "BayiStok",
                DataPropertyName = "BayiStok",
                HeaderText = "Bayi Stok",
                Width = 95,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ToplamTutar",
                DataPropertyName = "ToplamTutar",
                HeaderText = "Tutar",
                Width = 105,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SiparisDurumu",
                DataPropertyName = "SiparisDurumu",
                HeaderText = "Durum",
                Width = 125
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderSource",
                DataPropertyName = "OrderSource",
                HeaderText = "Kaynak",
                Width = 90
            });

            dgvOrders.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SiparisTarihi",
                DataPropertyName = "SiparisTarihi",
                HeaderText = "Tarih",
                Width = 145
            });

            dgvOrders.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colDetail",
                HeaderText = "",
                Text = "Detay",
                UseColumnTextForButtonValue = true,
                Width = 88
            });

            ApplyGridColumnSizing();
        }

        private void ApplyGridColumnSizing()
        {
            SetFill("SiparisNo", 7, 52);
            SetFill("MusteriAdi", 14, 95);
            SetFill("MagazaAdi", 13, 95);
            SetFill("YetkiliAdi", 11, 88);
            SetFill("UrunAdi", 15, 105);
            SetFill("Adet", 6, 46);
            SetFill("BayiStok", 7, 62);
            SetFill("ToplamTutar", 9, 78);
            SetFill("SiparisDurumu", 11, 82);
            SetFill("OrderSource", 8, 68);
            SetFill("SiparisTarihi", 11, 92);
            SetFill("colDetail", 8, 64);
        }

        private void SetFill(string columnName, float fillWeight, int minWidth)
        {
            if (!dgvOrders.Columns.Contains(columnName))
                return;

            DataGridViewColumn column = dgvOrders.Columns[columnName];
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.FillWeight = fillWeight;
            column.MinimumWidth = minWidth;
        }

        private void BuildFooterPanel()
        {
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = AdminModu ? 96 : 82,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(16, 12, 16, 12)
            };

            lblSelectedOrder = new Label
            {
                Text = "Seçili Sipariş ID",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(16, 6)
            };

            txtOrderId = new TextBox
            {
                Location = new Point(16, 28),
                Width = 100,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10F)
            };

            cmbStatus = new ComboBox
            {
                Location = new Point(130, 28),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            btnUpdateStatus = new Button
            {
                Text = "Durum Güncelle",
                Width = 145,
                Height = 34,
                Location = new Point(318, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnUpdateStatus.FlatAppearance.BorderSize = 0;
            ButtonStyleHelper.ApplyPrimary(btnUpdateStatus);
            btnUpdateStatus.Click += BtnUpdateStatus_Click;

            btnOpenDetail = new Button
            {
                Text = "Detay Aç",
                Width = 110,
                Height = 34,
                Location = new Point(470, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = AppColors.TextSecondary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnOpenDetail.FlatAppearance.BorderColor = AppColors.Border;
            ButtonStyleHelper.ApplyOutline(btnOpenDetail);
            btnOpenDetail.Click += BtnOpenDetail_Click;

            lblSelectedSummary = new Label
            {
                Text = "Seçili sipariş yok",
                AutoSize = false,
                Height = 22,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Location = new Point(600, 12),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };

            lblSelectedBayi = CreateSelectedInfoLabel("Bayi: -", 600, 36);
            lblSelectedProduct = CreateSelectedInfoLabel("Ürün: -", 600, 58);
            lblSelectedAmount = CreateSelectedInfoLabel("Tutar: -", 840, 58);

            if (AdminModu)
            {
                footerPanel.Controls.Add(lblSelectedOrder);
                footerPanel.Controls.Add(txtOrderId);
                footerPanel.Controls.Add(cmbStatus);
                footerPanel.Controls.Add(btnUpdateStatus);
            }
            else
            {
                btnOpenDetail.Location = new Point(16, 14);
                btnOpenDetail.Text = "Seçili Sipariş Detayı";
                btnOpenDetail.Width = 160;
                lblSelectedSummary.Location = new Point(196, 14);
                lblSelectedBayi.Location = new Point(196, 38);
                lblSelectedProduct.Location = new Point(196, 60);
                lblSelectedAmount.Location = new Point(436, 60);
            }

            footerPanel.Controls.Add(lblSelectedSummary);
            footerPanel.Controls.Add(lblSelectedBayi);
            footerPanel.Controls.Add(lblSelectedProduct);
            footerPanel.Controls.Add(lblSelectedAmount);
            footerPanel.Controls.Add(btnOpenDetail);
            footerPanel.Resize += (sender, e) => PlaceFooterSummary();
            PlaceFooterSummary();
        }

        private void PlaceFooterSummary()
        {
            if (footerPanel == null || lblSelectedSummary == null)
                return;

            int summaryLeft = AdminModu ? Math.Min(600, footerPanel.Width / 2 + 40) : 196;
            int usableWidth = Math.Max(260, footerPanel.Width - summaryLeft - 24);

            lblSelectedSummary.Location = new Point(summaryLeft, 12);
            lblSelectedSummary.Width = usableWidth;
            lblSelectedBayi.Location = new Point(summaryLeft, 36);
            lblSelectedBayi.Width = usableWidth;
            lblSelectedProduct.Location = new Point(summaryLeft, 58);
            lblSelectedProduct.Width = Math.Max(180, usableWidth - 230);
            lblSelectedAmount.Location = new Point(summaryLeft + Math.Max(190, usableWidth - 220), 58);
            lblSelectedAmount.Width = Math.Min(220, usableWidth);
        }

        private Label CreateSelectedInfoLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Width = 230,
                Height = 20,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppColors.TextSecondary,
                AutoEllipsis = true
            };
        }

        private void LoadStatuses()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add(new StatusItem("Hazırlanıyor", "Hazirlaniyor"));
            cmbStatus.Items.Add(new StatusItem("Kargoda", "Kargoda"));
            cmbStatus.Items.Add(new StatusItem("Teslim Edildi", "Teslim Edildi"));
            cmbStatus.Items.Add(new StatusItem("İptal", "Iptal"));
            cmbStatus.SelectedIndex = 0;

            cmbFilterStatus.Items.Clear();
            cmbFilterStatus.Items.Add(new StatusItem("Tüm Durumlar", ""));
            cmbFilterStatus.Items.Add(new StatusItem("Hazırlanıyor", "Hazirlaniyor"));
            cmbFilterStatus.Items.Add(new StatusItem("Kargoda", "Kargoda"));
            cmbFilterStatus.Items.Add(new StatusItem("Teslim Edildi", "Teslim Edildi"));
            cmbFilterStatus.Items.Add(new StatusItem("İptal", "Iptal"));
            cmbFilterStatus.SelectedIndex = 0;
        }


        private int? GetCurrentMagazaId()
        {
            if (AdminModu && AppSession.TumMagazalar)
                return null;

            return AppSession.SeciliMagazaId;
        }

        private bool IsTumMagazalar()
        {
            return AdminModu && (AppSession.TumMagazalar || !AppSession.SeciliMagazaId.HasValue);
        }
        private async Task LoadOrderSummaryAsync()
        {
            try
            {
                OrderSummaryView summary = await _apiClient.GetOrderSummaryAsync(
                    GetCurrentMagazaId(),
                    IsTumMagazalar()
                );

                cTotal.SetData("🧾", "Toplam", summary.TotalOrders.ToString());
                cPreparing.SetData("🟠", "Hazırlanıyor", summary.PreparingOrders.ToString());
                cShipped.SetData("🚚", "Kargoda", summary.ShippedOrders.ToString());
                cDelivered.SetData("✅", AdminModu ? "Teslim" : "Teslim Alındı", summary.DeliveredOrders.ToString());
                cCancelled.SetData("❌", "İptal", summary.CancelledOrders.ToString());
            }
            catch
            {
                cTotal.SetData("🧾", "Toplam", "0");
                cPreparing.SetData("🟠", "Hazırlanıyor", "0");
                cShipped.SetData("🚚", "Kargoda", "0");
                cDelivered.SetData("✅", AdminModu ? "Teslim" : "Teslim Alındı", "0");
                cCancelled.SetData("❌", "İptal", "0");
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                ordersTable = await GetOrdersAsync(
                    GetCurrentMagazaId(),
                    IsTumMagazalar()
                );

                ConfigureOrderGridColumns();
                ApplySearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnNewOrder_Click(object sender, EventArgs e)
        {
            using (OrderModalForm frm = new OrderModalForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await LoadOrderSummaryAsync();
                    await LoadOrdersAsync();
                    ApplySearch();
                    ClearSelectedOrder();
                }
            }
        }

        private void DgvOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            SelectOrderFromRow(e.RowIndex);
        }

        private void DgvOrders_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvOrders.Columns[e.ColumnIndex].Name;

            if (columnName == "colDetail")
                return;

            SelectOrderFromRow(e.RowIndex);
            OpenOrderDetail(e.RowIndex);
        }

        private void DgvOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvOrders.Columns[e.ColumnIndex].Name;

            if (columnName == "colDetail")
            {
                SelectOrderFromRow(e.RowIndex);
                OpenOrderDetail(e.RowIndex);
            }
        }

        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvOrders.Columns[e.ColumnIndex].Name;

            if (columnName == "SiparisTarihi" && e.Value != null && e.Value != DBNull.Value)
            {
                DateTime dt = Convert.ToDateTime(e.Value);
                e.Value = dt.ToString("dd.MM.yyyy HH:mm");
                e.FormattingApplied = true;
            }

            if (columnName == "ToplamTutar" && e.Value != null && e.Value != DBNull.Value)
            {
                decimal value;

                if (decimal.TryParse(e.Value.ToString(), out value))
                {
                    e.Value = value.ToString("N2", new CultureInfo("tr-TR")) + " ₺";
                    e.FormattingApplied = true;
                }
            }

            if ((columnName == "MusteriAdi" || columnName == "MagazaAdi" || columnName == "YetkiliAdi") && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.Length > 18)
                {
                    e.Value = text.Substring(0, 18) + "...";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "UrunAdi" && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.Length > 22)
                {
                    e.Value = text.Substring(0, 22) + "...";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "BayiStok" && e.Value != null && e.Value != DBNull.Value)
            {
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                e.CellStyle.ForeColor = AppColors.Primary;
            }

            if (columnName == "OrderSource" && e.Value != null)
            {
                string text = e.Value.ToString();
                e.Value = text == "AdminPanel" ? "Admin" : text;
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                e.FormattingApplied = true;
            }
        }

        private Task<DataTable> GetOrdersAsync(int? magazaId, bool tumMagazalar)
        {
            return _apiClient.GetOrdersAsync(magazaId, tumMagazalar);
        }

        private void DgvOrders_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            int newRow = -1;
            int newCol = -1;

            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                string columnName = dgvOrders.Columns[e.ColumnIndex].Name;

                if (columnName == "colDetail")
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
                    dgvOrders.InvalidateCell(oldCol, oldRow);

                if (hoveredRowIndex >= 0 && hoveredColumnIndex >= 0)
                    dgvOrders.InvalidateCell(hoveredColumnIndex, hoveredRowIndex);
            }
        }

        private void DgvOrders_MouseLeave(object sender, EventArgs e)
        {
            int oldRow = hoveredRowIndex;
            int oldCol = hoveredColumnIndex;

            hoveredRowIndex = -1;
            hoveredColumnIndex = -1;

            if (oldRow >= 0 && oldCol >= 0)
                dgvOrders.InvalidateCell(oldCol, oldRow);
        }

        private void DgvOrders_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvOrders.Columns[e.ColumnIndex].Name;

            if (columnName == "SiparisDurumu")
            {
                PaintStatusBadge(e);
                return;
            }

            if (columnName == "colDetail")
            {
                PaintDetailButton(e);
                return;
            }
        }

        private void PaintStatusBadge(DataGridViewCellPaintingEventArgs e)
        {
            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            string text = e.Value?.ToString() ?? "";

            Color backColor = Color.FromArgb(243, 244, 246);
            Color foreColor = Color.FromArgb(75, 85, 99);

            if (text == "Hazirlaniyor")
            {
                backColor = Color.FromArgb(255, 247, 237);
                foreColor = Color.FromArgb(194, 65, 12);
            }
            else if (text == "Kargoda")
            {
                backColor = Color.FromArgb(239, 246, 255);
                foreColor = Color.FromArgb(29, 78, 216);
            }
            else if (text == "Teslim Edildi")
            {
                backColor = Color.FromArgb(236, 253, 245);
                foreColor = Color.FromArgb(21, 128, 61);
            }
            else if (text == "Iptal")
            {
                backColor = Color.FromArgb(254, 242, 242);
                foreColor = Color.FromArgb(185, 28, 28);
            }

            string displayText = GetStatusDisplayText(text);

            Rectangle badgeRect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - 102) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                102,
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
                e.Graphics.DrawString(displayText, font, textBrush, badgeRect, sf);
            }
        }

        private void PaintDetailButton(DataGridViewCellPaintingEventArgs e)
        {
            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            bool isHovered = e.RowIndex == hoveredRowIndex && e.ColumnIndex == hoveredColumnIndex;

            Color baseColor = AppColors.Primary;
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
                e.Graphics.DrawString("Detay", font, textBrush, buttonRect, sf);
            }
        }

        private void SelectOrderFromRow(int rowIndex)
        {
            if (rowIndex < 0)
                return;

            DataGridViewRow row = dgvOrders.Rows[rowIndex];

            txtOrderId.Text = row.Cells["SiparisId"].Value?.ToString();

            string currentStatus = row.Cells["SiparisDurumu"].Value?.ToString();

            if (!string.IsNullOrWhiteSpace(currentStatus))
                SelectStatusInCombo(cmbStatus, currentStatus);

            UpdateSelectedOrderSummary(row);
        }

        private void UpdateSelectedOrderSummary(DataGridViewRow row)
        {
            if (row == null)
            {
                ClearSelectedOrderSummary();
                return;
            }

            string orderId = GetGridCellText(row, "SiparisId", "-");
            string status = GetStatusDisplayText(GetGridCellText(row, "SiparisDurumu", "-"));
            string bayi = GetGridCellText(row, "MusteriAdi", "-");
            string magaza = GetGridCellText(row, "MagazaAdi", "-");
            string urun = GetGridCellText(row, "UrunAdi", "-");
            string adet = GetGridCellText(row, "Adet", "0");
            string tutar = GetMoneyDisplay(GetGridCellText(row, "ToplamTutar", "0"));

            lblSelectedSummary.Text = "Sipariş #" + orderId + " / " + status;
            lblSelectedBayi.Text = "Bayi: " + bayi + " / " + magaza;
            lblSelectedProduct.Text = "Ürün: " + urun + " (" + adet + " adet)";
            lblSelectedAmount.Text = "Tutar: " + tutar;
        }

        private string GetGridCellText(DataGridViewRow row, string columnName, string defaultValue)
        {
            if (row == null || !dgvOrders.Columns.Contains(columnName))
                return defaultValue;

            object value = row.Cells[columnName].Value;

            if (value == null || value == DBNull.Value)
                return defaultValue;

            string text = Convert.ToString(value);
            return string.IsNullOrWhiteSpace(text) ? defaultValue : text;
        }

        private string GetMoneyDisplay(string value)
        {
            decimal amount;

            if (!decimal.TryParse(value, out amount))
                return value;

            return amount.ToString("N2", new CultureInfo("tr-TR")) + " TL";
        }

        private void SelectStatusInCombo(ComboBox comboBox, string value)
        {
            foreach (object item in comboBox.Items)
            {
                StatusItem statusItem = item as StatusItem;

                if (statusItem != null && statusItem.Value == value)
                {
                    comboBox.SelectedItem = statusItem;
                    return;
                }
            }
        }

        private async void BtnUpdateStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtOrderId.Text))
                {
                    MessageBox.Show("Lütfen bir sipariş seçin.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int orderId = int.Parse(txtOrderId.Text);
                string newStatus = GetSelectedStatusValue(cmbStatus);
                string currentStatus = GetSelectedOrderStatus();

                if (string.IsNullOrWhiteSpace(newStatus))
                {
                    MessageBox.Show("Lütfen durum seçin.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!CanChangeStatus(currentStatus, newStatus))
                    return;

                btnUpdateStatus.Enabled = false;
                await UpdateOrderStatusAsync(orderId, newStatus);

                MessageBox.Show("Sipariş durumu güncellendi.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                await LoadOrderSummaryAsync();
                await LoadOrdersAsync();
                ApplySearch();
                ClearSelectedOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş durumu güncellenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpdateStatus.Enabled = true;
            }
        }

        private bool CanChangeStatus(string currentStatus, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(currentStatus) || currentStatus == newStatus)
                return true;

            if (currentStatus == "Iptal")
            {
                MessageBox.Show("İptal edilen sipariş tekrar işleme alınamaz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (currentStatus == "Kargoda" && newStatus == "Hazirlaniyor")
            {
                MessageBox.Show("Kargodaki sipariş hazırlık durumuna geri alınamaz.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (currentStatus == "Teslim Edildi" && newStatus != "Iptal")
            {
                MessageBox.Show("Teslim edilen sipariş yalnızca iptal/iade sürecine alınabilir.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (newStatus == "Iptal")
            {
                string message = currentStatus == "Teslim Edildi"
                    ? "Teslim edilmiş sipariş iptal edilecek. Bayi stoğundan düşülüp merkez stoğa iade edilecek. Devam edilsin mi?"
                    : "Sipariş iptal edilecek ve ayrılan ürün merkez stoğa iade edilecek. Devam edilsin mi?";

                DialogResult result = MessageBox.Show(
                    message,
                    "Sipariş İptali",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                return result == DialogResult.Yes;
            }

            return true;
        }

        private Task UpdateOrderStatusAsync(int orderId, string status)
        {
            return _apiClient.UpdateOrderStatusAsync(orderId, status);
        }

        private void BtnOpenDetail_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null)
            {
                MessageBox.Show("Lütfen bir sipariş seçin.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenOrderDetail(dgvOrders.CurrentRow.Index);
        }

        private void OpenOrderDetail(int rowIndex)
        {
            try
            {
                DataRow row = GetDataRowFromGridRow(rowIndex);

                if (row == null)
                {
                    MessageBox.Show("Sipariş detayı bulunamadı.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (OrderDetailForm frm = new OrderDetailForm(row))
                {
                    frm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş detayı açılırken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataRow GetDataRowFromGridRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvOrders.Rows.Count)
                return null;

            DataGridViewRow gridRow = dgvOrders.Rows[rowIndex];

            DataRowView rowView = gridRow.DataBoundItem as DataRowView;

            if (rowView == null)
                return null;

            return rowView.Row;
        }

        private void ApplySearch()
        {
            try
            {
                if (ordersTable == null)
                {
                    dgvOrders.DataSource = null;
                    UpdateInfoLabel(0, 0);
                    return;
                }

                string search = txtSearch.Text.Trim().Replace("'", "''");
                string selectedStatus = GetSelectedStatusValue(cmbFilterStatus);

                DataView view = ordersTable.DefaultView;

                string searchFilter = BuildSearchFilter(search);
                string statusFilter = BuildStatusFilter(selectedStatus);

                if (!string.IsNullOrWhiteSpace(searchFilter) && !string.IsNullOrWhiteSpace(statusFilter))
                    view.RowFilter = "(" + searchFilter + ") AND " + statusFilter;
                else if (!string.IsNullOrWhiteSpace(searchFilter))
                    view.RowFilter = searchFilter;
                else if (!string.IsNullOrWhiteSpace(statusFilter))
                    view.RowFilter = statusFilter;
                else
                    view.RowFilter = string.Empty;

                dgvOrders.DataSource = view;
                RefreshOrderCountLabel();
                dgvOrders.BeginInvoke(new Action(RefreshOrderCountLabel));
            }
            catch
            {
                RefreshOrderCountLabel();
            }
        }

        private string BuildSearchFilter(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return string.Empty;

            string filter = string.Empty;

            AddSearchColumnFilter(ref filter, "MusteriAdi", search);
            AddSearchColumnFilter(ref filter, "MagazaAdi", search);
            AddSearchColumnFilter(ref filter, "YetkiliAdi", search);
            AddSearchColumnFilter(ref filter, "MusteriEmail", search);
            AddSearchColumnFilter(ref filter, "MusteriTelefon", search);
            AddSearchColumnFilter(ref filter, "UrunAdi", search);
            AddSearchColumnFilter(ref filter, "SiparisId", search);

            return filter;
        }

        private void AddSearchColumnFilter(ref string filter, string columnName, string search)
        {
            if (ordersTable == null)
                return;

            if (!ordersTable.Columns.Contains(columnName))
                return;

            string condition = $"Convert({columnName}, 'System.String') LIKE '%{search}%'";

            if (string.IsNullOrWhiteSpace(filter))
                filter = condition;
            else
                filter += " OR " + condition;
        }

        private string BuildStatusFilter(string selectedStatus)
        {
            if (string.IsNullOrWhiteSpace(selectedStatus))
                return string.Empty;

            if (ordersTable == null || !ordersTable.Columns.Contains("SiparisDurumu"))
                return string.Empty;

            selectedStatus = selectedStatus.Replace("'", "''");

            return $"SiparisDurumu = '{selectedStatus}'";
        }

        private int GetCurrentRowCount()
        {
            DataView view = ordersTable?.DefaultView;

            if (view == null)
                return 0;

            return view.Count;
        }

        private int GetVisibleOrderCount()
        {
            if (dgvOrders != null)
            {
                int rowCount = 0;
                foreach (DataGridViewRow row in dgvOrders.Rows)
                {
                    if (!row.IsNewRow)
                        rowCount++;
                }

                if (rowCount > 0)
                    return rowCount;
            }

            if (dgvOrders != null && dgvOrders.DataSource != null)
                return dgvOrders.Rows.Count;

            return GetCurrentRowCount();
        }

        private void RefreshOrderCountLabel()
        {
            UpdateInfoLabel(GetVisibleOrderCount(), ordersTable == null ? GetVisibleOrderCount() : ordersTable.Rows.Count);
        }

        private void UpdateInfoLabel(int displayCount, int totalCount)
        {
            if (lblInfo == null)
                return;

            DataGridViewStyleHelper.UpdateCountLabel(lblInfo, displayCount, totalCount);
        }

        private string GetSelectedStatusValue(ComboBox comboBox)
        {
            StatusItem selected = comboBox.SelectedItem as StatusItem;

            if (selected == null)
                return string.Empty;

            return selected.Value;
        }

        private string GetSelectedOrderStatus()
        {
            if (dgvOrders == null || dgvOrders.CurrentRow == null)
                return string.Empty;

            if (!dgvOrders.Columns.Contains("SiparisDurumu"))
                return string.Empty;

            object value = dgvOrders.CurrentRow.Cells["SiparisDurumu"].Value;

            if (value == null || value == DBNull.Value)
                return string.Empty;

            return Convert.ToString(value);
        }

        private string GetStatusDisplayText(string status)
        {
            if (status == "Hazirlaniyor")
                return "Hazırlanıyor";

            if (status == "Iptal")
                return "İptal";

            return status;
        }

        private void ClearSelectedOrder()
        {
            txtOrderId.Clear();

            if (cmbStatus.Items.Count > 0)
                cmbStatus.SelectedIndex = 0;

            ClearSelectedOrderSummary();
        }

        private void ClearSelectedOrderSummary()
        {
            if (lblSelectedSummary != null)
                lblSelectedSummary.Text = "Seçili sipariş yok";

            if (lblSelectedBayi != null)
                lblSelectedBayi.Text = "Bayi: -";

            if (lblSelectedProduct != null)
                lblSelectedProduct.Text = "Ürün: -";

            if (lblSelectedAmount != null)
                lblSelectedAmount.Text = "Tutar: -";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            ApplySearch();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                searchTimer.Stop();
                ApplySearch();
                e.SuppressKeyPress = true;
            }
        }

        private void CmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplySearch();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            searchTimer.Stop();
            ApplySearch();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            searchTimer.Stop();

            txtSearch.Clear();

            if (cmbFilterStatus.Items.Count > 0)
                cmbFilterStatus.SelectedIndex = 0;

            ApplySearch();
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

            if (footerPanel != null)
                footerPanel.BackColor = AppColors.CardBackground;

            if (lblTitle != null)
                lblTitle.ForeColor = AppColors.TextPrimary;

            if (lblSubtitle != null)
                lblSubtitle.ForeColor = AppColors.TextSecondary;

            if (lblInfo != null)
                lblInfo.ForeColor = AppColors.TextSecondary;

            if (lblSelectedOrder != null)
                lblSelectedOrder.ForeColor = AppColors.TextSecondary;

            if (lblSelectedSummary != null)
                lblSelectedSummary.ForeColor = AppColors.TextPrimary;

            if (lblSelectedBayi != null)
                lblSelectedBayi.ForeColor = AppColors.TextSecondary;

            if (lblSelectedProduct != null)
                lblSelectedProduct.ForeColor = AppColors.TextSecondary;

            if (lblSelectedAmount != null)
                lblSelectedAmount.ForeColor = AppColors.TextSecondary;

            if (btnNewOrder != null)
            {
                btnNewOrder.BackColor = AppColors.Primary;
                btnNewOrder.ForeColor = Color.White;
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

            if (btnUpdateStatus != null)
            {
                btnUpdateStatus.BackColor = AppColors.Primary;
                btnUpdateStatus.ForeColor = Color.White;
            }

            if (btnOpenDetail != null)
            {
                btnOpenDetail.BackColor = AppColors.CardBackground;
                btnOpenDetail.ForeColor = AppColors.TextSecondary;
                btnOpenDetail.FlatAppearance.BorderColor = AppColors.Border;
            }

            if (dgvOrders != null)
            {
                dgvOrders.BackgroundColor = AppColors.CardBackground;
                dgvOrders.GridColor = AppColors.Border;
                dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvOrders.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
                dgvOrders.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            }

            Invalidate(true);
            Refresh();
        }

        private class StatusItem
        {
            public string Text { get; }
            public string Value { get; }

            public StatusItem(string text, string value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
