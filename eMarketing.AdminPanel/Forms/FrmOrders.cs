using System;
using System.Data;
using System.Windows.Forms;
using eMarketing.AdminPanel.DataAccess;

namespace eMarketing.AdminPanel.Forms
{
    public partial class FrmOrders : Form
    {
        private readonly OrderRepository _orderRepository = new OrderRepository();

        public FrmOrders()
        {
            InitializeComponent();
        }

        private void FrmOrders_Load(object sender, EventArgs e)
        {
            LoadOrders();
            LoadStatuses();
        }

        private void LoadOrders()
        {
            try
            {
                dgvOrders.DataSource = _orderRepository.GetAllOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Siparişler yüklenirken hata: " + ex.Message);
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

        private void dgvOrders_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvOrders.Rows[e.RowIndex];

            txtOrderId.Text = row.Cells["OrderId"].Value?.ToString();

            string currentStatus = row.Cells["OrderStatus"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(currentStatus))
            {
                cmbStatus.SelectedItem = currentStatus;
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtOrderId.Text))
                {
                    MessageBox.Show("Lütfen bir sipariş seçin.");
                    return;
                }

                int orderId = int.Parse(txtOrderId.Text);
                string newStatus = cmbStatus.Text;

                if (string.IsNullOrWhiteSpace(newStatus))
                {
                    MessageBox.Show("Lütfen durum seçin.");
                    return;
                }

                _orderRepository.UpdateOrderStatus(orderId, newStatus);

                MessageBox.Show("Sipariş durumu güncellendi.");

                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        
    }
}