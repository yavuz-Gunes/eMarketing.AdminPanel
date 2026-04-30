using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class PrimaryButton : Button
    {
        public PrimaryButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;

            BackColor = AppColors.Primary;
            ForeColor = Color.White;

            Height = 40;
            Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Cursor = Cursors.Hand;

            MouseEnter += (s, e) => BackColor = AppColors.PrimaryDark;
            MouseLeave += (s, e) => BackColor = AppColors.Primary;
        }
    }
}