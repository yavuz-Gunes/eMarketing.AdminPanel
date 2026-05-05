using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;

namespace eMarketing.Data.Repositories
{
    public class PersonelRepository
    {
        public DataTable GetPersoneller(string arama = "", bool sadeceAktif = false)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kullanici_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 200).Value = GetText(arama);
                    cmd.Parameters.Add("@SadeceAktif", SqlDbType.Bit).Value = sadeceAktif;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Personel listesi getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
        }

        public int PersonelKaydet(
            int? kullaniciId,
            string kullaniciAdi,
            string sifre,
            string adSoyad,
            string rol,
            bool aktifMi)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kullanici_Kaydet", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@KullaniciId", SqlDbType.Int).Value =
                        kullaniciId.HasValue ? (object)kullaniciId.Value : DBNull.Value;
                    cmd.Parameters.Add("@KullaniciAdi", SqlDbType.NVarChar, 100).Value = GetRequiredText(kullaniciAdi, "Kullanıcı adı");
                    cmd.Parameters.Add("@Sifre", SqlDbType.NVarChar, 100).Value = GetNullableText(sifre);
                    cmd.Parameters.Add("@AdSoyad", SqlDbType.NVarChar, 150).Value = GetRequiredText(adSoyad, "Ad soyad");
                    cmd.Parameters.Add("@Rol", SqlDbType.NVarChar, 50).Value = GetRequiredText(rol, "Rol");
                    cmd.Parameters.Add("@AktifMi", SqlDbType.Bit).Value = aktifMi;

                    connection.Open();
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Personel kaydedilirken veritabanı hatası oluştu: " + ex.Message);
            }
        }

        public DataTable GetPersonelMagazalari(int kullaniciId)
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_KullaniciMagaza_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = kullaniciId;

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Personel mağazaları getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
        }

        public DataTable GetAtanabilirMagazalar(int kullaniciId, string arama = "")
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_KullaniciMagaza_AtanmamisMagaza_Listele", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = kullaniciId;
                    cmd.Parameters.Add("@Arama", SqlDbType.NVarChar, 200).Value = GetText(arama);

                    connection.Open();
                    adapter.Fill(table);
                }

                return table;
            }
            catch (SqlException ex)
            {
                throw new Exception("Atanabilir mağazalar getirilirken veritabanı hatası oluştu: " + ex.Message);
            }
        }

        public void MagazaAta(int kullaniciId, int magazaId)
        {
            ExecuteMagazaCommand("sp_KullaniciMagaza_Ata", kullaniciId, magazaId, "@MagazaId");
        }

        public void MagazaKaldir(int kullaniciMagazaId)
        {
            ExecuteMagazaCommand("sp_KullaniciMagaza_Kaldir", null, kullaniciMagazaId, "@KullaniciMagazaId");
        }

        private void ExecuteMagazaCommand(string procedureName, int? kullaniciId, int id, string idParameterName)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand(procedureName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (kullaniciId.HasValue)
                        cmd.Parameters.Add("@KullaniciId", SqlDbType.Int).Value = kullaniciId.Value;

                    cmd.Parameters.Add(idParameterName, SqlDbType.Int).Value = id;

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Mağaza yetkisi güncellenirken veritabanı hatası oluştu: " + ex.Message);
            }
        }

        private object GetNullableText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            return value.Trim();
        }

        private string GetText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }

        private string GetRequiredText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception(fieldName + " boş olamaz.");

            return value.Trim();
        }
    }
}
