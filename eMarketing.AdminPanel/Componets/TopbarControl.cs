using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class TopbarControl : Panel
    {
        private Label titleLabel;

        public TopbarControl()
        {
            Height = 60;
            Dock = DockStyle.Top;
            BackColor = Color.White;

            titleLabel = new Label();
            titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            titleLabel.ForeColor = AppColors.TextPrimary;
            titleLabel.Location = new Point(20, 18);
            titleLabel.AutoSize = true;

            Controls.Add(titleLabel);
        }

        public void SetTitle(string title)
        {
            titleLabel.Text = title;
        }
    }
}