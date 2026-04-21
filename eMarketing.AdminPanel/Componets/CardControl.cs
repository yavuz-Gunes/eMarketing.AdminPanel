using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class CardControl : Panel
    {
        public Label Title = new Label();
        public Label Value = new Label();

        public CardControl()
        {
            Width = 250;
            Height = 120;
            BackColor = AppColors.Card;
            Padding = new Padding(20);

            Title.Font = new Font("Segoe UI", 9F);
            Title.ForeColor = AppColors.TextSecondary;
            Title.Dock = DockStyle.Top;

            Value.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            Value.ForeColor = AppColors.TextPrimary;
            Value.Dock = DockStyle.Bottom;

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
