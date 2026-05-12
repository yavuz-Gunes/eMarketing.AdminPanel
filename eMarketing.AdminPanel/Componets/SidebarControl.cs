using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class SidebarControl : UserControl
    {
        public event EventHandler<string> MenuClicked;

        private readonly Dictionary<string, Button> menuButtons = new Dictionary<string, Button>();

        private readonly Color sidebarBack = Color.FromArgb(20, 27, 38);
        private readonly Color sidebarSecond = Color.FromArgb(28, 36, 50);
        private readonly Color activeBack = Color.FromArgb(37, 99, 235);
        private readonly Color hoverBack = Color.FromArgb(36, 47, 66);
        private readonly Color textColor = Color.FromArgb(230, 236, 245);
        private readonly Color mutedTextColor = Color.FromArgb(140, 150, 165);
        private readonly Color dangerColor = Color.FromArgb(239, 68, 68);

        private FlowLayoutPanel menuPanel;
        private Panel logoPanel;
        private Panel bottomPanel;

        private string activeKey = "Dashboard";

        public SidebarControl()
        {
            Width = 264;
            Dock = DockStyle.Left;
            BackColor = sidebarBack;

            BuildLayout();
            SetActiveMenu("Dashboard");
        }

        private void BuildLayout()
        {
            Controls.Clear();

            BuildBottomPanel();
            BuildLogoPanel();
            BuildMenuPanel();
        }

        private void BuildLogoPanel()
        {
            logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = sidebarBack,
                Padding = new Padding(18, 18, 18, 12)
            };

            Label lblTitle = new Label
            {
                Text = "eMarketing",
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblSubTitle = new Label
            {
                Text = AppSession.AdminMi ? "Yönetim Paneli" : "Mağaza Operasyonu",
                Dock = DockStyle.Top,
                Height = 24,
                ForeColor = mutedTextColor,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel line = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(42, 52, 68)
            };

            logoPanel.Controls.Add(line);
            logoPanel.Controls.Add(lblSubTitle);
            logoPanel.Controls.Add(lblTitle);

            Controls.Add(logoPanel);
        }

        private void BuildMenuPanel()
        {
            menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = sidebarBack,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(12, 20, 12, 10)
            };

            AddMenuButton("Dashboard", "Kontrol Paneli", "▦");
            AddMenuButton("Orders", AppSession.AdminMi ? "Siparişler" : "Sipariş Takibi", "≡");
            AddMenuButton("DealerStock", "Bayi Stokları", "▤");

            if (!AppSession.AdminMi)
                AddMenuButton("Personnel", "Bayi Personeli", "○");

            if (AppSession.AdminMi)
            {
                AddMenuButton("Products", "Ürünler", "□");
                AddMenuButton("Customers", "Sipariş Yetkilileri", "◇");
                AddMenuButton("Stores", "Bayiler", "⌂");
                AddMenuButton("Categories", "Kategoriler", "⌁");
                AddMenuButton("Personnel", "Personel", "○");
                AddMenuButton("Reports", "Raporlar", "↗");
                AddMenuButton("Settings", "Ayarlar", "⚙");
            }

            Controls.Add(menuPanel);
            menuPanel.BringToFront();
        }

        private void BuildBottomPanel()
        {
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 136,
                BackColor = sidebarBack,
                Padding = new Padding(12, 10, 12, 14)
            };

            Button btnSwitchUser = CreateBottomButton("  Kullanıcı Değiştir", textColor);
            btnSwitchUser.Dock = DockStyle.Top;
            btnSwitchUser.Height = 42;
            btnSwitchUser.Click += (sender, e) =>
            {
                MenuClicked?.Invoke(this, "SwitchUser");
            };

            Button btnLogout = CreateBottomButton("  Çıkış Yap", dangerColor);
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Height = 42;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 35, 42);
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 40, 48);
            btnLogout.Click += (sender, e) =>
            {
                MenuClicked?.Invoke(this, "Logout");
            };

            bottomPanel.Controls.Add(btnSwitchUser);
            bottomPanel.Controls.Add(btnLogout);
            Controls.Add(bottomPanel);
        }

        private Button CreateBottomButton(string text, Color foreColor)
        {
            Button button = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = sidebarSecond,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = hoverBack;
            button.FlatAppearance.MouseDownBackColor = activeBack;

            return button;
        }

        private void AddMenuButton(string key, string text, string icon)
        {
            Button button = new Button
            {
                Name = "btn" + key,
                Text = FormatMenuText(icon, text),
                Width = 236,
                Height = 44,
                Margin = new Padding(0, 0, 0, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = sidebarBack,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Tag = text,
                AccessibleName = icon
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = hoverBack;
            button.FlatAppearance.MouseDownBackColor = activeBack;

            button.MouseEnter += (sender, e) =>
            {
                if (activeKey != key)
                    button.BackColor = hoverBack;
            };

            button.MouseLeave += (sender, e) =>
            {
                if (activeKey != key)
                    button.BackColor = sidebarBack;
            };

            button.Click += (sender, e) =>
            {
                SetActiveMenu(key);
                MenuClicked?.Invoke(this, key);
            };

            menuButtons[key] = button;
            menuPanel.Controls.Add(button);
        }

        public void SetActiveMenu(string key)
        {
            activeKey = key;

            foreach (var item in menuButtons)
            {
                Button button = item.Value;
                string text = Convert.ToString(button.Tag);
                string icon = Convert.ToString(button.AccessibleName);

                button.Text = FormatMenuText(icon, text);

                if (item.Key == key)
                {
                    button.BackColor = activeBack;
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = sidebarBack;
                    button.ForeColor = textColor;
                }
            }
        }

        private string FormatMenuText(string icon, string text)
        {
            return "  " + icon + "  " + text;
        }
    }
}
