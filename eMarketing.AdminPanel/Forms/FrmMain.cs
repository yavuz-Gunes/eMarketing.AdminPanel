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
        private Panel bodyPanel;

        public FrmMain()
        {
            InitializeComponent();
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            SuspendLayout();

            BackColor = AppColors.Background;
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.Sizable;

            Controls.Clear();

            // BODY PANEL
            bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // CONTENT PANEL
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // TOPBAR
            topbar = new TopbarControl
            {
                Dock = DockStyle.Top,
                Height = 60,
                Margin = Padding.Empty
            };

            // SIDEBAR
            sidebar = new SidebarControl
            {
                Dock = DockStyle.Left,
                Width = 240,
                Margin = Padding.Empty
            };
            sidebar.MenuClicked += Sidebar_MenuClicked;

            // ÖNEMLİ: Dock sırası için önce Fill, sonra Top
            bodyPanel.Controls.Add(contentPanel);
            bodyPanel.Controls.Add(topbar);

            // ÖNEMLİ: Önce Fill alan, sonra Left sidebar
            Controls.Add(bodyPanel);
            Controls.Add(sidebar);

            LoadPage(new DashboardPage(), "Dashboard");

            ResumeLayout(true);
        }

        private void LoadPage(UserControl page, string title)
        {
            contentPanel.SuspendLayout();

            foreach (Control c in contentPanel.Controls)
                c.Dispose();

            contentPanel.Controls.Clear();

            page.Dock = DockStyle.Fill;
            page.Margin = Padding.Empty;

            contentPanel.Controls.Add(page);
            topbar.SetTitle(title);

            contentPanel.ResumeLayout(true);
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
                case "Categories":
                    LoadPage(new CategoriesPage(), "Categories");
                    break;
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
        }
    }
}