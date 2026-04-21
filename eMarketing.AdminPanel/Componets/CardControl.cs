using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class CardControl : Panel
    {
        public Label Title { get; private set; }
        public Label Value { get; private set; }

        public CardControl()
        {
            BackColor = AppColors.Card;
            Margin = Padding.Empty;
            Padding = new Padding(20);

            Title = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                AutoSize = false,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Value = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Controls.Add(Value);
            Controls.Add(Title);
        }

        public void SetData(string title, string value)
        {
            Title.Text = title;
            Value.Text = value;
        }
    }
}