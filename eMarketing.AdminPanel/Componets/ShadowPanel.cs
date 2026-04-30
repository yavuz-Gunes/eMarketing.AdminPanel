using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class ShadowPanel : Panel
    {
        public int CornerRadius { get; set; } = 16;
        public int ShadowSize { get; set; } = 7;

        public Color ShadowColor { get; set; } = Color.FromArgb(24, 15, 23, 42);
        public Color BorderColor { get; set; } = AppColors.Border;

        public ShadowPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = AppColors.CardBackground;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Color parentBackColor = Parent != null
                ? Parent.BackColor
                : AppColors.Background;

            using (SolidBrush brush = new SolidBrush(parentBackColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int rightPadding = ShadowSize + 2;
            int bottomPadding = ShadowSize + 2;

            Rectangle shadowRect = new Rectangle(
                ShadowSize,
                ShadowSize,
                Width - rightPadding,
                Height - bottomPadding);

            Rectangle panelRect = new Rectangle(
                0,
                0,
                Width - rightPadding,
                Height - bottomPadding);

            using (GraphicsPath shadowPath = GetRoundedRectanglePath(shadowRect, CornerRadius))
            using (SolidBrush shadowBrush = new SolidBrush(ShadowColor))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            using (GraphicsPath panelPath = GetRoundedRectanglePath(panelRect, CornerRadius))
            using (SolidBrush panelBrush = new SolidBrush(BackColor))
            using (Pen borderPen = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(panelBrush, panelPath);
                e.Graphics.DrawPath(borderPen, panelPath);
            }
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}