using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eMarketing.AdminPanel.DataAccess
{
    public class CategoryRepository
    {
        public DataTable GetActiveCategories()
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = DbHelper.GetConnection())
            {
                string query = @"SELECT CategoryId, CategoryName
                                 FROM Categories
                                 WHERE IsActive = 1
                                 ORDER BY CategoryName";

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
