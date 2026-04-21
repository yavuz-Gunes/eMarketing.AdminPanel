using System;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Pages;

namespace eMarketing.AdminPanel.Forms
{
    public partial class FrmMain : Form
    {
        private Panel contentPanel;
        private TopbarControl topbar;
        private SidebarControl sidebar;
        private Panel mainPanel;     // sidebar dışındaki tüm alan


        public FrmMain()
        {
            InitializeComponent();
            InitializeLayout();
        }

      private void InitializeLayout()
{
    BackColor = AppColors.Background;
    WindowState = FormWindowState.Maximized;
    FormBorderStyle = FormBorderStyle.Sizable;

    // Eğer designer'da eskiden kalma panel/btn vs varsa çakışmasın:
    // (Designer'da kontrol bırakmadıysan bunu kaldırabilirsin)
    Controls.Clear();

    // 1) SIDEBAR (Formun solunda, tüm boy boyunca)
    sidebar = new SidebarControl();
    sidebar.Dock = DockStyle.Left;
    sidebar.Width = 240; // SidebarControl içinde 240 yapmıştın, aynı kalsın
    sidebar.MenuClicked += Sidebar_MenuClicked;
    Controls.Add(sidebar);

    // 2) MAIN PANEL (Sidebar'ın sağındaki tüm alan)
    mainPanel = new Panel();
    mainPanel.Dock = DockStyle.Fill;
    mainPanel.BackColor = AppColors.Background;
    Controls.Add(mainPanel);

    // 3) TOPBAR (Sadece mainPanel içinde, sidebar'a binmez)
    topbar = new TopbarControl();
    topbar.Dock = DockStyle.Top;
    topbar.Height = 60;
    mainPanel.Controls.Add(topbar);

    // 4) CONTENT PANEL (Topbar’ın altında kalan alan)
    contentPanel = new Panel();
    contentPanel.Dock = DockStyle.Fill;
    contentPanel.BackColor = AppColors.Background;
    mainPanel.Controls.Add(contentPanel);

    // İlk açılan sayfa
    LoadPage(new DashboardPage(), "Dashboard");
}

private void LoadPage(UserControl page, string title)
{
    // eski sayfayı temizle + dispose et (üst üste binme hissini bitirir)
    foreach (Control c in contentPanel.Controls) c.Dispose();
    contentPanel.Controls.Clear();

    page.Dock = DockStyle.Fill;
    contentPanel.Controls.Add(page);

    topbar.SetTitle(title);
}

        private void Sidebar_MenuClicked(string menu)
        {
            switch (menu)
            {
                case "Dashboard":
                    LoadPage(new DashboardPage(), "Dashboard");
                    break;

                case "Products":
                    LoadPage(new ProductsPage(), "Products");
                    break;

                case "Orders":
                    LoadPage(new OrdersPage(), "Orders");
                    break;

                case "Customers":
                    LoadPage(new CustomersPage(), "Customers");
                    break;
            }
        }

      

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }
    }
}