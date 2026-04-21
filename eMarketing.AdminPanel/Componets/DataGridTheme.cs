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
    public static class DataGridTheme
    {
        public static void Apply(DataGridView dgv)
        {
            dgv.BackgroundColor = AppColors.Card;
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppColors.Primary;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersHeight = 45;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 238, 255);
            dgv.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
        }
    }
}
