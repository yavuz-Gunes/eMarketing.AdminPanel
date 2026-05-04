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

        public OrderSummary GetOrderSummary(int? magazaId = null, bool tumMagazalar = true)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_DurumOzet_Getir", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId.HasValue ? (object)magazaId.Value : DBNull.Value;

                    cmd.Parameters.Add("@TumMagazalar", SqlDbType.Bit)
                        .Value = tumMagazalar;

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return new OrderSummary();

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
            catch (SqlException ex)
            {
                throw new Exception("Sipariþ özeti getirilirken veritabaný hatasý oluþtu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariþ özeti getirilirken hata oluþtu: " + ex.Message);
            }
        }

        public DataTable GetAllOrders(int? magazaId = null, bool tumMagazalar = true)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId.HasValue ? (object)magazaId.Value : DBNull.Value;

                    cmd.Parameters.Add("@TumMagazalar", SqlDbType.Bit)
                        .Value = tumMagazalar;

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

        public int AddOrder(
            string customerName,
            string customerEmail,
            string customerPhone,
            int productId,
            int quantity,
            decimal totalPrice)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_Ekle_TekUrun", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CustomerName", SqlDbType.NVarChar, 300)
                        .Value = string.IsNullOrWhiteSpace(customerName)
                            ? throw new Exception("Müþteri adý boþ olamaz.")
                            : customerName.Trim();

                    cmd.Parameters.Add("@CustomerEmail", SqlDbType.NVarChar, 400)
                        .Value = GetNullableValue(customerEmail);

                    cmd.Parameters.Add("@CustomerPhone", SqlDbType.NVarChar, 100)
                        .Value = GetNullableValue(customerPhone);

                    cmd.Parameters.Add("@ProductId", SqlDbType.Int)
                        .Value = productId;

                    cmd.Parameters.Add("@Quantity", SqlDbType.Int)
                        .Value = quantity;

                    cmd.Parameters.Add("@TotalPrice", SqlDbType.Decimal)
                        .Value = totalPrice;

                    cmd.Parameters["@TotalPrice"].Precision = 18;
                    cmd.Parameters["@TotalPrice"].Scale = 2;

                    connection.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (SqlException ex)
            {
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
                        .Value = status.Trim();

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Sipariþ durumu güncellenirken veritabaný hatasý oluþtu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariþ durumu güncellenirken hata oluþtu: " + ex.Message);
            }
        }

        public void CancelOrder(int orderId)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Siparis_IptalEt", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SiparisId", SqlDbType.Int)
                        .Value = orderId;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Sipariþ iptal edilirken veritabaný hatasý oluþtu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariþ iptal edilirken hata oluþtu: " + ex.Message);
            }
        }

        private int GetInt(SqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(index))
                return 0;

            return Convert.ToInt32(reader.GetValue(index));
        }

        private object GetNullableValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            return value.Trim();
        }
    }
}