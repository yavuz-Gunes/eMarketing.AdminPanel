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
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Category_List", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? "" : search.Trim());
                cmd.Parameters.AddWithValue("@Status", status);

                connection.Open();
                adapter.Fill(table);
            }

            return table;
        }

        public DataRow GetCategoryById(int categoryId)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Category_GetById", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                connection.Open();
                adapter.Fill(table);
            }

            if (table.Rows.Count == 0)
                return null;

            return table.Rows[0];
        }

        public int InsertCategory(string categoryName)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Category_Insert", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryName", categoryName.Trim());

                connection.Open();

                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public void UpdateCategory(int categoryId, string categoryName, bool isActive)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Category_Update", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);
                cmd.Parameters.AddWithValue("@CategoryName", categoryName.Trim());
                cmd.Parameters.AddWithValue("@IsActive", isActive);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SetCategoryActiveStatus(int categoryId, bool isActive)
        {
            DataRow row = GetCategoryById(categoryId);

            if (row == null)
                throw new Exception("Kategori bulunamadı.");

            string categoryName = row["CategoryName"]?.ToString() ?? "";
            UpdateCategory(categoryId, categoryName, isActive);
        }

        public bool DeleteCategory(int categoryId, out string message)
        {
            message = string.Empty;

            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Category_Delete", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId);

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
            return GetCategories("", 1);
        }
    }
}