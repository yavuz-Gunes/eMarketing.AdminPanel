using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class ReportsPage : UserControl, IThemeable
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();
        private readonly CultureInfo _culture = new CultureInfo("tr-TR");

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblInfo;
        private Button btnRefresh;

        private CategoriesCard cRevenue;
        private CategoriesCard cOrders;
        private CategoriesCard cStores;
        private CategoriesCard cCriticalStock;

        private DataGridView dgvRecentOrders;
        private DataGridView dgvCriticalStock;

        public ReportsPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            BuildLayout();
            Load += ReportsPage_Load;
        }

        private async void ReportsPage_Load(object sender, EventArgs e)
        {
            await LoadReportAsync();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            Panel header = BuildHeader();
            Panel summary = BuildSummary();
            Panel content = BuildContent();

            Controls.Add(content);
            Controls.Add(summary);
            Controls.Add(header);

            ResumeLayout(true);
        }

        private Panel BuildHeader()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 82,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = "Raporlar",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            lblSubtitle = new Label
            {
                Text = "Satış, sipariş ve stok durumunu tek ekrandan izleyin.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 38)
            };

            lblInfo = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = AppColors.TextMuted,
                AutoSize = false,
                Width = 280,
                Height = 22,
                TextAlign = ContentAlignment.MiddleRight
            };

            btnRefresh = new Button
            {
                Text = "Yenile",
                Width = 110,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (sender, e) => await LoadReportAsync();

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSubtitle);
            header.Controls.Add(lblInfo);
            header.Controls.Add(btnRefresh);
            header.Resize += (sender, e) =>
            {
                btnRefresh.Location = new Point(header.Width - btnRefresh.Width, 6);
                lblInfo.Location = new Point(header.Width - btnRefresh.Width - lblInfo.Width - 14, 14);
            };

            return header;
        }

        private Panel BuildSummary()
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 126,
                BackColor = AppColors.Background
            };

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.Background
            };

            for (int i = 0; i < 4; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            cRevenue = CreateSummaryCard("₺", "Toplam Ciro");
            cOrders = CreateSummaryCard("🧾", "Toplam");
            cStores = CreateSummaryCard("🏬", "Aktif");
            cCriticalStock = CreateSummaryCard("!", "Kritik Stok");

            grid.Controls.Add(cRevenue, 0, 0);
            grid.Controls.Add(cOrders, 1, 0);
            grid.Controls.Add(cStores, 2, 0);
            grid.Controls.Add(cCriticalStock, 3, 0);

            panel.Controls.Add(grid);
            return panel;
        }

        private CategoriesCard CreateSummaryCard(string icon, string title)
        {
            CategoriesCard card = new CategoriesCard
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 16, 0)
            };
            card.SetData(icon, title, "0");
            return card;
        }

        private Panel BuildContent()
        {
            Panel wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background
            };

            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppColors.Background
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            dgvRecentOrders = CreateGrid();
            dgvCriticalStock = CreateGrid();

            grid.Controls.Add(CreateGridCard("Son Siparişler", "Bayi, ürün, tutar ve durum özeti", dgvRecentOrders), 0, 0);
            grid.Controls.Add(CreateGridCard("Kritik Stok", "Merkez ve bayi stok uyarıları", dgvCriticalStock), 1, 0);

            wrapper.Controls.Add(grid);
            return wrapper;
        }

        private Panel CreateGridCard(string title, string subtitle, DataGridView grid)
        {
            ShadowPanel panel = new ShadowPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 16, 0),
                Padding = new Padding(18, 14, 18, 18),
                BackColor = AppColors.CardBackground,
                BorderColor = AppColors.Border,
                CornerRadius = 12,
                ShadowSize = 4
            };

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            Label subtitleLabel = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            grid.Dock = DockStyle.Fill;

            panel.Controls.Add(grid);
            panel.Controls.Add(subtitleLabel);
            panel.Controls.Add(titleLabel);
            return panel;
        }

        private DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView
            {
                AutoGenerateColumns = true,
                ReadOnly = true,
                Dock = DockStyle.Fill
            };
            DataGridTheme.Apply(grid);
            grid.CellFormatting += Grid_CellFormatting;
            return grid;
        }

        private async Task LoadReportAsync()
        {
            btnRefresh.Enabled = false;
            lblInfo.Text = "Veriler yükleniyor...";

            try
            {
                int? magazaId = GetCurrentMagazaId();
                bool tumMagazalar = IsTumMagazalar();

                DashboardSummaryView summary = await _apiClient.GetDashboardSummaryAsync(magazaId, tumMagazalar);
                DataTable recentOrders = await _apiClient.GetDashboardRecentOrdersAsync(magazaId, tumMagazalar);
                DataTable criticalStock = await _apiClient.GetDashboardCriticalStockAsync(magazaId, tumMagazalar);

                cRevenue.SetData("₺", "Toplam Ciro", summary.TotalRevenue.ToString("N2", _culture));
                cOrders.SetData("🧾", "Toplam", summary.TotalOrders.ToString());
                cStores.SetData("🏬", "Aktif", summary.ActiveStores.ToString());
                cCriticalStock.SetData("!", "Kritik Stok", summary.LowStockProducts.ToString());

                dgvRecentOrders.DataSource = recentOrders;
                dgvCriticalStock.DataSource = criticalStock;
                FitGrid(dgvRecentOrders);
                FitGrid(dgvCriticalStock);

                lblInfo.Text = "Son güncelleme: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            }
            catch (Exception ex)
            {
                dgvRecentOrders.DataSource = null;
                dgvCriticalStock.DataSource = null;
                lblInfo.Text = "API bağlantısı kurulamadı.";
                MessageBox.Show(ex.Message, "Bağlantı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private void FitGrid(DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                column.MinimumWidth = 72;
            }
        }

        private void Grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid == null || e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null || e.Value == DBNull.Value)
                return;

            string columnName = grid.Columns[e.ColumnIndex].Name;
            if (columnName.IndexOf("Tarih", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DateTime date;
                if (DateTime.TryParse(Convert.ToString(e.Value), out date))
                {
                    e.Value = date.ToString("dd.MM.yyyy HH:mm");
                    e.FormattingApplied = true;
                }
            }

            if (columnName.IndexOf("Tutar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                columnName.IndexOf("Ciro", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                decimal money;
                if (decimal.TryParse(Convert.ToString(e.Value), out money))
                {
                    e.Value = money.ToString("N2", _culture) + " ₺";
                    e.FormattingApplied = true;
                }
            }
        }

        private int? GetCurrentMagazaId()
        {
            if (AppSession.AdminMi && AppSession.TumMagazalar)
                return null;

            return AppSession.SeciliMagazaId;
        }

        private bool IsTumMagazalar()
        {
            return AppSession.AdminMi && (AppSession.TumMagazalar || !AppSession.SeciliMagazaId.HasValue);
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;

            foreach (Control control in Controls)
                ApplyThemeRecursive(control);
        }

        private void ApplyThemeRecursive(Control control)
        {
            if (control is Panel || control is UserControl)
                control.BackColor = AppColors.Background;

            if (control is ShadowPanel)
                control.BackColor = AppColors.CardBackground;

            if (control is Label label)
            {
                if (label == lblTitle)
                    label.ForeColor = AppColors.TextPrimary;
                else
                    label.ForeColor = AppColors.TextSecondary;
            }

            if (control is DataGridView grid)
                DataGridTheme.Apply(grid);

            if (control is Button button)
            {
                button.BackColor = AppColors.Primary;
                button.ForeColor = Color.White;
            }

            foreach (Control child in control.Controls)
                ApplyThemeRecursive(child);
        }
    }
}
