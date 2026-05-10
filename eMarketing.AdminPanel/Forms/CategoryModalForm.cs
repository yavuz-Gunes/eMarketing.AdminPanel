using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Services;

namespace eMarketing.AdminPanel.Forms
{
    public partial class CategoryModalForm : Form
    {
        private readonly ApiDataClient _apiClient = new ApiDataClient();
        private readonly int _categoryId;

        private Label lblTitle;
        private Label lblCategoryName;
        private TextBox txtCategoryName;
        private CheckBox chkIsActive;
        private Button btnSave;
        private Button btnCancel;
        private Panel footerPanel;

        public bool IsSaved { get; private set; }

        public CategoryModalForm(int categoryId = 0)
        {
            _categoryId = categoryId;

            InitializeLayout();

            Load += CategoryModalForm_Load;
        }

        private void InitializeLayout()
        {
            Text = _categoryId > 0 ? "Kategori Düzenle" : "Yeni Kategori";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            Width = 460;
            Height = 270;
            BackColor = AppColors.Background;

            lblTitle = new Label
            {
                Text = _categoryId > 0 ? "Kategori Düzenle" : "Yeni Kategori",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = false,
                Height = 36,
                Dock = DockStyle.Top,
                Padding = new Padding(20, 16, 20, 0)
            };

            lblCategoryName = new Label
            {
                Text = "Kategori Adı",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(24, 70)
            };

            txtCategoryName = new TextBox
            {
                Location = new Point(24, 94),
                Width = 390,
                Font = new Font("Segoe UI", 10F),
            };
            ButtonStyleHelper.ApplyInput(txtCategoryName);

            chkIsActive = new CheckBox
            {
                Text = "Aktif",
                Location = new Point(24, 136),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                Checked = true
            };

            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = AppColors.CardBackground
            };

            btnCancel = new Button
            {
                Text = "İptal",
                Width = 100,
                Height = 36,
                Location = new Point(204, 14),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderColor = Color.Gainsboro;
            ButtonStyleHelper.ApplyOutline(btnCancel);
            btnCancel.Click += (s, e) => Close();

            btnSave = new Button
            {
                Text = "Kaydet",
                Width = 100,
                Height = 36,
                Location = new Point(314, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White
            };
            btnSave.FlatAppearance.BorderSize = 0;
            ButtonStyleHelper.ApplyPrimary(btnSave);
            btnSave.Click += BtnSave_Click;

            footerPanel.Controls.Add(btnCancel);
            footerPanel.Controls.Add(btnSave);

            Controls.Add(footerPanel);
            Controls.Add(chkIsActive);
            Controls.Add(txtCategoryName);
            Controls.Add(lblCategoryName);
            Controls.Add(lblTitle);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private async void CategoryModalForm_Load(object sender, EventArgs e)
        {
            if (_categoryId <= 0)
                return;

            try
            {
                DataRow row = await GetCategoryByIdAsync(_categoryId);

                if (row == null)
                {
                    MessageBox.Show("Kategori bulunamadı.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }

                txtCategoryName.Text = row["KategoriAdi"]?.ToString();
                chkIsActive.Checked = row["AktifMi"] != DBNull.Value && Convert.ToBoolean(row["AktifMi"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategori bilgisi yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string categoryName = txtCategoryName.Text.Trim();

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    MessageBox.Show("Kategori adı boş bırakılamaz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCategoryName.Focus();
                    return;
                }

                if (!IsValidCategoryName(categoryName))
                {
                    MessageBox.Show("Kategori adı yalnızca harf, boşluk ve izin verilen karakterleri içerebilir. Sayı ve özel karakterler kullanılamaz.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCategoryName.Focus();
                    return;
                }

                if (_categoryId > 0)
                {
                    await UpdateCategoryAsync(_categoryId, categoryName, chkIsActive.Checked);
                }
                else
                {
                    await InsertCategoryAsync(categoryName);
                }

                IsSaved = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategori kaydedilirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsValidCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return false;

            foreach (char c in categoryName)
            {
                if (char.IsDigit(c))
                    return false;

                if (!char.IsLetter(c)
                    && !char.IsWhiteSpace(c)
                    && c != '-'
                    && c != '('
                    && c != ')'
                    && c != '&'
                    && c != ',')
                {
                    return false;
                }
            }

            return true;
        }

        private Task<DataRow> GetCategoryByIdAsync(int categoryId)
        {
            return _apiClient.GetCategoryByIdAsync(categoryId);
        }

        private Task InsertCategoryAsync(string categoryName)
        {
            return _apiClient.InsertCategoryAsync(categoryName);
        }

        private Task UpdateCategoryAsync(int categoryId, string categoryName, bool isActive)
        {
            return _apiClient.UpdateCategoryAsync(categoryId, categoryName, isActive);
        }
    }
}
