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

                                TotalCategories = HasColumn(reader, "ToplamKategori")
                                    ? GetInt(reader, "ToplamKategori")
                                    : 0,

                                ActiveCategories = HasColumn(reader, "AktifKategori")
                                    ? GetInt(reader, "AktifKategori")
                                    : 0,

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

        public DataTable GetRecentOrders()
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Dashboard_SonSiparisler_Getir", connection))
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
                throw new Exception("Son siparişler getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Son siparişler getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataTable GetCriticalStockProducts()
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Dashboard_KritikStok_Getir", connection))
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
                throw new Exception("Kritik stok ürünleri getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Kritik stok ürünleri getirilirken hata oluştu: " + ex.Message);
            }
        }

        private int GetInt(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == DBNull.Value)
                return 0;

            return Convert.ToInt32(reader[columnName]);
        }

        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private DashboardSummary CreateEmptySummary()
        {
            return new DashboardSummary
            {
                TotalProducts = 0,
                ActiveProducts = 0,
                LowStockProducts = 0,
                TotalCategories = 0,
                ActiveCategories = 0,
                TotalOrders = 0
            };
        }
    }
}