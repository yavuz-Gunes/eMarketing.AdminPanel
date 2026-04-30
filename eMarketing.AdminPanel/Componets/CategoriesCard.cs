using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class CategoriesCard : Panel
    {
        private Panel iconBox;
        private Label lblIcon;
        private Label lblTitle;
        private Label lblValue;
        private Label lblHint;

        private bool isHovered = false;

        public CategoriesCard()
        {
            Height = 110;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            BackColor = AppColors.CardBackground;
            DoubleBuffered = true;

            iconBox = new Panel
            {
                Size = new Size(54, 54),
                Location = new Point(18, 22),
                BackColor = AppColors.PrimarySoft
            };

            iconBox.Paint += IconBox_Paint;

            lblIcon = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 20F, FontStyle.Regular),
                ForeColor = AppColors.Primary,
                Text = "•"
            };

            iconBox.Controls.Add(lblIcon);

            lblTitle = new Label
            {
                AutoSize = false,
                Location = new Point(86, 18),
                Size = new Size(180, 22),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                Text = "Başlık"
            };

            lblValue = new Label
            {
                AutoSize = false,
                Location = new Point(86, 40),
                Size = new Size(180, 38),
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                Text = "0"
            };

            lblHint = new Label
            {
                AutoSize = false,
                Location = new Point(86, 78),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = AppColors.TextMuted,
                Text = "Genel görünüm"
            };

            Controls.Add(iconBox);
            Controls.Add(lblTitle);
            Controls.Add(lblValue);
            Controls.Add(lblHint);

            MouseEnter += Card_MouseEnter;
            MouseLeave += Card_MouseLeave;

            foreach (Control control in Controls)
            {
                control.MouseEnter += Card_MouseEnter;
                control.MouseLeave += Card_MouseLeave;
            }
        }

        public void SetData(string icon, string title, string value)
        {
            lblIcon.Text = icon;
            lblTitle.Text = title;
            lblValue.Text = value;
            lblHint.Text = GetHint(title);

            ApplyIconColor(title);
        }

        private string GetHint(string title)
        {
            switch (title)
            {
                case "Toplam":
                    return "Tüm kayıtlar";
                case "Aktif":
                    return "Kullanımda olan";
                case "Pasif":
                    return "Gizlenen kayıtlar";
                case "Gösterilen":
                    return "Filtre sonucu";
                case "Kritik Stok":
                    return "Stok uyarısı";
                case "Hazırlanıyor":
                    return "Bekleyen siparişler";
                case "Kargoda":
                    return "Yolda olanlar";
                case "Teslim":
                    return "Tamamlanan";
                case "İptal":
                    return "İptal edilenler";
                default:
                    return "Genel görünüm";
            }
        }

        private void ApplyIconColor(string title)
        {
            if (title == "Aktif" || title == "Teslim")
            {
                iconBox.BackColor = AppColors.SuccessSoft;
                lblIcon.ForeColor = AppColors.Success;
            }
            else if (title == "Pasif" || title == "İptal")
            {
                iconBox.BackColor = AppColors.DangerSoft;
                lblIcon.ForeColor = AppColors.Danger;
            }
            else if (title == "Kritik Stok" || title == "Hazırlanıyor")
            {
                iconBox.BackColor = AppColors.WarningSoft;
                lblIcon.ForeColor = AppColors.Warning;
            }
            else if (title == "Kargoda" || title == "Gösterilen")
            {
                iconBox.BackColor = AppColors.InfoSoft;
                lblIcon.ForeColor = AppColors.Info;
            }
            else
            {
                iconBox.BackColor = AppColors.PrimarySoft;
                lblIcon.ForeColor = AppColors.Primary;
            }
        }

        private void IconBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = GetRoundedRectanglePath(iconBox.ClientRectangle, 14))
            {
                iconBox.Region = new Region(path);

                using (SolidBrush brush = new SolidBrush(iconBox.BackColor))
                    e.Graphics.FillPath(brush, path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = GetRoundedRectanglePath(ClientRectangle, 14))
            {
                Region = new Region(path);

                using (SolidBrush brush = new SolidBrush(BackColor))
                    e.Graphics.FillPath(brush, path);

                using (Pen pen = new Pen(isHovered ? AppColors.PrimaryLight : AppColors.Border, isHovered ? 1.4f : 1f))
                    e.Graphics.DrawPath(pen, path);
            }

            base.OnPaint(e);
        }

        private void Card_MouseEnter(object sender, System.EventArgs e)
        {
            isHovered = true;
            BackColor = Color.FromArgb(248, 250, 255);
            Invalidate();
        }

        private void Card_MouseLeave(object sender, System.EventArgs e)
        {
            isHovered = false;
            BackColor = AppColors.CardBackground;
            Invalidate();
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter - 1, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter - 1, bounds.Bottom - diameter - 1, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter - 1, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}