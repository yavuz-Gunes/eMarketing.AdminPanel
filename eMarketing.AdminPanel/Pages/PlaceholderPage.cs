using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Pages
{
    public class PlaceholderPage : UserControl, IThemeable
    {
        private readonly string title;
        private readonly string message;

        private ShadowPanel panel;
        private Label titleLabel;
        private Label messageLabel;

        public PlaceholderPage(string title, string message)
        {
            this.title = title;
            this.message = message;

            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24);

            BuildLayout();
            ApplyTheme();
        }

        private void BuildLayout()
        {
            panel = new ShadowPanel
            {
                Dock = DockStyle.Top,
                Height = 170,
                Padding = new Padding(28, 24, 28, 24),
                CornerRadius = 12,
                ShadowSize = 4
            };

            titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                Text = title,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            messageLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 62,
                Text = message,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(messageLabel);
            panel.Controls.Add(titleLabel);
            Controls.Add(panel);
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;

            if (panel != null)
            {
                panel.BackColor = AppColors.CardBackground;
                panel.BorderColor = AppColors.Border;
                panel.ShadowColor = Color.FromArgb(18, 15, 23, 42);
            }

            if (titleLabel != null)
                titleLabel.ForeColor = AppColors.TextPrimary;

            if (messageLabel != null)
                messageLabel.ForeColor = AppColors.TextSecondary;
        }
    }
}
