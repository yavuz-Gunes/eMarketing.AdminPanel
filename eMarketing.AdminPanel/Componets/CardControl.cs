using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Componets
{
    public class CardControl : UserControl
    {
        private Label lblTitle;
        private Label lblValue;
        private Label lblDescription;

        private string _title = "";
        private string _value = "0";
        private string _description = "";

        public string TitleText
        {
            get => _title;
            set
            {
                _title = value;
                if (lblTitle != null)
                    lblTitle.Text = value;

                Invalidate();
            }
        }

        public string ValueText
        {
            get => _value;
            set
            {
                _value = value;
                if (lblValue != null)
                    lblValue.Text = value;

                Invalidate();
            }
        }

        public string DescriptionText
        {
            get => _description;
            set
            {
                _description = value;
                if (lblDescription != null)
                    lblDescription.Text = value;

                Invalidate();
            }
        }

        public CardControl()
        {
            Width = 330;
            Height = 118;
            Margin = new Padding(0, 0, 22, 22);
            Padding = new Padding(28, 22, 28, 18);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BuildLayout();
            ApplyTheme();
        }

        public CardControl(string title, string value) : this()
        {
            SetData(title, value);
        }

        public CardControl(string title, string value, string description) : this()
        {
            SetData(title, value, description);
        }

        private void BuildLayout()
        {
            Controls.Clear();

            lblTitle = new Label
            {
                Text = _title,
                AutoSize = false,
                Height = 24,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblValue = new Label
            {
                Text = _value,
                AutoSize = false,
                Height = 42,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblDescription = new Label
            {
                Text = _description,
                AutoSize = false,
                Height = 24,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            Controls.Add(lblDescription);
            Controls.Add(lblValue);
            Controls.Add(lblTitle);
        }

        public void SetData(string title, string value)
        {
            _title = title;
            _value = value;
            _description = GetDefaultDescription(title);

            UpdateLabels();
        }

        public void SetData(string title, string value, string description)
        {
            _title = title;
            _value = value;
            _description = description;

            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (lblTitle != null)
                lblTitle.Text = _title;

            if (lblValue != null)
            {
                lblValue.Text = _value;
                lblValue.Font = new Font("Segoe UI", GetValueFontSize(_value), FontStyle.Bold);
            }

            if (lblDescription != null)
                lblDescription.Text = _description;

            Invalidate();
        }

        private float GetValueFontSize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 22F;

            if (value.Length > 14)
                return 17F;

            if (value.Length > 10)
                return 19F;

            return 22F;
        }

        private string GetDefaultDescription(string title)
        {
            switch (title)
            {
                case "Toplam Ürün":
                    return "Tüm parça kayıtları";

                case "Aktif Ürün":
                    return "Yayında olan parçalar";

                case "Kritik Stok":
                    return "Stok uyarısı";

                case "Toplam Kategori":
                    return "Tüm kategori kayıtları";

                case "Aktif Kategori":
                    return "Kullanımdaki kategoriler";

                case "Toplam Sipariş":
                    return "Tüm sipariş kayıtları";

                default:
                    return "";
            }
        }

        public void ApplyTheme()
        {
            BackColor = Color.Transparent;

            if (lblTitle != null)
                lblTitle.ForeColor = AppColors.TextSecondary;

            if (lblValue != null)
                lblValue.ForeColor = AppColors.TextPrimary;

            if (lblDescription != null)
                lblDescription.ForeColor = AppColors.TextSecondary;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle shadowRect = new Rectangle(4, 5, Width - 8, Height - 8);
            Rectangle cardRect = new Rectangle(0, 0, Width - 6, Height - 6);

            using (GraphicsPath shadowPath = GetRoundedRectanglePath(shadowRect, 18))
            using (SolidBrush shadowBrush = new SolidBrush(GetShadowColor()))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            using (GraphicsPath cardPath = GetRoundedRectanglePath(cardRect, 18))
            using (SolidBrush cardBrush = new SolidBrush(GetCardBackColor()))
            using (Pen borderPen = new Pen(AppColors.Border))
            {
                e.Graphics.FillPath(cardBrush, cardPath);
                e.Graphics.DrawPath(borderPen, cardPath);
            }
        }

        private Color GetCardBackColor()
        {
            if (AppColors.IsDarkMode)
                return Color.FromArgb(22, 29, 40);

            return Color.White;
        }

        private Color GetShadowColor()
        {
            if (AppColors.IsDarkMode)
                return Color.FromArgb(35, 0, 0, 0);

            return Color.FromArgb(14, 30, 41, 59);
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
