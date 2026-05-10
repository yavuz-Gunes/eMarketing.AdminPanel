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
        private bool _hovered;

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
            Padding = new Padding(28, 16, 28, 14);

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BuildLayout();
            ApplyTheme();

            MouseEnter += CardControl_MouseEnter;
            MouseLeave += CardControl_MouseLeave;
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
                Height = 22,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblValue = new Label
            {
                Text = _value,
                AutoSize = false,
                Height = 36,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblDescription = new Label
            {
                Text = _description,
                AutoSize = false,
                Height = 22,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
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

            if (lblTitle != null)
                lblTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);

            if (lblDescription != null)
                lblDescription.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);

            if (lblDescription != null)
                lblDescription.Text = _description;

            Invalidate();
        }

        private float GetValueFontSize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 20F;

            if (value.Length > 14)
                return 16F;

            if (value.Length > 10)
                return 18F;

            return 20F;
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

            Rectangle shadowRect = new Rectangle(4, _hovered ? 4 : 5, Width - 8, Height - 8);
            Rectangle cardRect = new Rectangle(0, 0, Width - 6, Height - 6);

            using (GraphicsPath shadowPath = GetRoundedRectanglePath(shadowRect, 18))
            using (SolidBrush shadowBrush = new SolidBrush(GetShadowColor()))
            {
                e.Graphics.FillPath(shadowBrush, shadowPath);
            }

            using (GraphicsPath cardPath = GetRoundedRectanglePath(cardRect, 18))
            using (SolidBrush cardBrush = new SolidBrush(GetCardBackColor()))
            using (Pen borderPen = new Pen(_hovered ? AppColors.PrimaryLight : AppColors.Border))
            {
                e.Graphics.FillPath(cardBrush, cardPath);
                e.Graphics.DrawPath(borderPen, cardPath);

                using (SolidBrush accentBrush = new SolidBrush(GetAccentColor()))
                {
                    Rectangle accent = new Rectangle(cardRect.X, cardRect.Y + 18, 4, cardRect.Height - 36);
                    e.Graphics.FillRectangle(accentBrush, accent);
                }
            }
        }

        private void CardControl_MouseEnter(object sender, EventArgs e)
        {
            _hovered = true;
            Invalidate();
        }

        private void CardControl_MouseLeave(object sender, EventArgs e)
        {
            _hovered = false;
            Invalidate();
        }

        private Color GetAccentColor()
        {
            if (_title.IndexOf("Teslim", StringComparison.OrdinalIgnoreCase) >= 0 ||
                _title.IndexOf("Aktif", StringComparison.OrdinalIgnoreCase) >= 0)
                return AppColors.Success;

            if (_title.IndexOf("Kritik", StringComparison.OrdinalIgnoreCase) >= 0 ||
                _title.IndexOf("Bekleyen", StringComparison.OrdinalIgnoreCase) >= 0)
                return AppColors.Warning;

            if (_title.IndexOf("Kargoda", StringComparison.OrdinalIgnoreCase) >= 0)
                return AppColors.Info;

            return AppColors.Primary;
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
