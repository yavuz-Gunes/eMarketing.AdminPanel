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
        }
    }
}
