using System.Windows.Forms;

namespace eMarketing.AdminPanel.Componets
{
    public static class DataGridViewStyleHelper
    {
        public static void ApplyModernGrid(DataGridView grid)
        {
            DataGridTheme.Apply(grid);
        }

        public static void UpdateCountLabel(Label label, int visibleCount, int totalCount)
        {
            if (label == null)
                return;

            label.Text = visibleCount == totalCount
                ? visibleCount + " kayıt"
                : visibleCount + " / " + totalCount + " kayıt";
            label.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            label.ForeColor = Core.AppColors.TextSecondary;
            label.BackColor = Core.AppColors.PrimarySoft;
            label.Padding = new Padding(10, 0, 10, 0);
            label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        }
    }
}
