using System;

namespace eMarketing.AdminPanel.Core
{
    public static class AppSession
    {
        public static int KullaniciId { get; private set; }
        public static string KullaniciAdi { get; private set; }
        public static string AdSoyad { get; private set; }
        public static string Rol { get; private set; }
        public static string ApiToken { get; private set; }

        public static int? SeciliMusteriId { get; private set; }
        public static int? SeciliMagazaId { get; private set; }

        public static string SeciliMusteriAdi { get; private set; }
        public static string SeciliMagazaAdi { get; private set; }
        public static string SeciliSehir { get; private set; }

        public static bool AdminMi { get; private set; }
        public static bool TumMagazalar { get; private set; }

        public static bool GirisYapildi
        {
            get { return KullaniciId > 0; }
        }

        public static bool MagazaSecildi
        {
            get { return TumMagazalar || SeciliMagazaId.HasValue; }
        }

        public static string MagazaGorunumAdi
        {
            get
            {
                if (TumMagazalar)
                    return "Tüm Mağazalar";

                if (!string.IsNullOrWhiteSpace(SeciliMusteriAdi) &&
                    !string.IsNullOrWhiteSpace(SeciliMagazaAdi))
                {
                    return SeciliMusteriAdi + " - " + SeciliMagazaAdi;
                }

                if (!string.IsNullOrWhiteSpace(SeciliMagazaAdi))
                    return SeciliMagazaAdi;

                return "Mağaza Seçilmedi";
            }
        }

        public static void GirisBilgisiAyarla(
            int kullaniciId,
            string kullaniciAdi,
            string adSoyad,
            string rol,
            string apiToken = "")
        {
            KullaniciId = kullaniciId;
            KullaniciAdi = kullaniciAdi ?? "";
            AdSoyad = adSoyad ?? "";
            Rol = rol ?? "";
            ApiToken = apiToken ?? "";

            AdminMi = Rol.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        public static void MagazaSec(
            int musteriId,
            int magazaId,
            string musteriAdi,
            string magazaAdi,
            string sehir)
        {
            SeciliMusteriId = musteriId;
            SeciliMagazaId = magazaId;

            SeciliMusteriAdi = musteriAdi ?? "";
            SeciliMagazaAdi = magazaAdi ?? "";
            SeciliSehir = sehir ?? "";

            TumMagazalar = false;
        }

        public static void TumMagazalariSec()
        {
            if (!AdminMi)
            {
                MagazaSeciminiTemizle();
                return;
            }

            SeciliMusteriId = null;
            SeciliMagazaId = null;

            SeciliMusteriAdi = "";
            SeciliMagazaAdi = "Tüm Mağazalar";
            SeciliSehir = "";

            TumMagazalar = true;
        }

        public static void MagazaSeciminiTemizle()
        {
            SeciliMusteriId = null;
            SeciliMagazaId = null;

            SeciliMusteriAdi = "";
            SeciliMagazaAdi = "";
            SeciliSehir = "";

            TumMagazalar = false;
        }

        public static void CikisYap()
        {
            KullaniciId = 0;
            KullaniciAdi = "";
            AdSoyad = "";
            Rol = "";
            ApiToken = "";

            SeciliMusteriId = null;
            SeciliMagazaId = null;

            SeciliMusteriAdi = "";
            SeciliMagazaAdi = "";
            SeciliSehir = "";

            AdminMi = false;
            TumMagazalar = false;
        }
    }
}
