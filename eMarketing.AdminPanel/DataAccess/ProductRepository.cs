using System;
using System.Data;
using System.Data.SqlClient;

namespace eMarketing.AdminPanel.DataAccess
{
    public class ProductRepository
    {
        public DataTable GetAllProducts()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"SELECT 
                                    p.ProductId,
                                    p.ProductName,
                                    p.Description,
                                    p.Price,
                                    p.Stock,
                                    p.ImageUrl,
                                    p.IsActive,
                                    p.CreatedDate,
                                    p.CategoryId,
                                    c.CategoryName AS Category
                                 FROM Products p
                                 INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                                 ORDER BY p.ProductId DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    connection.Open();
                    adapter.Fill(table);
                }
            }

            return table;
        }

        public void AddProduct(string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"INSERT INTO Products
                                 (ProductName, Description, Price, Stock, ImageUrl, IsActive, CreatedDate, CategoryId)
                                 VALUES
                                 (@name, @desc, @price, @stock, @img, @active, GETDATE(), @categoryId)";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(imageUrl) ? (object)DBNull.Value : imageUrl);
                    cmd.Parameters.AddWithValue("@active", isActive);
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProduct(int id, string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"UPDATE Products SET
                                 ProductName = @name,
                                 Description = @desc,
                                 Price = @price,
                                 Stock = @stock,
                                 ImageUrl = @img,
                                 IsActive = @active,
                                 CategoryId = @categoryId
                                 WHERE ProductId = @id";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@img", string.IsNullOrWhiteSpace(imageUrl) ? (object)DBNull.Value : imageUrl);
                    cmd.Parameters.AddWithValue("@active", isActive);
                    cmd.Parameters.AddWithValue("@categoryId", categoryId);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int id)
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"DELETE FROM Products WHERE ProductId = @id";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}