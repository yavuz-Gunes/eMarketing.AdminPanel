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

            Text = "eMarketing - Oto Yedek Parça Yönetim Paneli";
            ShowIcon = false;
            BackColor = AppColors.Background;
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.Sizable;

            Controls.Clear();

            bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            topbar = new TopbarControl
            {
                Dock = DockStyle.Top,
                Height = 88,
                Margin = Padding.Empty
            };

            topbar.ThemeToggleClicked += Topbar_ThemeToggleClicked;

            sidebar = new SidebarControl
            {
                Dock = DockStyle.Left,
                Width = 250,
                Margin = Padding.Empty
            };

            sidebar.MenuClicked += Sidebar_MenuClicked;

            bodyPanel.Controls.Add(contentPanel);
            bodyPanel.Controls.Add(topbar);

            Controls.Add(bodyPanel);
            Controls.Add(sidebar);

            LoadPage(
                new DashboardPage(),
                "Kontrol Paneli",
                "Genel satış, stok ve sipariş özetleri"
            );

            ResumeLayout(true);
        }

        private void LoadPage(UserControl page, string title, string subtitle)
        {
            if (contentPanel == null || topbar == null)
                return;

            contentPanel.SuspendLayout();

            while (contentPanel.Controls.Count > 0)
            {
                Control control = contentPanel.Controls[0];
                contentPanel.Controls.RemoveAt(0);
                control.Dispose();
            }

            page.Dock = DockStyle.Fill;
            page.Margin = Padding.Empty;
            page.BackColor = AppColors.Background;

            contentPanel.Controls.Add(page);

            topbar.SetTitle(title);
            topbar.SetSubtitle(GetTopbarSubtitle(subtitle));

            if (page is IThemeable themeablePage)
                themeablePage.ApplyTheme();

            contentPanel.ResumeLayout(true);
        }

        private string GetTopbarSubtitle(string pageSubtitle)
        {
            string selectedStore = AppSession.MagazaGorunumAdi;

            if (string.IsNullOrWhiteSpace(pageSubtitle))
                return "Seçili mağaza: " + selectedStore;

            return pageSubtitle + "  |  Seçili mağaza: " + selectedStore;
        }

        private void Sidebar_MenuClicked(object sender, string pageName)
        {
            if (pageName == "Dashboard")
            {
                LoadPage(
                    new DashboardPage(),
                    "Kontrol Paneli",
                    "Genel satış, stok ve sipariş özetleri"
                );
            }
            else if (pageName == "Products")
            {
                LoadPage(
                    new ProductsPage(),
                    "Ürünler",
                    "Oto yedek parça ürünlerini listele, ekle ve düzenle"
                );
            }
            else if (pageName == "Categories")
            {
                LoadPage(
                    new CategoriesPage(),
                    "Kategoriler",
                    "Ürün kategorilerini yönet"
                );
            }
            else if (pageName == "Orders")
            {
                LoadPage(
                    new OrdersPage(),
                    "Siparişler",
                    "Seçili mağazaya ait müşteri siparişlerini görüntüle ve takip et"
                );
            }
            else if (pageName == "Customers")
            {
                LoadPage(
                    new CustomersPage(),
                    "Müşteriler",
                    "Müşteri bilgilerini, mağazalarını ve sipariş ilişkilerini yönet"
                );
            }
            else if (pageName == "Personnel")
            {
                MessageBox.Show(
                    "Personel sayfası henüz oluşturulmadı.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else if (pageName == "Logout")
            {
                AppSession.CikisYap();
                Application.Exit();
            }
        }

        private void Topbar_ThemeToggleClicked()
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BackColor = AppColors.Background;

            if (bodyPanel != null)
                bodyPanel.BackColor = AppColors.Background;

            if (contentPanel != null)
                contentPanel.BackColor = AppColors.Background;

            topbar?.ApplyTheme();

            if (contentPanel != null)
            {
                foreach (Control control in contentPanel.Controls)
                {
                    ApplyThemeRecursive(control);
                }
            }

            Invalidate(true);
            Refresh();
        }

        private void ApplyThemeRecursive(Control control)
        {
            if (control is IThemeable themeable)
            {
                themeable.ApplyTheme();
            }
            else
            {
                if (control is Panel || control is UserControl)
                    control.BackColor = AppColors.Background;

                if (control is Label label)
                    label.ForeColor = AppColors.TextPrimary;

                if (control is TextBox textBox)
                {
                    textBox.BackColor = AppColors.InputBackground;
                    textBox.ForeColor = AppColors.TextPrimary;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                }

                if (control is ComboBox comboBox)
                {
                    comboBox.BackColor = AppColors.InputBackground;
                    comboBox.ForeColor = AppColors.TextPrimary;
                }

                if (control is Button button)
                {
                    button.BackColor = AppColors.PrimarySoft;
                    button.ForeColor = AppColors.Primary;
                }
            }

            foreach (Control child in control.Controls)
            {
                ApplyThemeRecursive(child);
            }

            control.Invalidate();
            control.Refresh();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
        }
    }
}