using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class BayiYetkiliRepository
    {
        public DataTable GetYetkililer(string arama = "", int durum = -1, int? bayiId = null, int? magazaId = null)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_BayiYetkili_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 200)
                        .Value = string.IsNullOrWhiteSpace(arama) ? "" : arama.Trim();

                    cmd.Parameters.Add("@Durum", SqlDbType.Int)
                        .Value = durum;

                    cmd.Parameters.Add("@BayiId", SqlDbType.Int)
                        .Value = bayiId.HasValue ? (object)bayiId.Value : DBNull.Value;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId.HasValue ? (object)magazaId.Value : DBNull.Value;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Bayi yetkilileri getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Bayi yetkilileri getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataRow GetYetkiliById(int bayiYetkiliId)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_BayiYetkili_Getir", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BayiYetkiliId", SqlDbType.Int)
                        .Value = bayiYetkiliId;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table.Rows.Count == 0 ? null : table.Rows[0];
            }
            catch (SqlException ex)
            {
                throw new Exception("Bayi yetkilisi getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Bayi yetkilisi getirilirken hata oluştu: " + ex.Message);
            }
        }

        public int Kaydet(
            int? bayiYetkiliId,
            int bayiId,
            int? magazaId,
            string adSoyad,
            string telefon,
            string email,
            string gorev,
            string notlar,
            bool aktifMi)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_BayiYetkili_Kaydet", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BayiYetkiliId", SqlDbType.Int)
                        .Value = bayiYetkiliId.HasValue ? (object)bayiYetkiliId.Value : DBNull.Value;

                    cmd.Parameters.Add("@BayiId", SqlDbType.Int)
                        .Value = bayiId;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId.HasValue ? (object)magazaId.Value : DBNull.Value;

                    cmd.Parameters.Add("@AdSoyad", SqlDbType.NVarChar, 200)
                        .Value = adSoyad.Trim();

                    cmd.Parameters.Add("@Telefon", SqlDbType.NVarChar, 60)
                        .Value = GetNullableValue(telefon);

                    cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 400)
                        .Value = GetNullableValue(email);

                    cmd.Parameters.Add("@Gorev", SqlDbType.NVarChar, 100)
                        .Value = GetNullableValue(gorev);

                    cmd.Parameters.Add("@Notlar", SqlDbType.NVarChar, 500)
                        .Value = GetNullableValue(notlar);

                    cmd.Parameters.Add("@AktifMi", SqlDbType.Bit)
                        .Value = aktifMi;

                    connection.Open();
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Bayi yetkilisi kaydedilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Bayi yetkilisi kaydedilirken hata oluştu: " + ex.Message);
            }
        }

        public void DurumGuncelle(int bayiYetkiliId, bool aktifMi)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_BayiYetkili_DurumGuncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BayiYetkiliId", SqlDbType.Int)
                        .Value = bayiYetkiliId;

                    cmd.Parameters.Add("@AktifMi", SqlDbType.Bit)
                        .Value = aktifMi;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Bayi yetkilisi durumu güncellenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Bayi yetkilisi durumu güncellenirken hata oluştu: " + ex.Message);
            }
        }

        private object GetNullableValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }
    }
}
