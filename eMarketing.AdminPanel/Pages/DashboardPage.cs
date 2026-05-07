using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.Data.Models;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Pages
{
    public class DashboardPage : UserControl, IThemeable
    {
        private readonly DashboardRepository _repo = new DashboardRepository();

        private TableLayoutPanel mainLayout;
        private TableLayoutPanel cardsGrid;
        private TableLayoutPanel bodyGrid;

        private CardControl cTotalRevenue;
        private CardControl cTotalOrders;
        private CardControl cActiveStores;
        private CardControl cTotalCustomers;
        private CardControl cPendingPayments;
        private CardControl cLowStock;
        private CardControl cPreparingOrders;
        private CardControl cShippedOrders;
        private CardControl cDeliveredOrders;

        private Panel bodyArea;

        private ShadowPanel recentOrdersPanel;
        private ShadowPanel criticalStockPanel;

        private Label lblRecentOrdersTitle;
        private Label lblRecentOrdersSubTitle;
        private Label lblCriticalStockTitle;
        private Label lblCriticalStockSubTitle;

        private FlowLayoutPanel recentOrdersList;
        private FlowLayoutPanel criticalStockList;

        public DashboardPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            BuildLayout();
            Load += DashboardPage_Load;
        }

        private void BuildLayout()
        {
            SuspendLayout();

            BuildCardsArea();
            BuildBodyArea();

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppColors.Background,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 430F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cardsGrid.Dock = DockStyle.Fill;
            bodyArea.Dock = DockStyle.Fill;
            bodyArea.Padding = new Padding(0, 10, 0, 0);

            mainLayout.Controls.Add(cardsGrid, 0, 0);
            mainLayout.Controls.Add(bodyArea, 0, 1);

            Controls.Clear();
            Controls.Add(mainLayout);

            ResumeLayout(true);
        }

        private void BuildCardsArea()
        {
            cardsGrid = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 3,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            cardsGrid.ColumnStyles.Clear();
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            cardsGrid.RowStyles.Clear();
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

            cTotalRevenue = CreateCard("₺ Toplam Ciro", "0,00 TL", new Padding(0, 0, 18, 18));
            cTotalOrders = CreateCard("🧾 Toplam Sipariş", "0", new Padding(0, 0, 18, 18));
            cActiveStores = CreateCard("⌂ Aktif Mağaza", "0", new Padding(0, 0, 0, 18));

            cTotalCustomers = CreateCard("◇ Müşteri", "0", new Padding(0, 0, 18, 18));
            cPendingPayments = CreateCard("◷ Bekleyen Ödeme", "0", new Padding(0, 0, 18, 18));
            cLowStock = CreateCard("⚠ Kritik Stok", "0", new Padding(0, 0, 0, 18));

            cPreparingOrders = CreateCard("◔ Hazırlanıyor", "0", new Padding(0, 0, 18, 0));
            cShippedOrders = CreateCard("⇢ Kargoda", "0", new Padding(0, 0, 18, 0));
            cDeliveredOrders = CreateCard("✓ Teslim Edildi", "0", Padding.Empty);

            EnableCardNavigation(cTotalRevenue, "Reports");
            EnableCardNavigation(cTotalOrders, "Orders");
            EnableCardNavigation(cActiveStores, "Stores");
            EnableCardNavigation(cTotalCustomers, "Customers");
            EnableCardNavigation(cPendingPayments, "Orders");
            EnableCardNavigation(cLowStock, "DealerStock");
            EnableCardNavigation(cPreparingOrders, "Orders");
            EnableCardNavigation(cShippedOrders, "Orders");
            EnableCardNavigation(cDeliveredOrders, "Orders");

            cardsGrid.Controls.Add(cTotalRevenue, 0, 0);
            cardsGrid.Controls.Add(cTotalOrders, 1, 0);
            cardsGrid.Controls.Add(cActiveStores, 2, 0);

            cardsGrid.Controls.Add(cTotalCustomers, 0, 1);
            cardsGrid.Controls.Add(cPendingPayments, 1, 1);
            cardsGrid.Controls.Add(cLowStock, 2, 1);

            cardsGrid.Controls.Add(cPreparingOrders, 0, 2);
            cardsGrid.Controls.Add(cShippedOrders, 1, 2);
            cardsGrid.Controls.Add(cDeliveredOrders, 2, 2);
        }

        private void BuildBodyArea()
        {
            bodyArea = new Panel
            {
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            bodyGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            bodyGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            bodyGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            bodyGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            recentOrdersPanel = CreateSectionPanel(new Padding(0, 0, 18, 0));
            criticalStockPanel = CreateSectionPanel(Padding.Empty);

            lblRecentOrdersTitle = CreateSectionTitle("🧾 Son Siparişler");
            lblRecentOrdersSubTitle = CreateSectionSubTitle("En son oluşturulan sipariş kayıtları");

            lblCriticalStockTitle = CreateSectionTitle("⚠ Kritik Stok Parçaları");
            lblCriticalStockSubTitle = CreateSectionSubTitle("Stok seviyesi düşük olan parçalar");

            recentOrdersList = CreateListPanel();
            criticalStockList = CreateListPanel();

            recentOrdersPanel.Controls.Add(recentOrdersList);
            recentOrdersPanel.Controls.Add(lblRecentOrdersSubTitle);
            recentOrdersPanel.Controls.Add(lblRecentOrdersTitle);

            criticalStockPanel.Controls.Add(criticalStockList);
            criticalStockPanel.Controls.Add(lblCriticalStockSubTitle);
            criticalStockPanel.Controls.Add(lblCriticalStockTitle);

            bodyGrid.Controls.Add(recentOrdersPanel, 0, 0);
            bodyGrid.Controls.Add(criticalStockPanel, 1, 0);

            bodyArea.Controls.Add(bodyGrid);
        }

        private CardControl CreateCard(string title, string value, Padding margin)
        {
            CardControl card = new CardControl
            {
                Dock = DockStyle.Fill,
                Margin = margin
            };

            card.SetData(title, value);
            return card;
        }

        private void EnableCardNavigation(Control card, string pageKey)
        {
            card.Cursor = Cursors.Hand;
            card.Click += (sender, e) => Navigate(pageKey);

            foreach (Control child in card.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += (sender, e) => Navigate(pageKey);
            }
        }

        private void Navigate(string pageKey)
        {
            FrmMain main = FindForm() as FrmMain;
            if (main != null)
                main.NavigateTo(pageKey);
        }

        private ShadowPanel CreateSectionPanel(Padding margin)
        {
            return new ShadowPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(24, 20, 24, 22),
                Margin = margin,
                ShadowColor = Color.FromArgb(24, 15, 23, 42),
                BorderColor = AppColors.Border,
                CornerRadius = 18,
                ShadowSize = 6
            };
        }

        private Label CreateSectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
        }

        private Label CreateSectionSubTitle(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
        }

        private FlowLayoutPanel CreateListPanel()
        {
            FlowLayoutPanel list = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(0, 14, 4, 0)
            };

            list.SizeChanged += (s, e) => FitListItemWidths(list);

            return list;
        }

        private void DashboardPage_Load(object sender, EventArgs e)
        {
            LoadDashboardSummary();
            LoadRecentOrders();
            LoadCriticalStockProducts();
        }
        private int? GetCurrentMagazaId()
        {
            if (AppSession.TumMagazalar)
                return null;

            return AppSession.SeciliMagazaId;
        }

        private bool IsTumMagazalar()
        {
            return AppSession.TumMagazalar || !AppSession.SeciliMagazaId.HasValue;
        }
        private void LoadDashboardSummary()
        {
            try
            {
                DashboardSummary summary = _repo.GetSummary(
                    GetCurrentMagazaId(),
                    IsTumMagazalar()
                );

                cTotalRevenue.SetData("₺ Toplam Ciro", FormatMoney(summary.TotalRevenue), GetScopeText("seçili mağaza cirosu"));
                cTotalOrders.SetData("🧾 Toplam Sipariş", summary.TotalOrders.ToString(), GetScopeText("sipariş adedi"));
                cActiveStores.SetData("⌂ Aktif Mağaza", summary.ActiveStores.ToString(), IsTumMagazalar() ? "aktif mağaza sayısı" : "seçili mağaza");

                cTotalCustomers.SetData("◇ Müşteri", summary.TotalCustomers.ToString(), GetScopeText("sipariş veren müşteri"));
                cPendingPayments.SetData("◷ Bekleyen Ödeme", summary.PendingPaymentOrders.ToString(), "ödeme bekleyen siparişler");
                cLowStock.SetData("⚠ Kritik Stok", summary.LowStockProducts.ToString(), "stok seviyesi düşük ürün");

                cPreparingOrders.SetData("◔ Hazırlanıyor", summary.PreparingOrders.ToString(), "operasyonda bekleyen sipariş");
                cShippedOrders.SetData("⇢ Kargoda", summary.ShippedOrders.ToString(), "sevkiyatta olan sipariş");
                cDeliveredOrders.SetData("✓ Teslim Edildi", summary.DeliveredOrders.ToString(), "tamamlanan sipariş");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Dashboard verileri çekilirken hata: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadRecentOrders()
        {
            try
            {
                recentOrdersList.Controls.Clear();

                DataTable table = _repo.GetRecentOrders(
                    GetCurrentMagazaId(),
                    IsTumMagazalar()
                );

                if (table.Rows.Count == 0)
                {
                    recentOrdersList.Controls.Add(CreateEmptyItem("Seçili mağaza için henüz sipariş bulunmuyor."));
                    FitListItemWidths(recentOrdersList);
                    return;
                }

                foreach (DataRow row in table.Rows)
                {
                    recentOrdersList.Controls.Add(CreateRecentOrderItem(row));
                }

                FitListItemWidths(recentOrdersList);
            }
            catch (Exception ex)
            {
                recentOrdersList.Controls.Clear();
                recentOrdersList.Controls.Add(CreateEmptyItem("Son siparişler yüklenemedi."));

                MessageBox.Show(
                    "Son siparişler yüklenirken hata: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadCriticalStockProducts()
        {
            try
            {
                criticalStockList.Controls.Clear();

                DataTable table = _repo.GetCriticalStockProducts(
                    GetCurrentMagazaId(),
                    IsTumMagazalar()
                );

                if (table.Rows.Count == 0)
                {
                    criticalStockList.Controls.Add(CreateEmptyItem("Kritik stokta parça bulunmuyor."));
                    FitListItemWidths(criticalStockList);
                    return;
                }

                foreach (DataRow row in table.Rows)
                {
                    criticalStockList.Controls.Add(CreateCriticalStockItem(row));
                }

                FitListItemWidths(criticalStockList);
            }
            catch (Exception ex)
            {
                criticalStockList.Controls.Clear();
                criticalStockList.Controls.Add(CreateEmptyItem("Kritik stok bilgisi yüklenemedi."));

                MessageBox.Show(
                    "Kritik stok parçaları yüklenirken hata: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private Panel CreateRecentOrderItem(DataRow row)
        {
            string customerName = GetText(row, "MusteriAdi", "Müşteri Yok");
            string productName = GetText(row, "UrunAdi", "Parça Yok");
            string quantity = GetText(row, "Adet", "0");
            string status = GetText(row, "SiparisDurumu", "-");
            string date = GetDateText(row, "SiparisTarihi");
            string total = GetMoneyText(row, "ToplamTutar");

            Panel item = CreateListItemBase(82);

            Label icon = CreateIconBox("🧾", AppColors.PrimarySoft, AppColors.Primary);
            icon.Location = new Point(12, 16);

            Label title = CreateLabel(customerName, 10F, FontStyle.Bold, AppColors.TextPrimary);
            title.Location = new Point(68, 12);
            title.Size = new Size(260, 22);

            Label detail = CreateLabel(productName + " • " + quantity + " adet", 8.5F, FontStyle.Regular, AppColors.TextSecondary);
            detail.Location = new Point(68, 35);
            detail.Size = new Size(270, 20);

            Label dateLabel = CreateLabel(date, 8F, FontStyle.Regular, AppColors.TextMuted);
            dateLabel.Location = new Point(68, 56);
            dateLabel.Size = new Size(180, 18);

            Label totalLabel = CreateLabel(total, 9.5F, FontStyle.Bold, AppColors.Primary);
            totalLabel.TextAlign = ContentAlignment.MiddleRight;
            totalLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            totalLabel.Location = new Point(item.Width - 160, 16);
            totalLabel.Size = new Size(135, 22);

            Label statusBadge = CreateStatusBadge(status);
            statusBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            statusBadge.Location = new Point(item.Width - 138, 48);

            item.Resize += (s, e) =>
            {
                title.Width = item.Width - 240;
                detail.Width = item.Width - 260;

                totalLabel.Location = new Point(item.Width - 160, 16);
                statusBadge.Location = new Point(item.Width - 138, 48);
            };

            item.MouseEnter += (s, e) => item.BackColor = GetListItemHoverColor();
            item.MouseLeave += (s, e) => item.BackColor = AppColors.Surface;

            item.Controls.Add(icon);
            item.Controls.Add(title);
            item.Controls.Add(detail);
            item.Controls.Add(dateLabel);
            item.Controls.Add(totalLabel);
            item.Controls.Add(statusBadge);

            AttachOrderClickEvent(item, row);

            return item;
        }

        private Panel CreateCriticalStockItem(DataRow row)
        {
            string productName = GetText(row, "UrunAdi", "Parça Yok");
            string categoryName = GetText(row, "KategoriAdi", "Kategori Yok");
            string stock = GetText(row, "Stok", "0");
            string price = GetMoneyText(row, "Fiyat");

            Panel item = CreateListItemBase(82);

            Label icon = CreateIconBox("⚠", AppColors.WarningSoft, AppColors.Warning);
            icon.Location = new Point(12, 16);

            Label title = CreateLabel(productName, 10F, FontStyle.Bold, AppColors.TextPrimary);
            title.Location = new Point(68, 12);
            title.Size = new Size(220, 22);

            Label detail = CreateLabel(categoryName, 8.5F, FontStyle.Regular, AppColors.TextSecondary);
            detail.Location = new Point(68, 35);
            detail.Size = new Size(220, 20);

            Label priceLabel = CreateLabel(price, 8.5F, FontStyle.Bold, AppColors.Primary);
            priceLabel.Location = new Point(68, 56);
            priceLabel.Size = new Size(160, 18);

            Label stockBadge = CreateSmallBadge("Stok: " + stock, AppColors.WarningSoft, AppColors.Warning);
            stockBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            stockBadge.Location = new Point(item.Width - 128, 28);

            item.Resize += (s, e) =>
            {
                title.Width = item.Width - 220;
                detail.Width = item.Width - 220;
                stockBadge.Location = new Point(item.Width - 128, 28);
            };

            item.MouseEnter += (s, e) => item.BackColor = GetListItemHoverColor();
            item.MouseLeave += (s, e) => item.BackColor = AppColors.Surface;

            item.Controls.Add(icon);
            item.Controls.Add(title);
            item.Controls.Add(detail);
            item.Controls.Add(priceLabel);
            item.Controls.Add(stockBadge);

            return item;
        }

        private Panel CreateEmptyItem(string message)
        {
            Panel item = CreateListItemBase(76);

            Label label = CreateLabel(message, 9F, FontStyle.Regular, AppColors.TextSecondary);
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;

            item.Controls.Add(label);

            return item;
        }

        private Panel CreateListItemBase(int height)
        {
            ShadowPanel item = new ShadowPanel
            {
                Width = 300,
                Height = height,
                BackColor = AppColors.Surface,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(10),
                BorderColor = AppColors.Border,
                CornerRadius = 12,
                ShadowSize = 0,
                ShadowColor = Color.Transparent
            };

            return item;
        }

        private Color GetListItemHoverColor()
        {
            if (AppColors.IsDarkMode)
                return Color.FromArgb(30, 39, 54);

            return Color.FromArgb(248, 250, 253);
        }

        private Label CreateIconBox(string text, Color backColor, Color foreColor)
        {
            return new Label
            {
                Text = text,
                Size = new Size(44, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 16F, FontStyle.Regular),
                BackColor = backColor,
                ForeColor = foreColor
            };
        }

        private Label CreateLabel(string text, float fontSize, FontStyle style, Color foreColor)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Font = new Font("Segoe UI", fontSize, style),
                ForeColor = foreColor,
                BackColor = Color.Transparent
            };
        }

        private Label CreateStatusBadge(string status)
        {
            string displayText = GetStatusDisplayText(status);

            Color backColor = AppColors.InfoSoft;
            Color foreColor = AppColors.Info;

            if (status == "Hazirlaniyor")
            {
                backColor = AppColors.WarningSoft;
                foreColor = AppColors.Warning;
            }
            else if (status == "Kargoda")
            {
                backColor = AppColors.InfoSoft;
                foreColor = AppColors.Info;
            }
            else if (status == "Teslim Edildi")
            {
                backColor = AppColors.SuccessSoft;
                foreColor = AppColors.Success;
            }
            else if (status == "Iptal")
            {
                backColor = AppColors.DangerSoft;
                foreColor = AppColors.Danger;
            }

            return CreateSmallBadge(displayText, backColor, foreColor);
        }

        private Label CreateSmallBadge(string text, Color backColor, Color foreColor)
        {
            return new Label
            {
                Text = text,
                Size = new Size(112, 26),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = foreColor,
                Padding = new Padding(4, 0, 4, 0)
            };
        }

        private void AttachOrderClickEvent(Control control, DataRow row)
        {
            control.Cursor = Cursors.Hand;
            control.Click += (s, e) => ShowOrderDetail(row);

            foreach (Control child in control.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += (s, e) => ShowOrderDetail(row);
            }
        }

        private void ShowOrderDetail(DataRow row)
        {
            using (OrderDetailForm frm = new OrderDetailForm(row))
            {
                frm.ShowDialog(this);
            }
        }

        private string GetStatusDisplayText(string status)
        {
            if (status == "Hazirlaniyor")
                return "Hazırlanıyor";

            if (status == "Iptal")
                return "İptal";

            return status;
        }

        private string GetText(DataRow row, string columnName, string defaultValue)
        {
            if (!row.Table.Columns.Contains(columnName))
                return defaultValue;

            if (row[columnName] == DBNull.Value)
                return defaultValue;

            string value = row[columnName]?.ToString();

            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private string GetDateText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return "-";

            if (row[columnName] == DBNull.Value)
                return "-";

            DateTime date = Convert.ToDateTime(row[columnName]);
            return date.ToString("dd.MM.yyyy HH:mm");
        }

        private string GetMoneyText(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return "0,00 TL";

            if (row[columnName] == DBNull.Value)
                return "0,00 TL";

            decimal value = Convert.ToDecimal(row[columnName]);
            return FormatMoney(value);
        }

        private string FormatMoney(decimal value)
        {
            return value.ToString("N2", new CultureInfo("tr-TR")) + " TL";
        }

        private string GetScopeText(string text)
        {
            if (IsTumMagazalar())
                return "tüm mağazalarda " + text;

            return text;
        }

        private void FitListItemWidths(FlowLayoutPanel list)
        {
            if (list == null)
                return;

            int width = list.ClientSize.Width - 30;

            if (width < 280)
                width = 280;

            foreach (Control control in list.Controls)
            {
                control.Width = width;
            }
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;

            if (mainLayout != null)
                mainLayout.BackColor = AppColors.Background;

            if (cardsGrid != null)
                cardsGrid.BackColor = AppColors.Background;

            if (bodyArea != null)
                bodyArea.BackColor = AppColors.Background;

            if (bodyGrid != null)
                bodyGrid.BackColor = AppColors.Background;

            if (recentOrdersPanel != null)
            {
                recentOrdersPanel.BackColor = AppColors.CardBackground;
                recentOrdersPanel.BorderColor = AppColors.Border;
            }

            if (criticalStockPanel != null)
            {
                criticalStockPanel.BackColor = AppColors.CardBackground;
                criticalStockPanel.BorderColor = AppColors.Border;
            }

            if (recentOrdersList != null)
                recentOrdersList.BackColor = AppColors.CardBackground;

            if (criticalStockList != null)
                criticalStockList.BackColor = AppColors.CardBackground;

            if (lblRecentOrdersTitle != null)
                lblRecentOrdersTitle.ForeColor = AppColors.TextPrimary;

            if (lblRecentOrdersSubTitle != null)
                lblRecentOrdersSubTitle.ForeColor = AppColors.TextSecondary;

            if (lblCriticalStockTitle != null)
                lblCriticalStockTitle.ForeColor = AppColors.TextPrimary;

            if (lblCriticalStockSubTitle != null)
                lblCriticalStockSubTitle.ForeColor = AppColors.TextSecondary;

            cTotalRevenue?.ApplyTheme();
            cTotalOrders?.ApplyTheme();
            cActiveStores?.ApplyTheme();
            cTotalCustomers?.ApplyTheme();
            cPendingPayments?.ApplyTheme();
            cLowStock?.ApplyTheme();
            cPreparingOrders?.ApplyTheme();
            cShippedOrders?.ApplyTheme();
            cDeliveredOrders?.ApplyTheme();

            Invalidate(true);
            Refresh();
        }
    }
}
