using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eMarketing.AdminPanel.DataAccess
{
    public class DashboardRepository
    {
        public int GetTotalProducts()
        {
            using (SqlConnection c = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Products", c))
            {
                c.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetActiveProducts()
        {
            using (SqlConnection c = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Products WHERE IsActive = 1", c))
            {
                c.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetLowStockProducts()
        {
            using (SqlConnection c = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Stock BETWEEN 1 AND 9", c))
            {
                c.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetTotalOrders()
        {
            using (SqlConnection c = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Orders", c))
            {
                c.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
