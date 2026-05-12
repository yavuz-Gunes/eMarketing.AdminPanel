/*
    Demo/test ortamı için hedefli yetki düzeltmesi.
    Genel demo verisini silmez; yalnızca bilinen demo kullanıcıların mağaza ve
    sipariş yetkilisi ilişkilerini MVP yetki kurgusuyla hizalar.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Admin INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'admin');
DECLARE @Emre INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'emre.kaya');
DECLARE @Selin INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'selin.demir');
DECLARE @Ahmet INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'ahmet.yilmaz');
DECLARE @Aylin INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'aylin.koc');
DECLARE @Mehmet INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'mehmet.demir');
DECLARE @Deniz INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'deniz.kara');
DECLARE @Murat INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'murat.arslan');

DECLARE @IstanbulBayi INT = (SELECT CustomerId FROM dbo.Customers WHERE CompanyName = N'İstanbul Yedek Parça');
DECLARE @AnkaraBayi INT = (SELECT CustomerId FROM dbo.Customers WHERE CompanyName = N'Ankara Oto Servis');
DECLARE @IzmirBayi INT = (SELECT CustomerId FROM dbo.Customers WHERE CompanyName = N'İzmir Motor Market');

DECLARE @IstanbulAna INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'İstanbul Yedek Parça Ana Mağaza');
DECLARE @IstanbulAnadolu INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'İstanbul Yedek Parça Anadolu');
DECLARE @AnkaraAna INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'Ankara Oto Servis Ana Mağaza');
DECLARE @IzmirAna INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'İzmir Motor Market Ana Mağaza');

IF @Admin IS NULL OR @Emre IS NULL OR @Selin IS NULL OR @Ahmet IS NULL OR @Aylin IS NULL OR @Mehmet IS NULL OR @Deniz IS NULL OR @Murat IS NULL
    THROW 51000, N'Demo kullanıcıları bulunamadı. Önce demo veri kurulumunu kontrol edin.', 1;

IF @IstanbulBayi IS NULL OR @AnkaraBayi IS NULL OR @IzmirBayi IS NULL
    THROW 51001, N'Demo bayi kayıtları bulunamadı. Önce demo veri kurulumunu kontrol edin.', 1;

IF @IstanbulAna IS NULL OR @IstanbulAnadolu IS NULL OR @AnkaraAna IS NULL OR @IzmirAna IS NULL
    THROW 51002, N'Demo mağaza kayıtları bulunamadı. Script UTF-8 olarak çalıştırılmalı veya demo veri kurulumu kontrol edilmeli.', 1;

UPDATE dbo.Kullanicilar
SET Rol = N'Admin'
WHERE KullaniciId = @Admin;

UPDATE dbo.Kullanicilar
SET Rol = N'Personel'
WHERE KullaniciId IN (@Emre, @Selin, @Ahmet, @Aylin, @Mehmet, @Deniz, @Murat);

UPDATE dbo.KullaniciMagazalari
SET AktifMi = 0
WHERE KullaniciId IN (@Ahmet, @Aylin, @Mehmet, @Deniz, @Murat)
  AND MagazaId NOT IN (@IstanbulAna, @IstanbulAnadolu, @AnkaraAna, @IzmirAna);

UPDATE dbo.KullaniciMagazalari
SET AktifMi = 0
WHERE (KullaniciId = @Emre AND MagazaId NOT IN (@IstanbulAna, @IstanbulAnadolu))
   OR (KullaniciId = @Selin AND MagazaId <> @AnkaraAna)
   OR (KullaniciId = @Ahmet AND MagazaId <> @IstanbulAna)
   OR (KullaniciId = @Aylin AND MagazaId <> @IstanbulAnadolu)
   OR (KullaniciId = @Mehmet AND MagazaId <> @AnkaraAna)
   OR (KullaniciId = @Deniz AND MagazaId <> @IzmirAna)
   OR (KullaniciId = @Murat AND MagazaId <> @IzmirAna);

MERGE dbo.KullaniciMagazalari AS hedef
USING
(
    SELECT @Emre AS KullaniciId, @IstanbulAna AS MagazaId, N'MagazaMuduru' AS Gorev
    UNION ALL SELECT @Emre, @IstanbulAnadolu, N'Supervisor'
    UNION ALL SELECT @Selin, @AnkaraAna, N'MagazaMuduru'
    UNION ALL SELECT @Ahmet, @IstanbulAna, N'Personel'
    UNION ALL SELECT @Aylin, @IstanbulAnadolu, N'Supervisor'
    UNION ALL SELECT @Mehmet, @AnkaraAna, N'Personel'
    UNION ALL SELECT @Deniz, @IzmirAna, N'Personel'
    UNION ALL SELECT @Murat, @IzmirAna, N'MagazaMuduru'
) AS kaynak
ON hedef.KullaniciId = kaynak.KullaniciId AND hedef.MagazaId = kaynak.MagazaId
WHEN MATCHED THEN
    UPDATE SET AktifMi = 1,
               Gorev = kaynak.Gorev
WHEN NOT MATCHED THEN
    INSERT (KullaniciId, MagazaId, Gorev, AktifMi)
    VALUES (kaynak.KullaniciId, kaynak.MagazaId, kaynak.Gorev, 1);

UPDATE dbo.BayiYetkilileri
SET AktifMi = 0
WHERE KullaniciId IN (@Emre, @Selin, @Ahmet, @Aylin, @Mehmet, @Deniz, @Murat);

MERGE dbo.BayiYetkilileri AS hedef
USING
(
    SELECT @IstanbulBayi AS BayiId, @IstanbulAna AS MagazaId, @Ahmet AS KullaniciId, N'SiparisYetkilisi' AS YetkiTipi, N'Ana mağaza siparişlerinden sorumlu.' AS Notlar
    UNION ALL SELECT @IstanbulBayi, @IstanbulAnadolu, @Aylin, N'SiparisYetkilisi', N'Anadolu yakası siparişlerinden sorumlu.'
    UNION ALL SELECT @AnkaraBayi, @AnkaraAna, @Mehmet, N'SiparisYetkilisi', N'Web siparişlerini takip eder.'
    UNION ALL SELECT @IzmirBayi, @IzmirAna, @Murat, N'SiparisYetkilisi', N'İzmir mağaza siparişlerinden sorumlu.'
) AS kaynak
ON hedef.KullaniciId = kaynak.KullaniciId
   AND hedef.BayiId = kaynak.BayiId
   AND hedef.MagazaId = kaynak.MagazaId
WHEN MATCHED THEN
    UPDATE SET
        YetkiTipi = kaynak.YetkiTipi,
        Notlar = kaynak.Notlar,
        AktifMi = 1,
        GuncellemeTarihi = SYSDATETIME()
WHEN NOT MATCHED THEN
    INSERT (BayiId, MagazaId, KullaniciId, YetkiTipi, Notlar, AktifMi)
    VALUES (kaynak.BayiId, kaynak.MagazaId, kaynak.KullaniciId, kaynak.YetkiTipi, kaynak.Notlar, 1);

COMMIT TRANSACTION;

SELECT
    k.KullaniciAdi,
    km.MagazaId,
    cs.StoreName,
    km.AktifMi
FROM dbo.KullaniciMagazalari km
INNER JOIN dbo.Kullanicilar k ON k.KullaniciId = km.KullaniciId
INNER JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = km.MagazaId
WHERE k.KullaniciAdi IN (N'emre.kaya', N'selin.demir', N'ahmet.yilmaz', N'aylin.koc', N'mehmet.demir', N'deniz.kara', N'murat.arslan')
ORDER BY k.KullaniciAdi, km.MagazaId;
