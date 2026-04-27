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
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Product_List", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? "" : search.Trim());
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                connection.Open();
                adapter.Fill(table);
            }

            return table;
        }

        public DataRow GetProductById(int productId)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Product_GetById", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductId", productId);

                connection.Open();
                adapter.Fill(table);
            }

            if (table.Rows.Count == 0)
                return null;

            return table.Rows[0];
        }

        public int InsertProduct(string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Product_Insert", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductName", name.Trim());
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Stock", stock);
                cmd.Parameters.AddWithValue("@ImageUrl", string.IsNullOrWhiteSpace(imageUrl) ? (object)DBNull.Value : imageUrl.Trim());
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                connection.Open();

                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public void UpdateProduct(int id, string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_Product_Update", connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProductId", id);
                cmd.Parameters.AddWithValue("@ProductName", name.Trim());
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Stock", stock);
                cmd.Parameters.AddWithValue("@ImageUrl", string.IsNullOrWhiteSpace(imageUrl) ? (object)DBNull.Value : imageUrl.Trim());
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SetProductActiveStatus(int productId, bool isActive)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("UPDATE Products SET IsActive = @isActive WHERE ProductId = @productId", connection))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@isActive", isActive);

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetActiveProducts()
        {
            return GetProducts("", 1, 0);
        }

        public DataTable GetProductsForOrder()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"SELECT 
                                    ProductId,
                                    ProductName + ' (Stok: ' + CAST(Stock AS NVARCHAR(20)) + ')' AS ProductDisplay,
                                    Price,
                                    Stock
                                 FROM Products
                                 WHERE IsActive = 1
                                 ORDER BY ProductName";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    connection.Open();
                    adapter.Fill(table);
                }
            }

            return table;
        }
    }
}