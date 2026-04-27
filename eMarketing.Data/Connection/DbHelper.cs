using System;
using System.Configuration;
using System.Data.SqlClient;

namespace eMarketing.Data.Connection
{
    public static class DbHelper
    {
        public static SqlConnection GetConnection()
        {
            try
            {
                string connectionString = GetConnectionString();
                return new SqlConnection(connectionString);
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new InvalidOperationException(
                    "Bağlantı ayarları okunurken yapılandırma hatası oluştu.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Veritabanı bağlantısı oluşturulurken beklenmeyen bir hata oluştu.", ex);
            }
        }

        private static string GetConnectionString()
        {
            var settings = ConfigurationManager.ConnectionStrings["DbConnection"];

            if (settings == null)
                throw new InvalidOperationException(
                    "App.config içinde 'DbConnection' isimli bağlantı bulunamadı.");

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new InvalidOperationException(
                    "'DbConnection' connection string değeri boş olamaz.");

            return settings.ConnectionString;
        }
    }
}