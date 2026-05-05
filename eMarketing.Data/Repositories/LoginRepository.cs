using System;
using System.Data;
using System.Data.SqlClient;
using eMarketing.Data.Connection;
using eMarketing.Data.Models;

namespace eMarketing.Data.Repositories
{
    public class LoginRepository
    {
        public KullaniciGirisModel GirisYap(string kullaniciAdi, string sifre)
        {
            try
            {
                using (SqlConnection connection = DbHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand("sp_Kullanici_GirisYap", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@KullaniciAdi", SqlDbType.NVarChar, 100)
                        .Value = (kullaniciAdi ?? string.Empty).Trim();

                    cmd.Parameters.Add("@Sifre", SqlDbType.NVarChar, 200)
                        .Value = (sifre ?? string.Empty).Trim();

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new KullaniciGirisModel
                        {
                            KullaniciId = Convert.ToInt32(reader["KullaniciId"]),
                            KullaniciAdi = Convert.ToString(reader["KullaniciAdi"]),
                            AdSoyad = Convert.ToString(reader["AdSoyad"]),
                            Rol = Convert.ToString(reader["Rol"])
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Kullanıcı girişi sırasında veritabanı hatası oluştu: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Kullanıcı girişi sırasında hata oluştu: " + ex.Message);
            }
        }
    }
}
