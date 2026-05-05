using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class MagazaRepository
    {
        public DataTable GetMagazaSecimListesi(string arama = "", bool sadeceAktif = true)
        {
            return GetMagazaSecimListesi(arama, sadeceAktif, null, false);
        }

        public DataTable GetMagazaSecimListesi(
            string arama,
            bool sadeceAktif,
            int? kullaniciId,
            bool adminMi)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Magaza_Secim_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 200)
                        .Value = string.IsNullOrWhiteSpace(arama) ? "" : arama.Trim();

                    cmd.Parameters.Add("@SadeceAktif", SqlDbType.Bit)
                        .Value = sadeceAktif;

                    cmd.Parameters.Add("@KullaniciId", SqlDbType.Int)
                        .Value = kullaniciId.HasValue ? (object)kullaniciId.Value : DBNull.Value;

                    cmd.Parameters.Add("@AdminMi", SqlDbType.Bit)
                        .Value = adminMi;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Mağaza seçim listesi getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Mağaza seçim listesi getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataTable GetAktifMagazalar()
        {
            return GetMagazaSecimListesi("", true);
        }

        public DataTable GetAktifMagazalar(int kullaniciId, bool adminMi)
        {
            return GetMagazaSecimListesi("", true, kullaniciId, adminMi);
        }

        public DataRow GetMagazaById(int magazaId)
        {
            return GetMagazaById(magazaId, null, false);
        }

        public DataRow GetMagazaById(int magazaId, int? kullaniciId, bool adminMi)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Magaza_Secim_Getir", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId;

                    cmd.Parameters.Add("@KullaniciId", SqlDbType.Int)
                        .Value = kullaniciId.HasValue ? (object)kullaniciId.Value : DBNull.Value;

                    cmd.Parameters.Add("@AdminMi", SqlDbType.Bit)
                        .Value = adminMi;

                    connection.Open();
                    adapter.Fill(table);
                }

                if (table.Rows.Count == 0)
                    return null;

                return table.Rows[0];
            }
            catch (SqlException ex)
            {
                throw new Exception("Mağaza bilgisi getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Mağaza bilgisi getirilirken hata oluştu: " + ex.Message);
            }
        }
    }
}
