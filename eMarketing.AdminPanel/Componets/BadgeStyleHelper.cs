using System;
using System.Drawing;

namespace eMarketing.AdminPanel.Componets
{
    public static class BadgeStyleHelper
    {
        public static void GetStatusColors(string status, out Color backColor, out Color foreColor)
        {
            string value = status ?? "";

            if (value.Equals("Teslim Edildi", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Aktif", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Yeterli", StringComparison.OrdinalIgnoreCase))
            {
                backColor = Color.FromArgb(236, 253, 245);
                foreColor = Color.FromArgb(21, 128, 61);
                return;
            }

            if (value.Equals("İptal", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Iptal", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Pasif", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Tükendi", StringComparison.OrdinalIgnoreCase))
            {
                backColor = Color.FromArgb(254, 242, 242);
                foreColor = Color.FromArgb(185, 28, 28);
                return;
            }

            if (value.Equals("Hazırlanıyor", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Hazirlaniyor", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("Kritik", StringComparison.OrdinalIgnoreCase))
            {
                backColor = Color.FromArgb(255, 247, 237);
                foreColor = Color.FromArgb(194, 65, 12);
                return;
            }

            if (value.Equals("Kargoda", StringComparison.OrdinalIgnoreCase))
            {
                backColor = Color.FromArgb(239, 246, 255);
                foreColor = Color.FromArgb(29, 78, 216);
                return;
            }

            backColor = Color.FromArgb(243, 244, 246);
            foreColor = Color.FromArgb(75, 85, 99);
        }
    }
}
