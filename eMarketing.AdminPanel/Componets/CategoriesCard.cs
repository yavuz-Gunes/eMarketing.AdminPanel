using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class CategoriesCard : Panel
    {
        private Panel iconCircle;
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
            BackColor = Color.White;
            DoubleBuffered = true;

            // Premium görünüm için border ve gölge
            BorderStyle = BorderStyle.None;

            iconCircle = new Panel
            {
                Size = new Size(52, 52),
                Location = new Point(18, 18),
                BackColor = Color.FromArgb(235, 241, 255)
            };

            // Icon circle'ı yuvarlatılmış yap
            iconCircle.Paint += (s, e) => 
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, iconCircle.Width - 1, iconCircle.Height - 1);
                    iconCircle.Region = new Region(path);
                }
            };

            lblIcon = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 20F, FontStyle.Regular),
                ForeColor = AppColors.Primary,
                Text = "•"
            };

            iconCircle.Controls.Add(lblIcon);

            lblTitle = new Label
            {
                AutoSize = false,
                Location = new Point(78, 16),
                Size = new Size(160, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(107, 114, 128),
                Text = "Başlık"
            };

            lblValue = new Label
            {
                AutoSize = false,
                Location = new Point(78, 35),
                Size = new Size(160, 36),
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(17, 24, 39),
                Text = "0"
            };

            lblHint = new Label
            {
                AutoSize = false,
                Location = new Point(78, 71),
                Size = new Size(160, 18),
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Text = "Genel görünüm"
            };

            Controls.Add(iconCircle);
            Controls.Add(lblTitle);
            Controls.Add(lblValue);
            Controls.Add(lblHint);

            MouseEnter += Card_MouseEnter;
            MouseLeave += Card_MouseLeave;

            foreach (Control ctl in Controls)
            {
                ctl.MouseEnter += Card_MouseEnter;
                ctl.MouseLeave += Card_MouseLeave;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Yuvarlatılmış dikdörtgen çiz
            int borderRadius = 12;
            var path = new GraphicsPath();

            path.AddArc(0, 0, borderRadius * 2, borderRadius * 2, 180, 90);
            path.AddArc(Width - borderRadius * 2, 0, borderRadius * 2, borderRadius * 2, 270, 90);
            path.AddArc(Width - borderRadius * 2, Height - borderRadius * 2, borderRadius * 2, borderRadius * 2, 0, 90);
            path.AddArc(0, Height - borderRadius * 2, borderRadius * 2, borderRadius * 2, 90, 90);
            path.CloseFigure();

            Region = new Region(path);

            // Arka plan
            e.Graphics.FillPath(new SolidBrush(BackColor), path);

            // Gölge efekti (border)
            if (isHovered)
            {
                using (var pen = new Pen(Color.FromArgb(220, 225, 235), 1.5f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
            else
            {
                using (var pen = new Pen(Color.FromArgb(230, 235, 245), 1f))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            path.Dispose();

            base.OnPaint(e);
        }

        private void Card_MouseEnter(object sender, System.EventArgs e)
        {
            isHovered = true;
            BackColor = Color.FromArgb(248, 250, 255);
            lblValue.ForeColor = AppColors.Primary;
            Invalidate();
        }

        private void Card_MouseLeave(object sender, System.EventArgs e)
        {
            isHovered = false;
            BackColor = Color.White;
            lblValue.ForeColor = Color.FromArgb(17, 24, 39);
            Invalidate();
        }


        public void SetData(string icon, string title, string value)
        {
            lblIcon.Text = icon;
            lblTitle.Text = title;
            lblValue.Text = value;

            if (title == "Toplam")
                lblHint.Text = "Tüm kayıtlar";
            else if (title == "Aktif")
                lblHint.Text = "Kullanımda olan";
            else if (title == "Pasif")
                lblHint.Text = "Gizlenen kayıtlar";
            else if (title == "Gösterilen")
                lblHint.Text = "Filtre sonucu";
            else
                lblHint.Text = "";
        }
    }
}