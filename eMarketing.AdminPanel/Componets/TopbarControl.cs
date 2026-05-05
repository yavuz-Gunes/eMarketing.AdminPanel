using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class TopbarControl : Panel
    {
        public event Action ThemeToggleClicked;
        public event Action StoreChangeClicked;

        private Panel leftPanel;
        private FlowLayoutPanel rightPanel;

        private Label titleLabel;
        private Label subtitleLabel;

        private Panel dateBadge;
        private Panel adminBadge;

        private Label dateTimeLabel;
        private Label adminLabel;

        private Button btnStore;
        private Button btnTheme;

        private Timer clockTimer;

        public TopbarControl()
        {
            Height = 92;
            Dock = DockStyle.Top;
            Padding = new Padding(28, 16, 28, 14);
            DoubleBuffered = true;

            BuildTopbar();
            ApplyTheme();

            clockTimer = new Timer
            {
                Interval = 1000
            };

            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        private void BuildTopbar()
        {
            Controls.Clear();

            leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            titleLabel = new Label
            {
                Text = "Kontrol Paneli",
                AutoSize = false,
                Width = 460,
                Height = 32,
                Location = new Point(0, 0),
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            subtitleLabel = new Label
            {
                Text = "Oto yedek parça yönetim özeti",
                AutoSize = false,
                Width = 520,
                Height = 24,
                Location = new Point(2, 38),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };

            leftPanel.Controls.Add(titleLabel);
            leftPanel.Controls.Add(subtitleLabel);

            rightPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Width = 760,
                Height = 52,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0),
                Margin = new Padding(0)
            };

            adminBadge = CreateBadgePanel(out adminLabel, "Admin", 118);
            btnTheme = CreateThemeButton();
            dateBadge = CreateBadgePanel(out dateTimeLabel, DateTime.Now.ToString("dd.MM.yyyy HH:mm"), 178);
            btnStore = CreateStoreButton();

            rightPanel.Controls.Add(adminBadge);
            rightPanel.Controls.Add(btnTheme);
            rightPanel.Controls.Add(dateBadge);
            rightPanel.Controls.Add(btnStore);

            Controls.Add(leftPanel);
            Controls.Add(rightPanel);

            UpdateClock();
        }

        private Panel CreateBadgePanel(out Label label, string text, int width)
        {
            Panel panel = new Panel
            {
                Width = width,
                Height = 38,
                Margin = new Padding(10, 0, 0, 0),
                Padding = new Padding(8, 0, 8, 0)
            };

            label = new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.Transparent
            };

            panel.Controls.Add(label);
            panel.Resize += (s, e) => ApplyRoundedRegion(panel, 12);

            return panel;
        }

        private Button CreateThemeButton()
        {
            Button button = new Button
            {
                Width = 46,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0),
                Text = "🌙",
                Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += BtnTheme_Click;
            button.Resize += (s, e) => ApplyRoundedRegion(button, 12);

            return button;
        }

        private Button CreateStoreButton()
        {
            Button button = new Button
            {
                Width = 300,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0),
                Text = "Mağaza: Tüm Mağazalar",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 12, 0),
                AutoEllipsis = true
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += (sender, e) => StoreChangeClicked?.Invoke();
            button.Resize += (sender, e) => ApplyRoundedRegion(button, 12);

            return button;
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            if (dateTimeLabel != null)
                dateTimeLabel.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }

        private void BtnTheme_Click(object sender, EventArgs e)
        {
            AppColors.ToggleTheme();
            ApplyTheme();
            ThemeToggleClicked?.Invoke();
        }

        public void SetTitle(string title)
        {
            if (titleLabel != null)
                titleLabel.Text = title;
        }

        public void SetSubtitle(string subtitle)
        {
            if (subtitleLabel != null)
                subtitleLabel.Text = subtitle;
        }

        public void SetHeader(string title, string subtitle)
        {
            SetTitle(title);
            SetSubtitle(subtitle);
        }

        public void SetStoreName(string storeName)
        {
            if (btnStore == null)
                return;

            btnStore.Text = "Mağaza: " + (string.IsNullOrWhiteSpace(storeName)
                ? "Mağaza Seçilmedi"
                : storeName);
        }

        public void SetUserName(string userName)
        {
            if (adminLabel == null)
                return;

            adminLabel.Text = string.IsNullOrWhiteSpace(userName) ? "Admin" : userName;
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.TopbarBackground;

            titleLabel.ForeColor = AppColors.TextPrimary;
            subtitleLabel.ForeColor = AppColors.TextSecondary;

            ApplyBadgeTheme(dateBadge, dateTimeLabel, false);
            ApplyBadgeTheme(adminBadge, adminLabel, true);

            btnStore.BackColor = AppColors.PrimarySoft;
            btnStore.ForeColor = AppColors.Primary;

            btnTheme.BackColor = AppColors.PrimarySoft;
            btnTheme.ForeColor = AppColors.Primary;
            btnTheme.Text = AppColors.IsDarkMode ? "☀️" : "🌙";

            Invalidate(true);
        }

        private void ApplyBadgeTheme(Panel panel, Label label, bool primaryText)
        {
            panel.BackColor = AppColors.PrimarySoft;
            label.ForeColor = primaryText ? AppColors.Primary : AppColors.TextSecondary;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(AppColors.Border))
            {
                e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            }
        }

        private void ApplyRoundedRegion(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
                return;

            using (GraphicsPath path = GetRoundedRectanglePath(
                new Rectangle(0, 0, control.Width, control.Height), radius))
            {
                control.Region = new Region(path);
            }
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter - 1, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter - 1, bounds.Bottom - diameter - 1, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter - 1, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
