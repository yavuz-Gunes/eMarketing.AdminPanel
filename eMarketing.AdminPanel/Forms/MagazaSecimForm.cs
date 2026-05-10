using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Forms
{
    public class MagazaSecimForm : Form
    {
        private readonly ApiDataClient apiClient = new ApiDataClient();

        private FlowLayoutPanel kartListesi;
        private TextBox txtArama;
        private Button btnTumMagazalar;
        private Button btnVazgec;
        private Label lblBaslik;
        private Label lblAciklama;
        private Label lblBosDurum;

        public bool SecimYapildi { get; private set; }

        public MagazaSecimForm()
        {
            Text = "Mağaza Seçimi";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = true;
            MinimizeBox = false;
            MinimumSize = new Size(980, 700);
            ClientSize = new Size(1180, 760);
            BackColor = AppColors.Background;

            BuildUi();
            Load += MagazaSecimForm_Load;
        }

        private void BuildUi()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 118,
                Padding = new Padding(24, 20, 24, 12),
                BackColor = AppColors.CardBackground
            };

            lblBaslik = new Label
            {
                Text = "Mağaza Seçimi",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary
            };

            lblAciklama = new Label
            {
                Text = AppSession.AdminMi
                    ? "Çalışmak istediğiniz mağazayı seçin ya da tüm mağazalarla devam edin."
                    : "Yetkili olduğunuz mağazalardan birini seçin.",
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = AppColors.TextSecondary
            };

            txtArama = new TextBox
            {
                Width = 360,
                Height = 28,
                Left = 24,
                Top = 78,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtArama.TextChanged += TxtArama_TextChanged;
            ButtonStyleHelper.ApplyInput(txtArama);

            header.Controls.Add(txtArama);
            header.Controls.Add(lblAciklama);
            header.Controls.Add(lblBaslik);

            kartListesi = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(24, 22, 24, 12),
                BackColor = AppColors.Background
            };
            kartListesi.SizeChanged += (sender, e) => RefreshCardWidths();

            lblBosDurum = new Label
            {
                Text = "Yetkili mağaza bulunamadı.",
                AutoSize = false,
                Width = 780,
                Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppColors.TextSecondary,
                Visible = false
            };

            Panel footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 74,
                Padding = new Padding(24, 14, 24, 16),
                BackColor = AppColors.CardBackground
            };

            btnTumMagazalar = new Button
            {
                Text = "Tüm Mağazalar",
                Width = 170,
                Height = 38,
                Left = 24,
                Top = 16,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = AppSession.AdminMi
            };
            btnTumMagazalar.Click += BtnTumMagazalar_Click;

            btnVazgec = new Button
            {
                Text = "Vazgeç",
                Width = 120,
                Height = 38,
                Left = ClientSize.Width - 150,
                Top = 16,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnVazgec.Click += (sender, e) => Close();

            StyleButton(btnTumMagazalar, true);
            StyleButton(btnVazgec, false);

            footer.Controls.Add(btnTumMagazalar);
            footer.Controls.Add(btnVazgec);

            Controls.Add(kartListesi);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private async void MagazaSecimForm_Load(object sender, EventArgs e)
        {
            await MagazalariYukleAsync();
        }

        private async void TxtArama_TextChanged(object sender, EventArgs e)
        {
            await MagazalariYukleAsync();
        }

        private async Task MagazalariYukleAsync()
        {
            try
            {
                kartListesi.SuspendLayout();
                kartListesi.Controls.Clear();

                DataTable table = await GetMagazaSecimListesiAsync();

                if (table.Rows.Count == 0)
                {
                    lblBosDurum.Visible = true;
                    kartListesi.Controls.Add(lblBosDurum);
                    return;
                }

                lblBosDurum.Visible = false;

                foreach (DataRow row in table.Rows)
                {
                    kartListesi.Controls.Add(CreateMagazaCard(row));
                }
                RefreshCardWidths();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                kartListesi.ResumeLayout(true);
            }
        }

        private Panel CreateMagazaCard(DataRow row)
        {
            Panel card = new Panel
            {
                Width = 310,
                Height = 158,
                Margin = new Padding(0, 0, 16, 16),
                Padding = new Padding(16),
                BackColor = AppColors.CardBackground,
                Cursor = Cursors.Hand,
                Tag = row
            };

            Label magazaAdi = CreateLabel(GetText(row, "MagazaAdi", "Mağaza"), 11F, FontStyle.Bold, AppColors.TextPrimary, 42);
            Label musteriAdi = CreateLabel(GetText(row, "MusteriAdi", "Müşteri"), 9F, FontStyle.Regular, AppColors.TextSecondary, 24);
            Label konum = CreateLabel(GetKonumText(row), 8.5F, FontStyle.Regular, AppColors.TextMuted, 22);
            Label ozet = CreateLabel(GetOzetText(row), 8.5F, FontStyle.Bold, AppColors.Primary, 24);

            magazaAdi.Dock = DockStyle.Top;
            musteriAdi.Dock = DockStyle.Top;
            konum.Dock = DockStyle.Top;
            ozet.Dock = DockStyle.Bottom;

            card.Controls.Add(ozet);
            card.Controls.Add(konum);
            card.Controls.Add(musteriAdi);
            card.Controls.Add(magazaAdi);

            AttachCardClick(card, row);

            card.MouseEnter += (sender, e) => card.BackColor = AppColors.PrimarySoft;
            card.MouseLeave += (sender, e) => card.BackColor = AppColors.CardBackground;
            card.Paint += Card_Paint;

            return card;
        }

        private Task<DataTable> GetMagazaSecimListesiAsync()
        {
            return apiClient.GetMagazaSecimListesiAsync(
                txtArama.Text.Trim(),
                true,
                AppSession.KullaniciId,
                AppSession.AdminMi);
        }

        private Label CreateLabel(string text, float size, FontStyle style, Color color, int height)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Height = height,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };
        }

        private void AttachCardClick(Control control, DataRow row)
        {
            control.Click += (sender, e) => MagazaSec(row);

            foreach (Control child in control.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += (sender, e) => MagazaSec(row);
            }
        }

        private void MagazaSec(DataRow row)
        {
            AppSession.MagazaSec(
                Convert.ToInt32(row["MusteriId"]),
                Convert.ToInt32(row["MagazaId"]),
                Convert.ToString(row["MusteriAdi"]),
                Convert.ToString(row["MagazaAdi"]),
                Convert.ToString(row["Sehir"]));

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

        private string GetKonumText(DataRow row)
        {
            string sehir = GetText(row, "Sehir", "");
            string ilce = GetText(row, "Ilce", "");

            if (!string.IsNullOrWhiteSpace(sehir) && !string.IsNullOrWhiteSpace(ilce))
                return sehir + " / " + ilce;

            if (!string.IsNullOrWhiteSpace(sehir))
                return sehir;

            return "Konum girilmemiş";
        }

        private string GetOzetText(DataRow row)
        {
            int siparisSayisi = GetInt(row, "SiparisSayisi");
            decimal toplamCiro = GetDecimal(row, "ToplamCiro");
            return siparisSayisi + " sipariş  |  " + toplamCiro.ToString("N2", new CultureInfo("tr-TR")) + " TL";
        }

        private string GetText(DataRow row, string columnName, string defaultValue)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            string value = Convert.ToString(row[columnName]);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private int GetInt(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(row[columnName]);
        }

        private decimal GetDecimal(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            return Convert.ToDecimal(row[columnName]);
        }

        private void StyleButton(Button button, bool primary)
        {
            if (primary)
                ButtonStyleHelper.ApplyPrimary(button);
            else
                ButtonStyleHelper.ApplyOutline(button);
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Control card = sender as Control;
            if (card == null)
                return;

            using (Pen pen = new Pen(AppColors.Border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            }
        }

        private void RefreshCardWidths()
        {
            if (kartListesi == null || kartListesi.Controls.Count == 0)
                return;

            int available = kartListesi.ClientSize.Width - 24;
            int columnCount = Math.Max(1, available / 330);
            int cardWidth = Math.Max(280, (available / columnCount) - 16);

            foreach (Control control in kartListesi.Controls)
            {
                if (control is Panel)
                    control.Width = cardWidth;
            }
        }
    }
}
