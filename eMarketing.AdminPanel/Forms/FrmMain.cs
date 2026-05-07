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

        private string currentPageKey = "Dashboard";

        public bool KullaniciDegistirIstendi { get; private set; }

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
            topbar.StoreChangeClicked += Topbar_StoreChangeClicked;
            topbar.SetUserName(AppSession.AdSoyad);
            topbar.SetStoreName(AppSession.MagazaGorunumAdi);

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

            LoadPage("Dashboard");

            ResumeLayout(true);
        }

        private void LoadPage(string pageKey)
        {
            UserControl page;
            string title;
            string subtitle;

            if (!CanAccessPage(pageKey))
            {
                page = new PlaceholderPage(
                    "Yetkisiz Erişim",
                    "Bu modül yalnızca yönetici rolündeki kullanıcılar tarafından görüntülenebilir.");
                title = "Yetkisiz Erişim";
                subtitle = "Bu alan için yönetici yetkisi gerekir";
            }
            else if (pageKey == "Products")
            {
                page = new ProductsPage();
                title = "Ürünler";
                subtitle = "Oto yedek parça ürünlerini listele, ekle ve düzenle";
            }
            else if (pageKey == "DealerStock")
            {
                page = new BayiStokPage();
                title = "Bayi Stokları";
                subtitle = AppSession.AdminMi
                    ? "Tüm bayilerin mağaza stoklarını takip et"
                    : "Seçili mağazanın stoklarını ve eksik ürünlerini takip et";
            }
            else if (pageKey == "Categories")
            {
                page = new CategoriesPage();
                title = "Kategoriler";
                subtitle = "Ürün kategorilerini yönet";
            }
            else if (pageKey == "Orders")
            {
                page = new OrdersPage();
                title = AppSession.AdminMi ? "Siparişler" : "Verilen Siparişler";
                subtitle = AppSession.AdminMi
                    ? "Seçili mağazaya ait müşteri siparişlerini görüntüle ve takip et"
                    : "Merkezden gelen siparişlerin hazırlık, kargo ve teslim durumunu takip et";
            }
            else if (pageKey == "Customers")
            {
                page = new CustomersPage();
                title = "Müşteriler";
                subtitle = "Müşteri bilgilerini, mağazalarını ve sipariş ilişkilerini yönet";
            }
            else if (pageKey == "Stores")
            {
                page = new MagazalarPage();
                title = "Mağazalar";
                subtitle = "Bayileri, mağaza kartlarını ve mağaza operasyonunu yönet";
            }
            else if (pageKey == "Personnel")
            {
                page = new PersonelPage();
                title = AppSession.AdminMi ? "Personel" : "Bayi Personeli";
                subtitle = AppSession.AdminMi
                    ? "Kullanıcı, rol ve mağaza yetkilendirmeleri"
                    : "Bayinize bağlı personel ve mağaza erişimleri";
            }
            else if (pageKey == "Reports")
            {
                page = new PlaceholderPage(
                    "Raporlar",
                    "Raporlar ekranı hazırlanıyor. Satış, ciro, stok ve mağaza performans raporları burada toplanacak.");
                title = "Raporlar";
                subtitle = "Satış, stok ve mağaza performans analizleri";
            }
            else if (pageKey == "Settings")
            {
                page = new PlaceholderPage(
                    "Ayarlar",
                    "Ayarlar ekranı hazırlanıyor. Sistem tercihleri ve uygulama davranışları bu modülde yönetilecek.");
                title = "Ayarlar";
                subtitle = "Sistem tercihleri ve yönetim ayarları";
            }
            else
            {
                page = new DashboardPage();
                title = "Kontrol Paneli";
                subtitle = "Genel satış, stok ve sipariş özetleri";
                pageKey = "Dashboard";
            }

            LoadPage(pageKey, page, title, subtitle);
        }

        public void NavigateTo(string pageKey)
        {
            LoadPage(pageKey);

            if (sidebar != null)
                sidebar.SetActiveMenu(pageKey);
        }

        private void LoadPage(string pageKey, UserControl page, string title, string subtitle)
        {
            if (contentPanel == null || topbar == null)
                return;

            currentPageKey = pageKey;
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
            topbar.SetSubtitle(subtitle);
            topbar.SetStoreName(AppSession.MagazaGorunumAdi);
            topbar.SetUserName(AppSession.AdSoyad);

            if (page is IThemeable themeablePage)
                themeablePage.ApplyTheme();

            contentPanel.ResumeLayout(true);
        }

        private void Sidebar_MenuClicked(object sender, string pageName)
        {
            if (pageName == "Dashboard" ||
                pageName == "Products" ||
                pageName == "DealerStock" ||
                pageName == "Categories" ||
                pageName == "Orders" ||
                pageName == "Customers" ||
                pageName == "Stores" ||
                pageName == "Personnel" ||
                pageName == "Reports" ||
                pageName == "Settings")
            {
                LoadPage(pageName);
            }
            else if (pageName == "Logout")
            {
                AppSession.CikisYap();
                Application.Exit();
            }
            else if (pageName == "SwitchUser")
            {
                KullaniciDegistirIstendi = true;
                Close();
            }
        }

        private bool CanAccessPage(string pageKey)
        {
            if (pageKey == "Customers" ||
                pageKey == "Stores" ||
                pageKey == "Products" ||
                pageKey == "Categories" ||
                pageKey == "Reports" ||
                pageKey == "Settings")
            {
                return AppSession.AdminMi;
            }

            if (pageKey == "Personnel")
                return AppSession.AdminMi || AppSession.Rol.Equals("StoreManager", StringComparison.OrdinalIgnoreCase);

            return true;
        }

        private void Topbar_StoreChangeClicked()
        {
            using (MagazaSecimForm magazaSecimForm = new MagazaSecimForm())
            {
                if (magazaSecimForm.ShowDialog(this) != DialogResult.OK || !magazaSecimForm.SecimYapildi)
                    return;
            }

            topbar.SetStoreName(AppSession.MagazaGorunumAdi);
            LoadPage(currentPageKey);
        }

        public void MagazaSeciminiYenile()
        {
            topbar?.SetStoreName(AppSession.MagazaGorunumAdi);
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
