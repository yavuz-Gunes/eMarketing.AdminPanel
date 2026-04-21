using System;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.DataAccess;

namespace eMarketing.AdminPanel.Pages
{
    public class DashboardPage : UserControl
    {
        private readonly DashboardRepository _repo = new DashboardRepository();

        private TableLayoutPanel cardsGrid;
        private CardControl cTotalProducts;
        private CardControl cActiveProducts;
        private CardControl cLowStock;
        private CardControl cTotalOrders;
        private Panel bodyArea;

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

            cardsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 130,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            cardsGrid.ColumnStyles.Clear();
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            cardsGrid.RowStyles.Clear();
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cTotalProducts = CreateCard("Toplam Ürün", "0", new Padding(0, 0, 16, 0));
            cActiveProducts = CreateCard("Aktif Ürün", "0", new Padding(0, 0, 16, 0));
            cLowStock = CreateCard("Kritik Stok", "0", new Padding(0, 0, 16, 0));
            cTotalOrders = CreateCard("Toplam Sipariþ", "0", Padding.Empty);

            cardsGrid.Controls.Add(cTotalProducts, 0, 0);
            cardsGrid.Controls.Add(cActiveProducts, 1, 0);
            cardsGrid.Controls.Add(cLowStock, 2, 0);
            cardsGrid.Controls.Add(cTotalOrders, 3, 0);

            bodyArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Margin = new Padding(0, 18, 0, 0),
                Padding = Padding.Empty
            };

            Controls.Add(bodyArea);
            Controls.Add(cardsGrid);

            ResumeLayout(true);
        }

        private CardControl CreateCard(string title, string value, Padding margin)
        {
            var card = new CardControl
            {
                Dock = DockStyle.Fill,
                Margin = margin
            };

            card.SetData(title, value);
            return card;
        }

        private void DashboardPage_Load(object sender, EventArgs e)
        {
            try
            {
                cTotalProducts.SetData("Toplam Ürün", _repo.GetTotalProducts().ToString());
                cActiveProducts.SetData("Aktif Ürün", _repo.GetActiveProducts().ToString());
                cLowStock.SetData("Kritik Stok", _repo.GetLowStockProducts().ToString());
                cTotalOrders.SetData("Toplam Sipariþ", _repo.GetTotalOrders().ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dashboard verileri çekilirken hata: " + ex.Message);
            }
        }
    }
}