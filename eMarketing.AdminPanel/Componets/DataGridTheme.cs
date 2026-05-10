using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public static class DataGridTheme
    {
        public static void Apply(DataGridView dgv)
        {
            dgv.BackgroundColor = AppColors.CardBackground;
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;

            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ScrollBars = ScrollBars.Both;

            dgv.ColumnHeadersHeight = 46;
            dgv.RowTemplate.Height = 48;

            dgv.GridColor = AppColors.Border;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = AppColors.IsDarkMode
                ? Color.FromArgb(30, 41, 59)
                : Color.FromArgb(248, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = dgv.ColumnHeadersDefaultCellStyle.BackColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            dgv.DefaultCellStyle.BackColor = AppColors.CardBackground;
            dgv.DefaultCellStyle.ForeColor = AppColors.TextPrimary;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.Padding = new Padding(8, 2, 8, 2);
            dgv.DefaultCellStyle.SelectionBackColor = AppColors.PrimarySoft;
            dgv.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = AppColors.IsDarkMode
                ? Color.FromArgb(24, 32, 44)
                : Color.FromArgb(250, 251, 253);
        }
    }
}
