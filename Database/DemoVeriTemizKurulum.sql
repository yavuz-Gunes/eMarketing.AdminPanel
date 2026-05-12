/*
    DIKKAT: Bu script demo ortami icin hazirlanmistir.
    Siparis, stok, musteri, bayi, urun, kategori ve personel demo verilerini temizleyip yeniden kurar.
    Calistirmadan once mevcut verilerin silinecegini kabul ettiginizden emin olun.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.MagazaStokHareketleri;
DELETE FROM dbo.MagazaStoklari;
IF OBJECT_ID('dbo.StockMovements', 'U') IS NOT NULL DELETE FROM dbo.StockMovements;
DELETE FROM dbo.OrderItems;
DELETE FROM dbo.Orders;
DELETE FROM dbo.KullaniciMagazalari;

IF OBJECT_ID('dbo.BayiYetkilileri', 'U') IS NOT NULL
    DELETE FROM dbo.BayiYetkilileri;

IF OBJECT_ID('dbo.StoreInventory', 'U') IS NOT NULL
    DELETE FROM dbo.StoreInventory;

DELETE FROM dbo.Products;
DELETE FROM dbo.Categories;
DELETE FROM dbo.CustomerStores;
DELETE FROM dbo.Customers;
DELETE FROM dbo.Kullanicilar;

DBCC CHECKIDENT ('dbo.MagazaStokHareketleri', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.MagazaStoklari', RESEED, 0) WITH NO_INFOMSGS;
IF OBJECT_ID('dbo.StockMovements', 'U') IS NOT NULL DBCC CHECKIDENT ('dbo.StockMovements', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.OrderItems', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Orders', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.KullaniciMagazalari', RESEED, 0) WITH NO_INFOMSGS;
IF OBJECT_ID('dbo.BayiYetkilileri', 'U') IS NOT NULL
    DBCC CHECKIDENT ('dbo.BayiYetkilileri', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Products', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Categories', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.CustomerStores', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Customers', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Kullanicilar', RESEED, 0) WITH NO_INFOMSGS;

INSERT INTO dbo.Categories (CategoryName, IsActive)
VALUES
    (N'Fren Sistemi', 1),
    (N'Filtre Grubu', 1),
    (N'Motor Parçaları', 1),
    (N'Elektrik', 1),
    (N'Süspansiyon', 1);

DECLARE @Fren INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Fren Sistemi');
DECLARE @Filtre INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Filtre Grubu');
DECLARE @Motor INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Motor Parçaları');
DECLARE @Elektrik INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Elektrik');
DECLARE @Suspansiyon INT = (SELECT CategoryId FROM dbo.Categories WHERE CategoryName = N'Süspansiyon');

INSERT INTO dbo.Products (ProductName, Description, Price, Stock, ImageUrl, IsActive, CategoryId)
VALUES
    (N'Fren Balatası Ön Takım', N'Binek araçlar için ön fren balata takımı', 1250.00, 80, NULL, 1, @Fren),
    (N'Yağ Filtresi', N'1.6 motor uyumlu yağ filtresi', 180.00, 220, NULL, 1, @Filtre),
    (N'Hava Filtresi', N'Standart hava filtresi', 210.00, 160, NULL, 1, @Filtre),
    (N'Buji Takımı', N'4lü performans buji seti', 480.00, 95, NULL, 1, @Motor),
    (N'Akü 60Ah', N'60 amper bakım gerektirmeyen akü', 2450.00, 36, NULL, 1, @Elektrik),
    (N'Amortisör Ön Sağ', N'Ön sağ amortisör', 1750.00, 42, NULL, 1, @Suspansiyon);

INSERT INTO dbo.Customers
(
    FullName,
    CompanyName,
    AuthorizedPerson,
    Phone,
    Email,
    TaxNumber,
    TaxOffice,
    Address,
    CustomerType,
    IsActive
)
VALUES
    (N'İstanbul Yedek Parça Ltd.', N'İstanbul Yedek Parça', N'Emre Kaya', N'02125550101', N'istanbul@bayi.local', N'1111111111', N'Maslak', N'İstanbul / Maslak', N'Bayi', 1),
    (N'Ankara Oto Servis A.Ş.', N'Ankara Oto Servis', N'Selin Demir', N'03124440202', N'ankara@bayi.local', N'2222222222', N'Ostim', N'Ankara / Ostim', N'Bayi', 1),
    (N'İzmir Motor Market', N'İzmir Motor Market', N'Murat Arslan', N'02323330303', N'izmir@bayi.local', N'3333333333', N'Bornova', N'İzmir / Bornova', N'Bayi', 1);

DECLARE @IstanbulBayi INT = (SELECT CustomerId FROM dbo.Customers WHERE CompanyName = N'İstanbul Yedek Parça');
DECLARE @AnkaraBayi INT = (SELECT CustomerId FROM dbo.Customers WHERE CompanyName = N'Ankara Oto Servis');
DECLARE @IzmirBayi INT = (SELECT CustomerId FROM dbo.Customers WHERE CompanyName = N'İzmir Motor Market');

INSERT INTO dbo.CustomerStores
(
    CustomerId,
    StoreName,
    City,
    District,
    Address,
    Phone,
    ResponsiblePerson,
    IsActive
)
VALUES
    (@IstanbulBayi, N'İstanbul Yedek Parça Ana Mağaza', N'İstanbul', N'Maslak', N'Maslak Sanayi Sitesi No:12', N'02125550111', N'Emre Kaya', 1),
    (@IstanbulBayi, N'İstanbul Yedek Parça Anadolu', N'İstanbul', N'Ümraniye', N'İMES Sanayi Sitesi No:45', N'02165550222', N'Aylin Koç', 1),
    (@AnkaraBayi, N'Ankara Oto Servis Ana Mağaza', N'Ankara', N'Ostim', N'Ostim OSB No:18', N'03124440333', N'Selin Demir', 1),
    (@IzmirBayi, N'İzmir Motor Market Ana Mağaza', N'İzmir', N'Bornova', N'Bornova Sanayi No:7', N'02323330444', N'Murat Arslan', 1);

DECLARE @IstanbulAna INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'İstanbul Yedek Parça Ana Mağaza');
DECLARE @IstanbulAnadolu INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'İstanbul Yedek Parça Anadolu');
DECLARE @AnkaraAna INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'Ankara Oto Servis Ana Mağaza');
DECLARE @IzmirAna INT = (SELECT CustomerStoreId FROM dbo.CustomerStores WHERE StoreName = N'İzmir Motor Market Ana Mağaza');

INSERT INTO dbo.Kullanicilar (KullaniciAdi, Sifre, AdSoyad, Rol, Telefon, Email, ImageUrl, AktifMi)
VALUES
    (N'admin', N'1234', N'Admin', N'Admin', N'', N'admin@emarketing.local', NULL, 1),
    (N'emre.kaya', N'1234', N'Emre Kaya', N'Personel', N'05321111111', N'emre.kaya@istanbulyp.local', NULL, 1),
    (N'selin.demir', N'1234', N'Selin Demir', N'Personel', N'05332222222', N'selin.demir@ankaraoto.local', NULL, 1),
    (N'murat.arslan', N'1234', N'Murat Arslan', N'Personel', N'05323333333', N'murat.arslan@izmirmotor.local', NULL, 1),
    (N'ahmet.yilmaz', N'1234', N'Ahmet Yılmaz', N'Personel', N'05321234567', N'ahmet.yilmaz@istanbulyp.local', NULL, 1),
    (N'aylin.koc', N'1234', N'Aylin Koç', N'Personel', N'05321234568', N'aylin.koc@istanbulyp.local', NULL, 1),
    (N'mehmet.demir', N'1234', N'Mehmet Demir', N'Personel', N'05334445566', N'mehmet.demir@ankaraoto.local', NULL, 1),
    (N'deniz.kara', N'1234', N'Deniz Kara', N'Personel', N'05326667788', N'deniz.kara@izmirmotor.local', NULL, 1);

DECLARE @Admin INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'admin');
DECLARE @Emre INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'emre.kaya');
DECLARE @Selin INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'selin.demir');
DECLARE @Murat INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'murat.arslan');
DECLARE @AhmetUser INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'ahmet.yilmaz');
DECLARE @AylinUser INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'aylin.koc');
DECLARE @MehmetUser INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'mehmet.demir');
DECLARE @DenizUser INT = (SELECT KullaniciId FROM dbo.Kullanicilar WHERE KullaniciAdi = N'deniz.kara');

INSERT INTO dbo.KullaniciMagazalari (KullaniciId, MagazaId, Gorev, AktifMi)
VALUES
    (@Admin, @IstanbulAna, N'MagazaMuduru', 1),
    (@Admin, @IstanbulAnadolu, N'MagazaMuduru', 1),
    (@Admin, @AnkaraAna, N'MagazaMuduru', 1),
    (@Admin, @IzmirAna, N'MagazaMuduru', 1),
    (@Emre, @IstanbulAna, N'MagazaMuduru', 1),
    (@Emre, @IstanbulAnadolu, N'Supervisor', 1),
    (@Selin, @AnkaraAna, N'MagazaMuduru', 1),
    (@Murat, @IzmirAna, N'MagazaMuduru', 1),
    (@AhmetUser, @IstanbulAna, N'Personel', 1),
    (@AylinUser, @IstanbulAnadolu, N'Supervisor', 1),
    (@MehmetUser, @AnkaraAna, N'Personel', 1),
    (@DenizUser, @IzmirAna, N'Personel', 1);

DECLARE @FrenBalata INT = (SELECT ProductId FROM dbo.Products WHERE ProductName = N'Fren Balatası Ön Takım');
DECLARE @YagFiltre INT = (SELECT ProductId FROM dbo.Products WHERE ProductName = N'Yağ Filtresi');
DECLARE @HavaFiltre INT = (SELECT ProductId FROM dbo.Products WHERE ProductName = N'Hava Filtresi');
DECLARE @Buji INT = (SELECT ProductId FROM dbo.Products WHERE ProductName = N'Buji Takımı');
DECLARE @Aku INT = (SELECT ProductId FROM dbo.Products WHERE ProductName = N'Akü 60Ah');
DECLARE @Amortisor INT = (SELECT ProductId FROM dbo.Products WHERE ProductName = N'Amortisör Ön Sağ');

UPDATE dbo.CustomerStores
SET SorumluKullaniciId = CASE
    WHEN CustomerStoreId = @IstanbulAna THEN @Emre
    WHEN CustomerStoreId = @IstanbulAnadolu THEN @AylinUser
    WHEN CustomerStoreId = @AnkaraAna THEN @Selin
    WHEN CustomerStoreId = @IzmirAna THEN @Murat
    ELSE SorumluKullaniciId
END
WHERE CustomerStoreId IN (@IstanbulAna, @IstanbulAnadolu, @AnkaraAna, @IzmirAna);

INSERT INTO dbo.BayiYetkilileri
(
    BayiId,
    MagazaId,
    KullaniciId,
    YetkiTipi,
    Notlar,
    AktifMi
)
VALUES
    (@IstanbulBayi, @IstanbulAna, @AhmetUser, N'SiparisYetkilisi', N'Ana mağaza siparişlerinden sorumlu.', 1),
    (@IstanbulBayi, @IstanbulAnadolu, @AylinUser, N'SiparisYetkilisi', N'Anadolu yakası siparişlerinden sorumlu.', 1),
    (@AnkaraBayi, @AnkaraAna, @MehmetUser, N'SiparisYetkilisi', N'Web siparişlerini takip eder.', 1),
    (@IzmirBayi, @IzmirAna, @Murat, N'SiparisYetkilisi', N'İzmir mağaza siparişlerinden sorumlu.', 1);

DECLARE @AhmetYetkili INT = (SELECT BayiYetkiliId FROM dbo.BayiYetkilileri WHERE KullaniciId = @AhmetUser AND MagazaId = @IstanbulAna);
DECLARE @AylinYetkili INT = (SELECT BayiYetkiliId FROM dbo.BayiYetkilileri WHERE KullaniciId = @AylinUser AND MagazaId = @IstanbulAnadolu);
DECLARE @MehmetYetkili INT = (SELECT BayiYetkiliId FROM dbo.BayiYetkilileri WHERE KullaniciId = @MehmetUser AND MagazaId = @AnkaraAna);
DECLARE @MuratYetkili INT = (SELECT BayiYetkiliId FROM dbo.BayiYetkilileri WHERE KullaniciId = @Murat AND MagazaId = @IzmirAna AND YetkiTipi = N'SiparisYetkilisi');

DECLARE @SiparisId INT;
DECLARE @MagazaStokId INT;
DECLARE @YeniSiparis TABLE (SiparisId INT);

DELETE FROM @YeniSiparis;
INSERT INTO @YeniSiparis
EXEC dbo.sp_Siparis_Ekle_TekUrun
    @CustomerName = N'İstanbul Yedek Parça',
    @ProductId = @FrenBalata,
    @Quantity = 4,
    @TotalPrice = 5000.00,
    @CustomerStoreId = @IstanbulAna,
    @OrderType = N'Bayi',
    @OrderSource = N'AdminPanel',
    @BayiYetkiliId = @AhmetYetkili;
SELECT @SiparisId = SiparisId FROM @YeniSiparis;
EXEC dbo.sp_Siparis_Durum_Guncelle @SiparisId = @SiparisId, @SiparisDurumu = N'Teslim Edildi';
SELECT @MagazaStokId = MagazaStokId FROM dbo.MagazaStoklari WHERE MagazaId = @IstanbulAna AND ProductId = @FrenBalata;
EXEC dbo.sp_MagazaStok_MinimumGuncelle @MagazaStokId = @MagazaStokId, @MinimumStok = 2;

DELETE FROM @YeniSiparis;
INSERT INTO @YeniSiparis
EXEC dbo.sp_Siparis_Ekle_TekUrun
    @CustomerName = N'İstanbul Yedek Parça',
    @ProductId = @YagFiltre,
    @Quantity = 12,
    @TotalPrice = 2160.00,
    @CustomerStoreId = @IstanbulAnadolu,
    @OrderType = N'Bayi',
    @OrderSource = N'AdminPanel',
    @BayiYetkiliId = @AylinYetkili;
SELECT @SiparisId = SiparisId FROM @YeniSiparis;
EXEC dbo.sp_Siparis_Durum_Guncelle @SiparisId = @SiparisId, @SiparisDurumu = N'Teslim Edildi';
SELECT @MagazaStokId = MagazaStokId FROM dbo.MagazaStoklari WHERE MagazaId = @IstanbulAnadolu AND ProductId = @YagFiltre;
EXEC dbo.sp_MagazaStok_MinimumGuncelle @MagazaStokId = @MagazaStokId, @MinimumStok = 6;

DELETE FROM @YeniSiparis;
INSERT INTO @YeniSiparis
EXEC dbo.sp_Siparis_Ekle_TekUrun
    @CustomerName = N'Ankara Oto Servis',
    @ProductId = @Aku,
    @Quantity = 2,
    @TotalPrice = 4900.00,
    @CustomerStoreId = @AnkaraAna,
    @OrderType = N'Bayi',
    @OrderSource = N'AdminPanel',
    @BayiYetkiliId = @MehmetYetkili;
SELECT @SiparisId = SiparisId FROM @YeniSiparis;
EXEC dbo.sp_Siparis_Durum_Guncelle @SiparisId = @SiparisId, @SiparisDurumu = N'Teslim Edildi';
SELECT @MagazaStokId = MagazaStokId FROM dbo.MagazaStoklari WHERE MagazaId = @AnkaraAna AND ProductId = @Aku;
EXEC dbo.sp_MagazaStok_MinimumGuncelle @MagazaStokId = @MagazaStokId, @MinimumStok = 3;

DELETE FROM @YeniSiparis;
INSERT INTO @YeniSiparis
EXEC dbo.sp_Siparis_Ekle_TekUrun
    @CustomerName = N'İzmir Motor Market',
    @ProductId = @Buji,
    @Quantity = 6,
    @TotalPrice = 2880.00,
    @CustomerStoreId = @IzmirAna,
    @OrderType = N'Bayi',
    @OrderSource = N'AdminPanel',
    @BayiYetkiliId = @MuratYetkili;
SELECT @SiparisId = SiparisId FROM @YeniSiparis;
EXEC dbo.sp_Siparis_Durum_Guncelle @SiparisId = @SiparisId, @SiparisDurumu = N'Kargoda';

DELETE FROM @YeniSiparis;
INSERT INTO @YeniSiparis
EXEC dbo.sp_Siparis_Ekle_TekUrun
    @CustomerName = N'Ankara Oto Servis',
    @ProductId = @Amortisor,
    @Quantity = 3,
    @TotalPrice = 5250.00,
    @CustomerStoreId = @AnkaraAna,
    @OrderType = N'Bayi',
    @OrderSource = N'AdminPanel',
    @BayiYetkiliId = @MehmetYetkili;
SELECT @SiparisId = SiparisId FROM @YeniSiparis;

COMMIT TRANSACTION;

SELECT
    (SELECT COUNT(*) FROM dbo.Customers) AS BayiSayisi,
    (SELECT COUNT(*) FROM dbo.CustomerStores) AS MagazaSayisi,
    (SELECT COUNT(*) FROM dbo.BayiYetkilileri) AS BayiYetkiliSayisi,
    (SELECT COUNT(*) FROM dbo.Products) AS UrunSayisi,
    (SELECT COUNT(*) FROM dbo.Orders) AS SiparisSayisi,
    (SELECT COUNT(*) FROM dbo.MagazaStoklari) AS BayiStokKarti;
