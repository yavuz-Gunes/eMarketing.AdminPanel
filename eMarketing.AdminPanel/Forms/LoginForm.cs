using System;
using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Models;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public class LoginForm : Form
    {
        private readonly LoginRepository _repo = new LoginRepository();

        private TextBox txtKullaniciAdi;
        private TextBox txtSifre;
        private Button btnGiris;

        public LoginForm()
        {
            Text = "Giriş - eMarketing";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(360, 220);

            BuildUi();
        }

        private void BuildUi()
        {
            Label lblKullanici = new Label { Text = "Kullanıcı Adı", Left = 24, Top = 30, Width = 120 };
            txtKullaniciAdi = new TextBox { Left = 24, Top = 52, Width = 312 };

            Label lblSifre = new Label { Text = "Şifre", Left = 24, Top = 90, Width = 120 };
            txtSifre = new TextBox { Left = 24, Top = 112, Width = 312, UseSystemPasswordChar = true };

            btnGiris = new Button { Text = "Giriş Yap", Left = 24, Top = 156, Width = 312, Height = 34 };
            btnGiris.Click += BtnGiris_Click;

            Controls.Add(lblKullanici);
            Controls.Add(txtKullaniciAdi);
            Controls.Add(lblSifre);
            Controls.Add(txtSifre);
            Controls.Add(btnGiris);

            AcceptButton = btnGiris;
        }

        private void BtnGiris_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text;

            if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
            {
                MessageBox.Show("Kullanıcı adı ve şifre zorunludur.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                KullaniciGirisModel kullanici = _repo.GirisYap(kullaniciAdi, sifre);

                if (kullanici == null)
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Giriş Başarısız", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AppSession.GirisBilgisiAyarla(kullanici.KullaniciId, kullanici.KullaniciAdi, kullanici.AdSoyad, kullanici.Rol);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
