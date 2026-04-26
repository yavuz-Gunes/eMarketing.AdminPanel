using System;
using System.Data.SqlClient;
using eMarketing.Data.Connection;
using eMarketing.Data.Models;

namespace eMarketing.Data.Repositories
{
    public class DashboardRepository
    {
        public DashboardSummary GetSummary()
        {
            using (SqlConnection connection = DbHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT ToplamUrun, AktifUrun, KritikStok, ToplamSiparis FROM vw_DashboardOzet", connection))
            {
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new DashboardSummary
                        {
                            TotalProducts = Convert.ToInt32(reader["ToplamUrun"]),
                            ActiveProducts = Convert.ToInt32(reader["AktifUrun"]),
                            LowStockProducts = Convert.ToInt32(reader["KritikStok"]),
                            TotalOrders = Convert.ToInt32(reader["ToplamSiparis"])
                        };
                    }
                }
            }

            return new DashboardSummary
            {
                TotalProducts = 0,
                ActiveProducts = 0,
                LowStockProducts = 0,
                TotalOrders = 0
            };
        }
    }
}