using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using eMarketing.AdminPanel.Componets;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;
using eMarketing.Data.Models;
using eMarketing.Data.Repositories;

namespace eMarketing.AdminPanel.Pages
{
    public class OrdersPage : UserControl
    {
        private readonly OrderRepository _repo = new OrderRepository();

        private Panel headerPanel;
        private Panel statsPanel;
        private Panel filterPanel;
        private Panel gridPanel;
        private Panel footerPanel;

        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnNewOrder;

        private CategoriesCard cTotal;
        private CategoriesCard cPreparing;
        private CategoriesCard cShipped;
        private CategoriesCard cDelivered;
        private CategoriesCard cCancelled;

        private DataGridView dgvOrders;

        private Label lblSelectedOrder;
        private TextBox txtOrderId;
        private ComboBox cmbStatus;
        private Button btnUpdateStatus;

        public OrdersPage()
        {
            Dock = DockStyle.Fill;
            BackColor = AppColors.Background;
            Padding = new Padding(24, 18, 24, 18);

            BuildLayout();
            Load += OrdersPage_Load;
        }

        private void OrdersPage_Load(object sender, EventArgs e)
        {
            LoadStatuses();
            LoadOrderSummary();
            LoadOrders();
        }

        private void BuildLayout()
        {
            BuildHeaderPanel();
            BuildStatsPanel();
            BuildFilterPanel();
            BuildGridPanel();
            BuildFooterPanel();

            Controls.Add(gridPanel);
            Controls.Add(footerPanel);
            Controls.Add(filterPanel);
            Controls.Add(statsPanel);
            Controls.Add(headerPanel);
        }

        private void BuildHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = AppColors.Background
            };

            lblTitle = new Label
            {
                Text = "Siparişler",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.TextPrimary,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            lblSubtitle = new Label
            {
                Text = "Sipariş kayıtlarını görüntüleyin ve durum güncelleyin.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                AutoSize = true,
                Location = new Point(2, 34)
            };

            btnNewOrder = new Button
            {
                Text = "+ Yeni Sipariş",
                Width = 150,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White
            };
            btnNewOrder.FlatAppearance.BorderSize = 0;
            btnNewOrder.Click += BtnNewOrder_Click;

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Controls.Add(btnNewOrder);

            headerPanel.Resize += (s, e) =>
            {
                btnNewOrder.Location = new Point(headerPanel.Width - btnNewOrder.Width, 4);
            };
        }

        private void BuildStatsPanel()
        {
            statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = AppColors.Background
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                BackColor = AppColors.Background,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cTotal = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cPreparing = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cShipped = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cDelivered = new CategoriesCard { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 16, 0) };
            cCancelled = new CategoriesCard { Dock = DockStyle.Fill, Margin = Padding.Empty };

            cTotal.SetData("🧾", "Toplam", "0");
            cPreparing.SetData("🟠", "Hazırlanıyor", "0");
            cShipped.SetData("🚚", "Kargoda", "0");
            cDelivered.SetData("✅", "Teslim", "0");
            cCancelled.SetData("❌", "İptal", "0");

            grid.Controls.Add(cTotal, 0, 0);
            grid.Controls.Add(cPreparing, 1, 0);
            grid.Controls.Add(cShipped, 2, 0);
            grid.Controls.Add(cDelivered, 3, 0);
            grid.Controls.Add(cCancelled, 4, 0);

            statsPanel.Controls.Add(grid);
        }

        private void BuildFilterPanel()
        {
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 18,
                BackColor = AppColors.Background
            };
        }

        private void BuildGridPanel()
        {
            gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoGenerateColumns = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersHeight = 42;
            dgvOrders.RowTemplate.Height = 44;
            dgvOrders.GridColor = Color.Gainsboro;
            dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvOrders.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvOrders.DefaultCellStyle.BackColor = Color.White;
            dgvOrders.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 243, 255);
            dgvOrders.DefaultCellStyle.SelectionForeColor = AppColors.TextPrimary;

            dgvOrders.CellClick += DgvOrders_CellClick;
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;
            dgvOrders.CellPainting += DgvOrders_CellPainting;

            gridPanel.Controls.Add(dgvOrders);
        }

        private void BuildFooterPanel()
        {
            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 76,
                BackColor = Color.White,
                Padding = new Padding(16, 16, 16, 16)
            };

            lblSelectedOrder = new Label
            {
                Text = "Seçili Sipariş ID",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = AppColors.TextSecondary,
                Location = new Point(16, 8)
            };

            txtOrderId = new TextBox
            {
                Location = new Point(16, 30),
                Width = 100,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10F)
            };

            cmbStatus = new ComboBox
            {
                Location = new Point(136, 30),
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };

            btnUpdateStatus = new Button
            {
                Text = "Durum Güncelle",
                Width = 140,
                Height = 34,
                Location = new Point(326, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Primary,
                ForeColor = Color.White
            };
            btnUpdateStatus.FlatAppearance.BorderSize = 0;
            btnUpdateStatus.Click += BtnUpdateStatus_Click;

            footerPanel.Controls.Add(lblSelectedOrder);
            footerPanel.Controls.Add(txtOrderId);
            footerPanel.Controls.Add(cmbStatus);
            footerPanel.Controls.Add(btnUpdateStatus);
        }

        private void LoadOrderSummary()
        {
            try
            {
                OrderSummary summary = _repo.GetOrderSummary();

                cTotal.SetData("🧾", "Toplam", summary.TotalOrders.ToString());
                cPreparing.SetData("🟠", "Hazırlanıyor", summary.PreparingOrders.ToString());
                cShipped.SetData("🚚", "Kargoda", summary.ShippedOrders.ToString());
                cDelivered.SetData("✅", "Teslim", summary.DeliveredOrders.ToString());
                cCancelled.SetData("❌", "İptal", summary.CancelledOrders.ToString());
            }
            catch
            {
                cTotal.SetData("🧾", "Toplam", "0");
                cPreparing.SetData("🟠", "Hazırlanıyor", "0");
                cShipped.SetData("🚚", "Kargoda", "0");
                cDelivered.SetData("✅", "Teslim", "0");
                cCancelled.SetData("❌", "İptal", "0");
            }
        }

        private void LoadOrders()
        {
            try
            {
                DataTable table = _repo.GetAllOrders();
                dgvOrders.DataSource = table;

                if (dgvOrders.Columns.Contains("ProductId"))
                    dgvOrders.Columns["ProductId"].Visible = false;

                if (dgvOrders.Columns.Contains("OrderId"))
                    dgvOrders.Columns["OrderId"].Width = 80;

                if (dgvOrders.Columns.Contains("CustomerName"))
                {
                    dgvOrders.Columns["CustomerName"].HeaderText = "Müşteri";
                    dgvOrders.Columns["CustomerName"].Width = 140;
                }

                if (dgvOrders.Columns.Contains("CustomerEmail"))
                {
                    dgvOrders.Columns["CustomerEmail"].HeaderText = "E-Posta";
                    dgvOrders.Columns["CustomerEmail"].Width = 170;
                }

                if (dgvOrders.Columns.Contains("CustomerPhone"))
                {
                    dgvOrders.Columns["CustomerPhone"].HeaderText = "Telefon";
                    dgvOrders.Columns["CustomerPhone"].Width = 110;
                }

                if (dgvOrders.Columns.Contains("ProductName"))
                {
                    dgvOrders.Columns["ProductName"].HeaderText = "Ürün";
                    dgvOrders.Columns["ProductName"].Width = 170;
                }

                if (dgvOrders.Columns.Contains("Quantity"))
                {
                    dgvOrders.Columns["Quantity"].HeaderText = "Adet";
                    dgvOrders.Columns["Quantity"].Width = 60;
                }

                if (dgvOrders.Columns.Contains("TotalPrice"))
                {
                    dgvOrders.Columns["TotalPrice"].HeaderText = "Tutar";
                    dgvOrders.Columns["TotalPrice"].Width = 90;
                    dgvOrders.Columns["TotalPrice"].DefaultCellStyle.Format = "N2";
                }

                if (dgvOrders.Columns.Contains("OrderStatus"))
                {
                    dgvOrders.Columns["OrderStatus"].HeaderText = "Durum";
                    dgvOrders.Columns["OrderStatus"].Width = 120;
                }

                if (dgvOrders.Columns.Contains("OrderDate"))
                {
                    dgvOrders.Columns["OrderDate"].HeaderText = "Tarih";
                    dgvOrders.Columns["OrderDate"].Width = 130;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Siparişler yüklenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatuses()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Hazirlaniyor");
            cmbStatus.Items.Add("Kargoda");
            cmbStatus.Items.Add("Teslim Edildi");
            cmbStatus.Items.Add("Iptal");
            cmbStatus.SelectedIndex = 0;
        }

        private void BtnNewOrder_Click(object sender, EventArgs e)
        {
            using (var frm = new OrderModalForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadOrderSummary();
                    LoadOrders();
                }
            }
        }

        private void DgvOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvOrders.Rows[e.RowIndex];

            txtOrderId.Text = row.Cells["OrderId"].Value?.ToString();

            string currentStatus = row.Cells["OrderStatus"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(currentStatus))
                cmbStatus.SelectedItem = currentStatus;
        }

        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            string columnName = dgvOrders.Columns[e.ColumnIndex].Name;

            if (columnName == "OrderDate" && e.Value != null && e.Value != DBNull.Value)
            {
                DateTime dt = Convert.ToDateTime(e.Value);
                e.Value = dt.ToString("dd.MM.yyyy HH:mm");
                e.FormattingApplied = true;
            }

            if (columnName == "CustomerEmail" && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.Length > 22)
                {
                    e.Value = text.Substring(0, 22) + "...";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "CustomerName" && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.Length > 18)
                {
                    e.Value = text.Substring(0, 18) + "...";
                    e.FormattingApplied = true;
                }
            }

            if (columnName == "CustomerPhone" && e.Value != null)
            {
                string text = e.Value.ToString();
                if (text.Length > 11)
                {
                    e.Value = text.Substring(0, 11) + "...";
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvOrders_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string columnName = dgvOrders.Columns[e.ColumnIndex].Name;

            if (columnName != "OrderStatus")
                return;

            e.PaintBackground(e.CellBounds, true);
            e.Handled = true;

            string text = e.Value?.ToString() ?? "";

            Color backColor = Color.FromArgb(243, 244, 246);
            Color foreColor = Color.FromArgb(75, 85, 99);

            if (text == "Hazirlaniyor")
            {
                backColor = Color.FromArgb(255, 247, 237);
                foreColor = Color.FromArgb(194, 65, 12);
            }
            else if (text == "Kargoda")
            {
                backColor = Color.FromArgb(239, 246, 255);
                foreColor = Color.FromArgb(29, 78, 216);
            }
            else if (text == "Teslim Edildi")
            {
                backColor = Color.FromArgb(236, 253, 245);
                foreColor = Color.FromArgb(21, 128, 61);
            }
            else if (text == "Iptal")
            {
                backColor = Color.FromArgb(254, 242, 242);
                foreColor = Color.FromArgb(185, 28, 28);
            }

            Rectangle badgeRect = new Rectangle(
                e.CellBounds.X + (e.CellBounds.Width - 96) / 2,
                e.CellBounds.Y + (e.CellBounds.Height - 24) / 2,
                96,
                24
            );

            using (SolidBrush brush = new SolidBrush(backColor))
            using (SolidBrush textBrush = new SolidBrush(foreColor))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                e.Graphics.FillRectangle(brush, badgeRect);
                e.Graphics.DrawString(text, new Font("Segoe UI", 8.5F, FontStyle.Bold), textBrush, badgeRect, sf);
            }
        }

        private void BtnUpdateStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtOrderId.Text))
                {
                    MessageBox.Show("Lütfen bir sipariş seçin.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int orderId = int.Parse(txtOrderId.Text);
                string newStatus = cmbStatus.Text;

                if (string.IsNullOrWhiteSpace(newStatus))
                {
                    MessageBox.Show("Lütfen durum seçin.",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _repo.UpdateOrderStatus(orderId, newStatus);

                MessageBox.Show("Sipariş durumu güncellendi.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadOrderSummary();
                LoadOrders();
                txtOrderId.Clear();
                cmbStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sipariş durumu güncellenirken hata: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}