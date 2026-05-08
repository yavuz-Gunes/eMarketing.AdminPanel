using System;
using System.IO;

namespace eMarketing.AdminPanel.Services
{
    public static class ApiFallbackReporter
    {
        public static void Report(string operation, Exception exception)
        {
            try
            {
                string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                string logPath = Path.Combine(logFolder, "api-fallback.log");
                string line = string.Format(
                    "{0:yyyy-MM-dd HH:mm:ss} | {1} | API kullanılamadı, repository fallback çalıştı. Hata: {2}",
                    DateTime.Now,
                    operation,
                    exception.Message);

                File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
