using System;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class DashboardRepository
    {
        public int GetTotalProducts()
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Products", connection))
            {
                connection.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetActiveProducts()
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Products WHERE IsActive = 1", connection))
            {
                connection.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetLowStockProducts()
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Stock BETWEEN 1 AND 9", connection))
            {
                connection.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int GetTotalOrders()
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Orders", connection))
            {
                connection.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}