using System;
using System.Configuration;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Pages
{
    public class SettingsPage : UserControl, IThemeable
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();

        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblApiStatus;
        private Label lblThemeStatus;
        private Label lblProfileStatus;
        private Button btnTestApi;
        private Button btnRefreshInfo;

        public SettingsPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            BuildLayout();
            ApplyTheme();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            Panel header = BuildHeader();
            TableLayoutPanel grid = BuildSettingsGrid();

            Controls.Add(grid);
            Controls.Add(header);

            RefreshInfo();
            ResumeLayout(true);
        }

        private Panel BuildHeader()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = "Ayarlar",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            lblSubtitle = new Label
            {
                Text = "API bağlantısı, profil durumu, oturum ve tema bilgilerini kontrol edin.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 38)
            };

            btnTestApi = CreatePrimaryButton("API Test Et", 130);
            btnTestApi.Click += async (sender, e) => await TestApiAsync();

            btnRefreshInfo = CreateSecondaryButton("Bilgileri Yenile", 140);
            btnRefreshInfo.Click += (sender, e) => RefreshInfo();

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSubtitle);
            header.Controls.Add(btnTestApi);
            header.Controls.Add(btnRefreshInfo);
            header.Resize += (sender, e) =>
            {
                btnTestApi.Location = new Point(header.Width - btnTestApi.Width, 6);
                btnRefreshInfo.Location = new Point(btnTestApi.Left - btnRefreshInfo.Width - 10, 6);
            };

            return header;
        }

        private TableLayoutPanel BuildSettingsGrid()
        {
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 520,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = AppColors.Background
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

            lblApiStatus = CreateValueLabel("");
            lblThemeStatus = CreateValueLabel("");
            lblProfileStatus = CreateValueLabel("");

            grid.Controls.Add(CreateInfoCard("API Bağlantısı", "Admin panel sadece Web API üzerinden çalışır.", lblApiStatus), 0, 0);
            grid.Controls.Add(CreateInfoCard("Profil", "Personelin kendi oturum ve mağaza doluluk durumu.", lblProfileStatus), 1, 0);
            grid.Controls.Add(CreateInfoCard("Oturum", "Aktif kullanıcı ve rol bilgisi.", CreateValueLabel(GetSessionText())), 0, 1);
            grid.Controls.Add(CreateInfoCard("Mağaza Seçimi", "Seçili bayi/mağaza kapsamı.", CreateValueLabel(GetStoreText())), 1, 1);
            grid.Controls.Add(CreateInfoCard("Tema", "Light/Dark tema durumu.", lblThemeStatus), 0, 2);
            grid.Controls.Add(CreateInfoCard("Teslim Kontrolü", "Pazartesi sunumu için hızlı durum özeti.", CreateValueLabel(GetDeliveryChecklistText())), 1, 2);

            return grid;
        }

        private Button CreatePrimaryButton(string text, int width)
        {
            Button button = new Button
            {
                Text = text,
                Width = width,
                Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Button CreateSecondaryButton(string text, int width)
        {
            Button button = CreatePrimaryButton(text, width);
            button.BackColor = AppColors.PrimarySoft;
            button.ForeColor = AppColors.Primary;
            button.FlatAppearance.BorderColor = AppColors.Border;
            return button;
        }

        private ShadowPanel CreateInfoCard(string title, string subtitle, Label valueLabel)
        {
            ShadowPanel panel = new ShadowPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 16, 16),
                Padding = new Padding(22, 18, 22, 18),
                CornerRadius = 12,
                ShadowSize = 4,
                BackColor = AppColors.CardBackground,
                BorderColor = AppColors.Border
            };

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            Label subtitleLabel = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary
            };

            valueLabel.Dock = DockStyle.Fill;

            panel.Controls.Add(valueLabel);
            panel.Controls.Add(subtitleLabel);
            panel.Controls.Add(titleLabel);
            return panel;
        }

        private Label CreateValueLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private async Task TestApiAsync()
        {
            btnTestApi.Enabled = false;
            lblApiStatus.Text = "API bağlantısı test ediliyor...";

            try
            {
                await _apiClient.GetDashboardSummaryAsync(GetCurrentMagazaId(), IsTumMagazalar());
                lblApiStatus.Text = "API çalışıyor. Bağlantı başarılı.\r\n" + GetApiUrl();
            }
            catch (Exception ex)
            {
                lblApiStatus.Text = "API bağlantısı kurulamadı.\r\n" + GetApiUrl();
                MessageBox.Show(ex.Message, "API Bağlantısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnTestApi.Enabled = true;
            }
        }

        private void RefreshInfo()
        {
            lblApiStatus.Text = "API adresi: " + GetApiUrl();
            lblThemeStatus.Text = AppColors.IsDarkMode ? "Koyu tema aktif" : "Açık tema aktif";
            lblProfileStatus.Text = GetProfileText();
        }

        private string GetApiUrl()
        {
            string value = ConfigurationManager.AppSettings["ApiBaseUrl"];
            return string.IsNullOrWhiteSpace(value) ? "http://localhost:5088/api" : value;
        }

        private string GetSessionText()
        {
            return AppSession.AdSoyad + "\r\nRol: " + AppSession.Rol + "\r\nKullanıcı: " + AppSession.KullaniciAdi;
        }

        private string GetStoreText()
        {
            return AppSession.MagazaGorunumAdi + "\r\n" + (AppSession.TumMagazalar ? "Kapsam: Tüm mağazalar" : "Kapsam: Seçili mağaza");
        }

        private string GetProfileText()
        {
            int completed = 0;
            int total = 5;

            if (!string.IsNullOrWhiteSpace(AppSession.KullaniciAdi)) completed++;
            if (!string.IsNullOrWhiteSpace(AppSession.AdSoyad)) completed++;
            if (!string.IsNullOrWhiteSpace(AppSession.Rol)) completed++;
            if (AppSession.MagazaSecildi) completed++;
            if (!string.IsNullOrWhiteSpace(AppSession.ApiToken)) completed++;

            int percent = (int)Math.Round((completed * 100.0) / total);
            return "Profil doluluk: %" + percent +
                   "\r\nAd Soyad: " + AppSession.AdSoyad +
                   "\r\nRol: " + AppSession.Rol;
        }

        private string GetDeliveryChecklistText()
        {
            return "API-only bağlantı aktif\r\nSwagger/API temeli hazır\r\nRol ve mağaza kapsamı oturumdan okunuyor";
        }

        private int? GetCurrentMagazaId()
        {
            if (AppSession.AdminMi && AppSession.TumMagazalar)
                return null;

            return AppSession.SeciliMagazaId;
        }

        private bool IsTumMagazalar()
        {
            return AppSession.AdminMi && (AppSession.TumMagazalar || !AppSession.SeciliMagazaId.HasValue);
        }

        public void ApplyTheme()
        {
            BackColor = AppColors.Background;
            ApplyThemeRecursive(this);
            RefreshInfo();
        }

        private void ApplyThemeRecursive(Control control)
        {
            if (control is TableLayoutPanel table)
                table.BackColor = AppColors.Background;
            else if (control is Panel || control is UserControl)
                control.BackColor = AppColors.Background;

            if (control is ShadowPanel shadowPanel)
            {
                shadowPanel.BackColor = AppColors.CardBackground;
                shadowPanel.BorderColor = AppColors.Border;
            }

            if (control is Label label)
            {
                if (label == lblTitle || label == lblApiStatus || label == lblThemeStatus || label == lblProfileStatus)
                    label.ForeColor = AppColors.TextPrimary;
                else
                    label.ForeColor = AppColors.TextSecondary;
            }

            if (control is Button button)
            {
                if (button == btnTestApi)
                {
                    button.BackColor = AppColors.Primary;
                    button.ForeColor = Color.White;
                }
                else
                {
                    button.BackColor = AppColors.PrimarySoft;
                    button.ForeColor = AppColors.Primary;
                }
            }

            foreach (Control child in control.Controls)
                ApplyThemeRecursive(child);
        }
    }
}
