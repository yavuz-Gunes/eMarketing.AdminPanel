using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Core;
using eMarketing.AdminPanel.Forms;

namespace eMarketing.AdminPanel.Pages
{
    public class OrdersPage : UserControl
    {
        public OrdersPage()
        {
            Dock = DockStyle.Fill;
            Load += OrdersPage_Load;
        }

        private void OrdersPage_Load(object sender, System.EventArgs e)
        {
            Controls.Clear();

            var frm = new FrmOrders
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill
            };

            Controls.Add(frm);
            frm.Show();
        }
    }
}
