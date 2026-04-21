using System;
using System.Drawing;
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

        private Panel bodyArea; // kartların altı (ileride grafik/liste)

        public DashboardPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;

            BuildLayout();
            Load += DashboardPage_Load;
        }

        private void BuildLayout()
        {
            // Sayfanın iç padding’i (content panelin içinde güzel dursun)
            Padding = new Padding(24, 18, 24, 18);

            // 1) Kart grid
            cardsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 140,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = AppColors.Background
            };

            // 4 kolon eşit genişlik
            cardsGrid.ColumnStyles.Clear();
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            cardsGrid.RowStyles.Clear();
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Kartları oluştur
            cTotalProducts = CreateCard("Toplam Ürün", "0");
            cActiveProducts = CreateCard("Aktif Ürün", "0");
            cLowStock = CreateCard("Kritik Stok", "0");
            cTotalOrders = CreateCard("Toplam Sipariş", "0");

            // Grid’e ekle
            cardsGrid.Controls.Add(cTotalProducts, 0, 0);
            cardsGrid.Controls.Add(cActiveProducts, 1, 0);
            cardsGrid.Controls.Add(cLowStock, 2, 0);
            cardsGrid.Controls.Add(cTotalOrders, 3, 0);

            // 2) Kartların altı (şimdilik boş alan)
            bodyArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background
            };

            Controls.Add(bodyArea);
            Controls.Add(cardsGrid);
        }

        private CardControl CreateCard(string title, string value)
        {
            var card = new CardControl();

            // Kartın grid hücresini doldurması için
            card.Dock = DockStyle.Fill;

            // Kartlar arası boşluk (çok kritik, premium his verir)
            card.Margin = new Padding(0, 0, 16, 0);

            // Son kolonda sağ margin olmasın
            // (margin’i sonradan Load’da ayarlayacağız)
            card.SetData(title, value);
            return card;
        }

        private void DashboardPage_Load(object sender, EventArgs e)
        {
            try
            {
                // son kolondaki margin’i sıfırla (sağda boşluk kalmasın)
                cTotalOrders.Margin = new Padding(0);

                cTotalProducts.SetData("Toplam Ürün", _repo.GetTotalProducts().ToString());
                cActiveProducts.SetData("Aktif Ürün", _repo.GetActiveProducts().ToString());
                cLowStock.SetData("Kritik Stok", _repo.GetLowStockProducts().ToString());
                cTotalOrders.SetData("Toplam Sipariş", _repo.GetTotalOrders().ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dashboard verileri çekilirken hata: " + ex.Message);
            }
        }
    }
}