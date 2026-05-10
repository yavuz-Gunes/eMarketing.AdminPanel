using System;
using System.IO;

namespace eMarketing.AdminPanel.Services
{
    public static class ApiErrorLogger
    {
        private static readonly object SyncRoot = new object();

        public static void Log(string operation, Exception exception)
        {
            try
            {
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(logDirectory);

                string logPath = Path.Combine(logDirectory, "api-error.log");
                string text =
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    " | " + (operation ?? "API") +
                    Environment.NewLine +
                    exception +
                    Environment.NewLine +
                    new string('-', 80) +
                    Environment.NewLine;

                lock (SyncRoot)
                {
                    File.AppendAllText(logPath, text);
                }
            }
            catch
            {
                // Loglama uygulamanın akışını bozmamalı.
            }
        }
    }
}
