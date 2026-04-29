using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;
using eMarketing.Data.Models;

namespace eMarketing.Data.Repositories
{
    public class DashboardRepository
    {
        public DashboardSummary GetSummary()
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Dashboard_Ozet_Getir", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DashboardSummary
                            {
                                TotalProducts = GetInt(reader, "ToplamUrun"),
                                ActiveProducts = GetInt(reader, "AktifUrun"),
                                LowStockProducts = GetInt(reader, "KritikStok"),
                                TotalOrders = GetInt(reader, "ToplamSiparis")
                            };
                        }
                    }
                }

                return CreateEmptySummary();
            }
            catch (SqlException ex)
            {
                throw new Exception("Dashboard özeti getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Dashboard özeti getirilirken hata oluştu: " + ex.Message);
            }
        }

        private int GetInt(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(reader[columnName]);
        }

        private DashboardSummary CreateEmptySummary()
        {
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