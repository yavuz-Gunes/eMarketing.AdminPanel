using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;
using eMarketing.Data.Models;

namespace eMarketing.Data.Repositories
{
    public class DashboardRepository
    {
        public DashboardSummary GetSummary(int? magazaId = null, bool tumMagazalar = true)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Dashboard_Ozet_Getir", connection))
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
                            return new DashboardSummary();

                        return new DashboardSummary
                        {
                            TotalProducts = GetInt(reader, "ToplamUrun"),
                            ActiveProducts = GetInt(reader, "AktifUrun"),
                            LowStockProducts = GetInt(reader, "KritikStok"),
                            TotalCategories = GetInt(reader, "ToplamKategori"),
                            ActiveCategories = GetInt(reader, "AktifKategori"),
                            TotalOrders = GetInt(reader, "ToplamSiparis")
                        };
                    }
                }
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

        public DataTable GetRecentOrders(int? magazaId = null, bool tumMagazalar = true)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Dashboard_SonSiparisler_Getir", connection))
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
            int index = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(index))
                return 0;

            return Convert.ToInt32(reader.GetValue(index));
        }
    }
}