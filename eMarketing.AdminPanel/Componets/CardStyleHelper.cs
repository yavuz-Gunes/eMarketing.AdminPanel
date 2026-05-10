using System.Windows.Forms;

namespace eMarketing.AdminPanel.Componets
{
    public static class CardStyleHelper
    {
        public static void ApplySummarySpacing(Control card)
        {
            if (card == null)
                return;

            card.Margin = new Padding(0, 0, 16, 18);
        }
    }
}
