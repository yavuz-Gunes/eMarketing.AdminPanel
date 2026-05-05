using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using eMarketing.AdminPanel.Forms;

namespace eMarketing.AdminPanel
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                    return;
            }

            using (MagazaSecimForm magazaSecimForm = new MagazaSecimForm())
            {
                if (magazaSecimForm.ShowDialog() != DialogResult.OK || !magazaSecimForm.SecimYapildi)
                    return;
            }

            Application.Run(new FrmMain());
        }
    }
}
