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

        public int AddOrder(string name, string email, string phone, int productId, int quantity, decimal totalPrice)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_Ekle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MusteriAdi", SqlDbType.NVarChar, 150)
                        .Value = name.Trim();

                    cmd.Parameters.Add("@MusteriEmail", SqlDbType.NVarChar, 150)
                        .Value = string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email.Trim();

                    cmd.Parameters.Add("@MusteriTelefon", SqlDbType.NVarChar, 50)
                        .Value = string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone.Trim();

                    cmd.Parameters.Add("@UrunId", SqlDbType.Int)
                        .Value = productId;

                    cmd.Parameters.Add("@Adet", SqlDbType.Int)
                        .Value = quantity;

                    cmd.Parameters.Add("@ToplamTutar", SqlDbType.Decimal)
                        .Value = totalPrice;
                    cmd.Parameters["@ToplamTutar"].Precision = 18;
                    cmd.Parameters["@ToplamTutar"].Scale = 2;

                    connection.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
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

                    cmd.Parameters.Add("@SiparisId", SqlDbType.Int)
                        .Value = orderId;

                    cmd.Parameters.Add("@SiparisDurumu", SqlDbType.NVarChar, 50)
                        .Value = status;

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
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_DurumOzet_Getir", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new OrderSummary
                            {
                                TotalOrders = GetInt(reader, "ToplamSiparis"),
                                PreparingOrders = GetInt(reader, "HazirlaniyorSayisi"),
                                ShippedOrders = GetInt(reader, "KargodaSayisi"),
                                DeliveredOrders = GetInt(reader, "TeslimEdildiSayisi"),
                                CancelledOrders = GetInt(reader, "IptalSayisi")
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

        private int GetInt(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(reader[columnName]);
        }
    }
}