using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class ProductRepository
    {
        public DataTable GetProducts(string search = "", int status = 1, int categoryId = 0)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Urun_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 200)
                        .Value = string.IsNullOrWhiteSpace(search) ? "" : search.Trim();

                    cmd.Parameters.Add("@Durum", SqlDbType.Int)
                        .Value = status;

                    cmd.Parameters.Add("@KategoriId", SqlDbType.Int)
                        .Value = categoryId;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Ürünler getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Ürünler getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataRow GetProductById(int productId)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Urun_Getir", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@UrunId", SqlDbType.Int)
                        .Value = productId;

                    connection.Open();
                    adapter.Fill(table);
                }

                if (table.Rows.Count == 0)
                    return null;

                return table.Rows[0];
            }
            catch (SqlException ex)
            {
                throw new Exception("Ürün bilgisi getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Ürün bilgisi getirilirken hata oluştu: " + ex.Message);
            }
        }

        public int InsertProduct(string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Urun_Ekle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@KategoriId", SqlDbType.Int)
                        .Value = categoryId;

                    cmd.Parameters.Add("@UrunAdi", SqlDbType.NVarChar, 200)
                        .Value = name.Trim();

                    cmd.Parameters.Add("@Aciklama", SqlDbType.NVarChar)
                        .Value = string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();

                    cmd.Parameters.Add("@Fiyat", SqlDbType.Decimal)
                        .Value = price;
                    cmd.Parameters["@Fiyat"].Precision = 18;
                    cmd.Parameters["@Fiyat"].Scale = 2;

                    cmd.Parameters.Add("@Stok", SqlDbType.Int)
                        .Value = stock;

                    cmd.Parameters.Add("@GorselUrl", SqlDbType.NVarChar, 500)
                        .Value = string.IsNullOrWhiteSpace(imageUrl) ? (object)DBNull.Value : imageUrl.Trim();

                    cmd.Parameters.Add("@AktifMi", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Ürün eklenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Ürün eklenirken hata oluştu: " + ex.Message);
            }
        }

        public void UpdateProduct(int id, string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Urun_Guncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@UrunId", SqlDbType.Int)
                        .Value = id;

                    cmd.Parameters.Add("@KategoriId", SqlDbType.Int)
                        .Value = categoryId;

                    cmd.Parameters.Add("@UrunAdi", SqlDbType.NVarChar, 200)
                        .Value = name.Trim();

                    cmd.Parameters.Add("@Aciklama", SqlDbType.NVarChar)
                        .Value = string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim();

                    cmd.Parameters.Add("@Fiyat", SqlDbType.Decimal)
                        .Value = price;
                    cmd.Parameters["@Fiyat"].Precision = 18;
                    cmd.Parameters["@Fiyat"].Scale = 2;

                    cmd.Parameters.Add("@Stok", SqlDbType.Int)
                        .Value = stock;

                    cmd.Parameters.Add("@GorselUrl", SqlDbType.NVarChar, 500)
                        .Value = string.IsNullOrWhiteSpace(imageUrl) ? (object)DBNull.Value : imageUrl.Trim();

                    cmd.Parameters.Add("@AktifMi", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Ürün güncellenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Ürün güncellenirken hata oluştu: " + ex.Message);
            }
        }

        public void SetProductActiveStatus(int productId, bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Urun_DurumGuncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@UrunId", SqlDbType.Int)
                        .Value = productId;

                    cmd.Parameters.Add("@AktifMi", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Ürün durumu değiştirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Ürün durumu değiştirilirken hata oluştu: " + ex.Message);
            }
        }

        public bool DeleteProduct(int productId, out string message)
        {
            message = string.Empty;

            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Urun_Sil", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@UrunId", SqlDbType.Int)
                        .Value = productId;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    message = "Bu ürün başka kayıtlarda kullanıldığı için silinemez.";
                    return false;
                }

                if (ex.Number == 50000)
                {
                    message = ex.Message;
                    return false;
                }

                message = "Ürün silinirken veritabanı hatası oluştu: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                message = "Ürün silinirken hata oluştu: " + ex.Message;
                return false;
            }
        }

        public DataTable GetActiveProducts()
        {
            try
            {
                return GetProducts("", 1, 0);
            }
            catch (Exception ex)
            {
                throw new Exception("Aktif ürünler getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataTable GetProductsForOrder()
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_SiparisIcin_Urun_Listele", connection))
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
                throw new Exception("Sipariş için ürünler getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Sipariş için ürünler getirilirken hata oluştu: " + ex.Message);
            }
        }
    }
}