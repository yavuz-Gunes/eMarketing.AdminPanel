using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class MagazaStokRepository
    {
        public DataTable GetMagazaStoklari(
            int? magazaId = null,
            string arama = "",
            bool sadeceStokta = false,
            bool sadeceKritik = false,
            bool sadeceAktif = true,
            int? kullaniciId = null,
            bool adminMi = false)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MagazaStok_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId.HasValue ? (object)magazaId.Value : DBNull.Value;

                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 200)
                        .Value = string.IsNullOrWhiteSpace(arama) ? "" : arama.Trim();

                    cmd.Parameters.Add("@SadeceStokta", SqlDbType.Bit)
                        .Value = sadeceStokta;

                    cmd.Parameters.Add("@SadeceKritik", SqlDbType.Bit)
                        .Value = sadeceKritik;

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
                throw new Exception("Mağaza stokları getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Mağaza stokları getirilirken hata oluştu: " + ex.Message);
            }
        }

        public DataRow GetMagazaStokOzeti(
            int? magazaId = null,
            bool tumMagazalar = true,
            int? kullaniciId = null,
            bool adminMi = false)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MagazaStok_Ozet_Getir", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId.HasValue ? (object)magazaId.Value : DBNull.Value;

                    cmd.Parameters.Add("@TumMagazalar", SqlDbType.Bit)
                        .Value = tumMagazalar;

                    cmd.Parameters.Add("@KullaniciId", SqlDbType.Int)
                        .Value = kullaniciId.HasValue ? (object)kullaniciId.Value : DBNull.Value;

                    cmd.Parameters.Add("@AdminMi", SqlDbType.Bit)
                        .Value = adminMi;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table.Rows.Count == 0 ? null : table.Rows[0];
            }
            catch (SqlException ex)
            {
                throw new Exception("Mağaza stok özeti getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Mağaza stok özeti getirilirken hata oluştu: " + ex.Message);
            }
        }

        public void MinimumStokGuncelle(int magazaStokId, int minimumStok)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MagazaStok_MinimumGuncelle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MagazaStokId", SqlDbType.Int)
                        .Value = magazaStokId;

                    cmd.Parameters.Add("@MinimumStok", SqlDbType.Int)
                        .Value = minimumStok;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Minimum stok güncellenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Minimum stok güncellenirken hata oluştu: " + ex.Message);
            }
        }

        public void StokHareketiIsle(
            int magazaId,
            int urunId,
            string hareketTipi,
            int miktar,
            string aciklama = "",
            int? minimumStok = null)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_MagazaStok_Hareket_Isle", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MagazaId", SqlDbType.Int)
                        .Value = magazaId;

                    cmd.Parameters.Add("@ProductId", SqlDbType.Int)
                        .Value = urunId;

                    cmd.Parameters.Add("@HareketTipi", SqlDbType.NVarChar, 50)
                        .Value = string.IsNullOrWhiteSpace(hareketTipi) ? "ManuelGiris" : hareketTipi.Trim();

                    cmd.Parameters.Add("@Miktar", SqlDbType.Int)
                        .Value = miktar;

                    cmd.Parameters.Add("@KaynakSiparisId", SqlDbType.Int)
                        .Value = DBNull.Value;

                    cmd.Parameters.Add("@KaynakSiparisKalemId", SqlDbType.Int)
                        .Value = DBNull.Value;

                    cmd.Parameters.Add("@Aciklama", SqlDbType.NVarChar, 500)
                        .Value = string.IsNullOrWhiteSpace(aciklama) ? (object)DBNull.Value : aciklama.Trim();

                    cmd.Parameters.Add("@MinimumStok", SqlDbType.Int)
                        .Value = minimumStok.HasValue ? (object)minimumStok.Value : DBNull.Value;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Mağaza stok hareketi işlenirken veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Mağaza stok hareketi işlenirken hata oluştu: " + ex.Message);
            }
        }
    }
}
