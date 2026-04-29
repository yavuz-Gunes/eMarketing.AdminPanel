using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;
using eMarketing.Data.Models;

namespace eMarketing.Data.Repositories
{
    public class OrderRepository
    {
        public DataTable GetAllOrders()
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Sipariþler getirilirken veritabaný hatasý oluþtu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariþler getirilirken hata oluþtu: " + ex.Message);
            }
        }

        public void AddOrder(string name, string email, string phone, int productId, int quantity, decimal totalPrice)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_Ekle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerName", name);
                    cmd.Parameters.AddWithValue("@CustomerEmail",
                        string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    cmd.Parameters.AddWithValue("@CustomerPhone",
                        string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@TotalPrice", totalPrice);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000)
                    throw new Exception(ex.Message);

                throw new Exception("Sipariþ eklenirken veritabaný hatasý oluþtu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariþ eklenirken hata oluþtu: " + ex.Message);
            }
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_Durum_Guncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.Parameters.AddWithValue("@OrderStatus", status);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000)
                    throw new Exception(ex.Message);

                throw new Exception("Sipariþ durumu güncellenirken veritabaný hatasý oluþtu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariþ durumu güncellenirken hata oluþtu: " + ex.Message);
            }
        }

        public OrderSummary GetOrderSummary()
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ToplamSiparis, HazirlaniyorSayisi, KargodaSayisi, TeslimEdildiSayisi, IptalSayisi FROM vw_SiparisDurumOzet",
                    connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new OrderSummary
                            {
                                TotalOrders = Convert.ToInt32(reader["ToplamSiparis"]),
                                PreparingOrders = Convert.ToInt32(reader["HazirlaniyorSayisi"]),
                                ShippedOrders = Convert.ToInt32(reader["KargodaSayisi"]),
                                DeliveredOrders = Convert.ToInt32(reader["TeslimEdildiSayisi"]),
                                CancelledOrders = Convert.ToInt32(reader["IptalSayisi"])
                            };
                        }
                    }
                }

                return new OrderSummary();
            }
            catch (SqlException ex)
            {
                throw new Exception("Sipariþ özeti getirilirken veritabaný hatasý oluþtu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariþ özeti getirilirken hata oluþtu: " + ex.Message);
            }
        }
    }
}