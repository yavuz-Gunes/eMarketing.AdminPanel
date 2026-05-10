using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public static class ButtonStyleHelper
    {
        public static void ApplyPrimary(Button button)
        {
            Apply(button, AppColors.Primary, Color.White, 0);
        }

        public static void ApplySoft(Button button)
        {
            Apply(button, AppColors.PrimarySoft, AppColors.Primary, 0);
        }

        public static void ApplyOutline(Button button)
        {
            Apply(button, AppColors.CardBackground, AppColors.TextSecondary, 1);
            button.FlatAppearance.BorderColor = AppColors.Border;
        }

        public static void ApplySuccess(Button button)
        {
            Apply(button, AppColors.Success, Color.White, 0);
        }

        public static void ApplyDanger(Button button)
        {
            Apply(button, AppColors.Danger, Color.White, 0);
        }

        public static void ApplyInput(TextBox textBox)
        {
            if (textBox == null)
                return;

            textBox.BackColor = AppColors.InputBackground;
            textBox.ForeColor = AppColors.TextPrimary;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("Segoe UI", 10F);
        }

        public static void ApplyDropdown(ComboBox comboBox)
        {
            if (comboBox == null)
                return;

            comboBox.BackColor = AppColors.InputBackground;
            comboBox.ForeColor = AppColors.TextPrimary;
            comboBox.Font = new Font("Segoe UI", 10F);
        }

        private static void Apply(Button button, Color backColor, Color foreColor, int borderSize)
        {
            if (button == null)
                return;

            button.Height = button.Height < 34 ? 34 : button.Height;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = borderSize;
        }
    }
}
