using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace eMarketing.AdminPanel.DataAccess
{
    public static class DbHelper
    {
        public static SqlConnection GetConnection()
        {
            var settings = ConfigurationManager.ConnectionStrings["DbConnection"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new InvalidOperationException("App.config içinde 'DbConnection' connectionString bulunamadý veya boþ. Lütfen App.config'u kontrol edin.");

            return new SqlConnection(settings.ConnectionString);
        }
    }
}
