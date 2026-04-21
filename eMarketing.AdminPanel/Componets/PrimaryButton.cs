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
    public class PrimaryButton : Button
    {
        public PrimaryButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = AppColors.Primary;
            ForeColor = Color.White;
            Height = 38;
            Font = new Font("Segoe UI", 9F);
            Cursor = Cursors.Hand;
        }
    }
}
