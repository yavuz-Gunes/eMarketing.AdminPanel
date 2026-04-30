 using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class CategoryRepository
    {
        public DataTable GetCategories(string search = "", int status = 1)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kategori_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 150)
                        .Value = string.IsNullOrWhiteSpace(search) ? "" : search.Trim();

                    cmd.Parameters.Add("@Durum", SqlDbType.Int)
                        .Value = status;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Kategoriler getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Kategoriler getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataRow GetCategoryById(int categoryId)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kategori_Getir", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@KategoriId", SqlDbType.Int)
                        .Value = categoryId;

                    connection.Open();
                    adapter.Fill(table);
                }

                if (table.Rows.Count == 0)
                    return null;

                return table.Rows[0];
            }
            catch (SqlException ex)
            {
                throw new Exception("Kategori bilgisi getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Kategori bilgisi getirilirken hata oluştu: " + ex.Message);
            }
        }

        public int InsertCategory(string categoryName)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kategori_Ekle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@KategoriAdi", SqlDbType.NVarChar, 150)
                        .Value = categoryName.Trim();

                    connection.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Kategori eklenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Kategori eklenirken hata oluştu: " + ex.Message);
            }
        }

        public void UpdateCategory(int categoryId, string categoryName, bool isActive)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kategori_Guncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@KategoriId", SqlDbType.Int)
                        .Value = categoryId;

                    cmd.Parameters.Add("@KategoriAdi", SqlDbType.NVarChar, 150)
                        .Value = categoryName.Trim();

                    cmd.Parameters.Add("@AktifMi", SqlDbType.Bit)
                        .Value = isActive;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Kategori güncellenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Kategori güncellenirken hata oluştu: " + ex.Message);
            }
        }

        public void SetCategoryActiveStatus(int categoryId, bool isActive)
        {
            try
            {
                DataRow row = GetCategoryById(categoryId);

                if (row == null)
                    throw new Exception("Kategori bulunamadı.");

                string categoryName = row["KategoriAdi"]?.ToString() ?? "";

                UpdateCategory(categoryId, categoryName, isActive);
            }
            catch (Exception ex)
            {
                throw new Exception("Kategori durumu değiştirilirken hata oluştu: " + ex.Message);
            }
        }

        public bool DeleteCategory(int categoryId, out string message)
        {
            message = string.Empty;

            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kategori_Sil", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@KategoriId", SqlDbType.Int)
                        .Value = categoryId;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    message = "Bu kategoriye bağlı ürünler olduğu için silinemez.";
                    return false;
                }

                if (ex.Number == 50000)
                {
                    message = ex.Message;
                    return false;
                }

                message = "Kategori silinirken veritabanı hatası oluştu: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                message = "Kategori silinirken hata oluştu: " + ex.Message;
                return false;
            }
        }

        public DataTable GetActiveCategories()
        {
            try
            {
                return GetCategories("", 1);
            }
            catch (Exception ex)
            {
                throw new Exception("Aktif kategoriler getirilirken hata oluştu: " + ex.Message);
            }
        }
    }
}