using System;
using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;
using eMarketing.Data.Models;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public class LoginForm : Form
    {
        private readonly LoginRepository repo = new LoginRepository();
        private readonly ApiDataClient apiClient = new ApiDataClient();

        private TextBox txtKullaniciAdi;
        private TextBox txtSifre;
        private Button btnGiris;
        private Label lblMesaj;

        public LoginForm()
        {
            Text = "Giriş - eMarketing";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ClientSize = new Size(760, 460);
            BackColor = AppColors.Background;

            BuildUi();
        }

        private void BuildUi()
        {
            Panel solPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = AppColors.Primary,
                Padding = new Padding(30)
            };

            Label lblMarka = new Label
            {
                Text = "eMarketing",
                Dock = DockStyle.Top,
                Height = 48,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            Label lblMarkaAlt = new Label
            {
                Text = "Bayi, stok ve sipariş yönetimi",
                Dock = DockStyle.Top,
                Height = 58,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(232, 240, 255),
                BackColor = Color.Transparent
            };

            Label lblSurum = new Label
            {
                Text = "Yönetim Paneli",
                Dock = DockStyle.Bottom,
                Height = 34,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(232, 240, 255),
                BackColor = Color.Transparent
            };

            solPanel.Controls.Add(lblSurum);
            solPanel.Controls.Add(lblMarkaAlt);
            solPanel.Controls.Add(lblMarka);

            Panel formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppColors.CardBackground,
                Padding = new Padding(54, 48, 54, 42)
            };

            Label lblBaslik = new Label
            {
                Text = "Giriş Yap",
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("Segoe UI", 19F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                BackColor = Color.Transparent
            };

            Label lblAlt = new Label
            {
                Text = "Hesabınızla devam edin.",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };

            Label lblKullanici = CreateLabel("Kullanıcı Adı", 0, 106);
            txtKullaniciAdi = CreateTextBox(0, 130);

            Label lblSifre = CreateLabel("Şifre", 0, 188);
            txtSifre = CreateTextBox(0, 212);
            txtSifre.UseSystemPasswordChar = true;

            lblMesaj = new Label
            {
                Text = "",
                Location = new Point(0, 258),
                Width = 340,
                Height = 26,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = AppColors.Danger,
                BackColor = Color.Transparent
            };

            btnGiris = new Button
            {
                Text = "Giriş Yap",
                Location = new Point(0, 298),
                Width = 340,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGiris.FlatAppearance.BorderSize = 0;
            btnGiris.Click += BtnGiris_Click;

            formPanel.Controls.Add(btnGiris);
            formPanel.Controls.Add(lblMesaj);
            formPanel.Controls.Add(txtSifre);
            formPanel.Controls.Add(lblSifre);
            formPanel.Controls.Add(txtKullaniciAdi);
            formPanel.Controls.Add(lblKullanici);
            formPanel.Controls.Add(lblAlt);
            formPanel.Controls.Add(lblBaslik);

            Controls.Add(formPanel);
            Controls.Add(solPanel);

            AcceptButton = btnGiris;
            Shown += (sender, e) => txtKullaniciAdi.Focus();
        }

        private Label CreateLabel(string text, int left, int top)
        {
            return new Label
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 340,
                Height = 22,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                BackColor = Color.Transparent
            };
        }

        private TextBox CreateTextBox(int left, int top)
        {
            return new TextBox
            {
                Left = left,
                Top = top,
                Width = 340,
                Height = 32,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = AppColors.InputBackground,
                ForeColor = AppColors.TextPrimary
            };
        }

        private void BtnGiris_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text;

            lblMesaj.Text = "";

            if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
            {
                lblMesaj.Text = "Kullanıcı adı ve şifre zorunludur.";
                return;
            }

            try
            {
                btnGiris.Enabled = false;
                btnGiris.Text = "Kontrol ediliyor...";

                string apiToken;
                KullaniciGirisModel kullanici = GirisYap(kullaniciAdi, sifre, out apiToken);

                if (kullanici == null)
                {
                    lblMesaj.Text = "Kullanıcı adı veya şifre hatalı.";
                    return;
                }

                AppSession.GirisBilgisiAyarla(
                    kullanici.KullaniciId,
                    kullanici.KullaniciAdi,
                    kullanici.AdSoyad,
                    kullanici.Rol,
                    apiToken);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGiris.Enabled = true;
                btnGiris.Text = "Giriş Yap";
            }
        }

        private KullaniciGirisModel GirisYap(string kullaniciAdi, string sifre, out string apiToken)
        {
            apiToken = "";

            try
            {
                return apiClient.Login(kullaniciAdi, sifre, out apiToken);
            }
            catch (Exception ex)
            {
                ApiFallbackReporter.Report("Kullanıcı girişi", ex);
                return repo.GirisYap(kullaniciAdi, sifre);
            }
        }
    }
}
