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
                var settings = ConfigurationManager.ConnectionStrings["DbConnection"];

                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                    throw new InvalidOperationException(
                        "App.config içinde 'DbConnection' connectionString bulunamadı veya boş.");

                return new SqlConnection(settings.ConnectionString);
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
    }
}