using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Forms
{
    public class MagazaSecimForm : Form
    {
        private readonly MagazaRepository _repo = new MagazaRepository();

        private ListBox lstMagazalar;
        private Button btnSec;
        private Button btnTumMagazalar;

        public bool SecimYapildi { get; private set; }

        public MagazaSecimForm()
        {
            Text = "Mağaza Seçimi";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 420);

            BuildUi();
            Load += MagazaSecimForm_Load;
        }

        private void BuildUi()
        {
            lstMagazalar = new ListBox { Left = 20, Top = 20, Width = 480, Height = 320, DisplayMember = "MagazaGosterim" };
            btnSec = new Button { Text = "Seçili Mağazayı Kullan", Left = 20, Top = 356, Width = 230, Height = 34 };
            btnTumMagazalar = new Button { Text = "Tüm Mağazalar", Left = 270, Top = 356, Width = 230, Height = 34 };

            btnSec.Click += BtnSec_Click;
            btnTumMagazalar.Click += BtnTumMagazalar_Click;

            Controls.Add(lstMagazalar);
            Controls.Add(btnSec);
            Controls.Add(btnTumMagazalar);
        }

        private void MagazaSecimForm_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable table = _repo.GetAktifMagazalar();
                table.Columns.Add("MagazaGosterim", typeof(string));

                foreach (DataRow row in table.Rows)
                {
                    string musteri = Convert.ToString(row["MusteriAdi"]);
                    string magaza = Convert.ToString(row["MagazaAdi"]);
                    string sehir = Convert.ToString(row["Sehir"]);
                    row["MagazaGosterim"] = string.Format("{0} - {1} ({2})", musteri, magaza, sehir);
                }

                lstMagazalar.DataSource = table;
                lstMagazalar.ValueMember = "MagazaId";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSec_Click(object sender, EventArgs e)
        {
            DataRowView selected = lstMagazalar.SelectedItem as DataRowView;
            if (selected == null)
            {
                MessageBox.Show("Lütfen bir mağaza seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppSession.MagazaSec(
                Convert.ToInt32(selected["MusteriId"]),
                Convert.ToInt32(selected["MagazaId"]),
                Convert.ToString(selected["MusteriAdi"]),
                Convert.ToString(selected["MagazaAdi"]),
                Convert.ToString(selected["Sehir"]));

            SecimYapildi = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnTumMagazalar_Click(object sender, EventArgs e)
        {
            AppSession.TumMagazalariSec();
            SecimYapildi = true;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
