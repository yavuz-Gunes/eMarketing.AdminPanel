using System;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Models;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Pages
{
    public class DashboardPage : UserControl
    {
        private readonly DashboardRepository _repo = new DashboardRepository();

        private TableLayoutPanel cardsGrid;

        private CardControl cTotalProducts;
        private CardControl cActiveProducts;
        private CardControl cLowStock;
        private CardControl cTotalCategories;
        private CardControl cActiveCategories;
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
                Height = 270,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            cardsGrid.ColumnStyles.Clear();
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            cardsGrid.RowStyles.Clear();
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            cTotalProducts = CreateCard("Toplam Ürün", "0", new Padding(0, 0, 16, 16));
            cActiveProducts = CreateCard("Aktif Ürün", "0", new Padding(0, 0, 16, 16));
            cLowStock = CreateCard("Kritik Stok", "0", new Padding(0, 0, 0, 16));

            cTotalCategories = CreateCard("Toplam Kategori", "0", new Padding(0, 0, 16, 0));
            cActiveCategories = CreateCard("Aktif Kategori", "0", new Padding(0, 0, 16, 0));
            cTotalOrders = CreateCard("Toplam Sipariþ", "0", Padding.Empty);

            cardsGrid.Controls.Add(cTotalProducts, 0, 0);
            cardsGrid.Controls.Add(cActiveProducts, 1, 0);
            cardsGrid.Controls.Add(cLowStock, 2, 0);

            cardsGrid.Controls.Add(cTotalCategories, 0, 1);
            cardsGrid.Controls.Add(cActiveCategories, 1, 1);
            cardsGrid.Controls.Add(cTotalOrders, 2, 1);

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
            LoadDashboardSummary();
        }

        private void LoadDashboardSummary()
        {
            try
            {
                DashboardSummary summary = _repo.GetSummary();

                cTotalProducts.SetData("Toplam Ürün", summary.TotalProducts.ToString());
                cActiveProducts.SetData("Aktif Ürün", summary.ActiveProducts.ToString());
                cLowStock.SetData("Kritik Stok", summary.LowStockProducts.ToString());

                cTotalCategories.SetData("Toplam Kategori", summary.TotalCategories.ToString());
                cActiveCategories.SetData("Aktif Kategori", summary.ActiveCategories.ToString());

                cTotalOrders.SetData("Toplam Sipariþ", summary.TotalOrders.ToString());
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
    }
}