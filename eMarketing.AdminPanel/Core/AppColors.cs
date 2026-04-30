using System.Drawing;

namespace eMarketing.AdminPanel.Core
{
    public static class AppColors
    {
        public static bool IsDarkMode { get; private set; } = false;

        public static void SetDarkMode(bool enabled)
        {
            IsDarkMode = enabled;
        }

        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        // Ana renk
        public static Color Primary =>
            IsDarkMode ? Color.FromArgb(96, 165, 250) : Color.FromArgb(59, 130, 246);

        public static Color PrimaryDark =>
            IsDarkMode ? Color.FromArgb(59, 130, 246) : Color.FromArgb(37, 99, 235);

        public static Color PrimaryLight =>
            IsDarkMode ? Color.FromArgb(30, 64, 175) : Color.FromArgb(219, 234, 254);

        public static Color PrimarySoft =>
            IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(239, 246, 255);

        // Arka planlar
        public static Color Background =>
            IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.FromArgb(244, 247, 251);

        public static Color Surface =>
            IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.White;

        public static Color CardBackground =>
            IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.White;

        public static Color SidebarBackground =>
            IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.White;

        public static Color TopbarBackground =>
            IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.White;

        // Eski kod uyumluluğu
        public static Color Card => CardBackground;
        public static Color Sidebar => SidebarBackground;

        // Yazılar
        public static Color TextPrimary =>
            IsDarkMode ? Color.FromArgb(241, 245, 249) : Color.FromArgb(17, 24, 39);

        public static Color TextSecondary =>
            IsDarkMode ? Color.FromArgb(203, 213, 225) : Color.FromArgb(107, 114, 128);

        public static Color TextMuted =>
            IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(148, 163, 184);

        // Kenarlıklar
        public static Color Border =>
            IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(229, 231, 235);

        public static Color BorderSoft =>
            IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);

        // Durum renkleri
        public static Color Success =>
            IsDarkMode ? Color.FromArgb(74, 222, 128) : Color.FromArgb(22, 163, 74);

        public static Color SuccessSoft =>
            IsDarkMode ? Color.FromArgb(20, 83, 45) : Color.FromArgb(220, 252, 231);

        public static Color Warning =>
            IsDarkMode ? Color.FromArgb(251, 191, 36) : Color.FromArgb(245, 158, 11);

        public static Color WarningSoft =>
            IsDarkMode ? Color.FromArgb(120, 53, 15) : Color.FromArgb(255, 247, 237);

        public static Color Danger =>
            IsDarkMode ? Color.FromArgb(248, 113, 113) : Color.FromArgb(239, 68, 68);

        public static Color DangerSoft =>
            IsDarkMode ? Color.FromArgb(127, 29, 29) : Color.FromArgb(254, 242, 242);

        public static Color Info =>
            IsDarkMode ? Color.FromArgb(56, 189, 248) : Color.FromArgb(14, 165, 233);

        public static Color InfoSoft =>
            IsDarkMode ? Color.FromArgb(12, 74, 110) : Color.FromArgb(224, 242, 254);

        // Menü renkleri
        public static Color MenuText =>
            IsDarkMode ? Color.FromArgb(203, 213, 225) : Color.FromArgb(71, 85, 105);

        public static Color MenuTextActive =>
            Primary;

        public static Color MenuActiveBackground =>
            IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(224, 231, 255);

        public static Color MenuHoverBackground =>
            IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(248, 250, 252);

        // Buton / input
        public static Color InputBackground =>
            IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.White;

        public static Color InputBorder =>
            IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(209, 213, 219);

        public static Color ButtonText =>
            Color.White;
    }
}