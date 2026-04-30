using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace eMarketing.AdminPanel.Componets
{
    public class SidebarControl : UserControl
    {
        public event EventHandler<string> MenuClicked;

        private readonly Dictionary<string, Button> _menuButtons = new Dictionary<string, Button>();

        private readonly Color _sidebarBack = Color.FromArgb(20, 27, 38);
        private readonly Color _sidebarSecond = Color.FromArgb(28, 36, 50);
        private readonly Color _activeBack = Color.FromArgb(59, 130, 246);
        private readonly Color _hoverBack = Color.FromArgb(36, 47, 66);
        private readonly Color _textColor = Color.FromArgb(230, 236, 245);
        private readonly Color _mutedTextColor = Color.FromArgb(140, 150, 165);
        private readonly Color _dangerColor = Color.FromArgb(239, 68, 68);

        private FlowLayoutPanel menuPanel;
        private Panel logoPanel;
        private Panel bottomPanel;

        private string _activeKey = "Dashboard";

        public SidebarControl()
        {
            Width = 240;
            Dock = DockStyle.Left;
            BackColor = _sidebarBack;

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
                BackColor = _sidebarBack,
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
                Text = "Oto Yedek Parça Paneli",
                Dock = DockStyle.Top,
                Height = 24,
                ForeColor = _mutedTextColor,
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
                BackColor = _sidebarBack,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(12, 20, 12, 10)
            };

            AddMenuButton("Dashboard", "  Kontrol Paneli");
            AddMenuButton("Products", "  Ürünler");
            AddMenuButton("Categories", "  Kategoriler");
            AddMenuButton("Orders", "  Siparişler");
            AddMenuButton("Customers", "  Müşteriler");
            AddMenuButton("Personnel", "  Personel");

            Controls.Add(menuPanel);
            menuPanel.BringToFront();
        }

        private void BuildBottomPanel()
        {
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 86,
                BackColor = _sidebarBack,
                Padding = new Padding(12, 10, 12, 14)
            };

            Button btnLogout = new Button
            {
                Text = "  Çıkış Yap",
                Dock = DockStyle.Fill,
                Height = 46,
                FlatStyle = FlatStyle.Flat,
                BackColor = _sidebarSecond,
                ForeColor = _dangerColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };

            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 35, 42);
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 40, 48);

            btnLogout.Click += (sender, e) =>
            {
                MenuClicked?.Invoke(this, "Logout");
            };

            bottomPanel.Controls.Add(btnLogout);
            Controls.Add(bottomPanel);
        }

        private void AddMenuButton(string key, string text)
        {
            Button button = new Button
            {
                Name = "btn" + key,
                Text = text,
                Width = 216,
                Height = 44,
                Margin = new Padding(0, 0, 0, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = _sidebarBack,
                ForeColor = _textColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand,
                Tag = key
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = _hoverBack;
            button.FlatAppearance.MouseDownBackColor = _activeBack;

            button.MouseEnter += (sender, e) =>
            {
                if (_activeKey != key)
                    button.BackColor = _hoverBack;
            };

            button.MouseLeave += (sender, e) =>
            {
                if (_activeKey != key)
                    button.BackColor = _sidebarBack;
            };

            button.Click += (sender, e) =>
            {
                SetActiveMenu(key);
                MenuClicked?.Invoke(this, key);
            };

            _menuButtons[key] = button;
            menuPanel.Controls.Add(button);
        }

        public void SetActiveMenu(string key)
        {
            _activeKey = key;

            foreach (var item in _menuButtons)
            {
                Button button = item.Value;
                string cleanText = button.Text.Replace("●", "").Trim();

                if (item.Key == key)
                {
                    button.BackColor = _activeBack;
                    button.ForeColor = Color.White;
                    button.Text = "  ● " + cleanText;
                }
                else
                {
                    button.BackColor = _sidebarBack;
                    button.ForeColor = _textColor;
                    button.Text = "  " + cleanText;
                }
            }
        }
    }
}