using System.Data;
using System.Data.SqlClient;

namespace eMarketing.AdminPanel.DataAccess
{
    public class OrderRepository
    {
        public DataTable GetAllOrders()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"SELECT 
                                    o.OrderId,
                                    o.CustomerName,
                                    o.CustomerEmail,
                                    o.CustomerPhone,
                                    p.ProductName,
                                    o.Quantity,
                                    o.TotalPrice,
                                    o.OrderStatus,
                                    o.OrderDate
                                 FROM Orders o
                                 INNER JOIN Products p ON o.ProductId = p.ProductId
                                 ORDER BY o.OrderId DESC";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        connection.Open();
                        adapter.Fill(table);
                    }
                }
            }

            return table;
        }

        public void AddOrder(string name, string email, string phone, int productId, int quantity, decimal totalPrice)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"INSERT INTO Orders
                                 (CustomerName, CustomerEmail, CustomerPhone, ProductId, Quantity, TotalPrice, OrderStatus, OrderDate)
                                 VALUES
                                 (@name, @email, @phone, @pid, @qty, @total, 'Hazýrlanýyor', GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@phone", phone);
                    cmd.Parameters.AddWithValue("@pid", productId);
                    cmd.Parameters.AddWithValue("@qty", quantity);
                    cmd.Parameters.AddWithValue("@total", totalPrice);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = "UPDATE Orders SET OrderStatus=@status WHERE OrderId=@id";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", orderId);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
