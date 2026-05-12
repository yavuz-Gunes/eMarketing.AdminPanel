IF COL_LENGTH('dbo.Kullanicilar', 'Telefon') IS NULL
BEGIN
    ALTER TABLE dbo.Kullanicilar ADD Telefon NVARCHAR(60) NULL;
END
GO

IF COL_LENGTH('dbo.Kullanicilar', 'Email') IS NULL
BEGIN
    ALTER TABLE dbo.Kullanicilar ADD Email NVARCHAR(400) NULL;
END
GO

IF COL_LENGTH('dbo.Kullanicilar', 'ImageUrl') IS NULL
BEGIN
    ALTER TABLE dbo.Kullanicilar ADD ImageUrl NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH('dbo.CustomerStores', 'SorumluKullaniciId') IS NULL
BEGIN
    ALTER TABLE dbo.CustomerStores ADD SorumluKullaniciId INT NULL;
END
GO

IF OBJECT_ID('dbo.KullaniciMagazalari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KullaniciMagazalari
    (
        KullaniciMagazaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KullaniciMagazalari PRIMARY KEY,
        KullaniciId INT NOT NULL,
        MagazaId INT NOT NULL,
        Gorev NVARCHAR(30) NOT NULL CONSTRAINT DF_KullaniciMagazalari_Gorev DEFAULT (N'Personel'),
        AktifMi BIT NOT NULL CONSTRAINT DF_KullaniciMagazalari_AktifMi DEFAULT (1),
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_KullaniciMagazalari_OlusturmaTarihi DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_KullaniciMagazalari_Kullanici_Magaza UNIQUE (KullaniciId, MagazaId),
        CONSTRAINT FK_KullaniciMagazalari_Kullanicilar FOREIGN KEY (KullaniciId)
            REFERENCES dbo.Kullanicilar(KullaniciId),
        CONSTRAINT FK_KullaniciMagazalari_CustomerStores FOREIGN KEY (MagazaId)
            REFERENCES dbo.CustomerStores(CustomerStoreId)
    );
END
GO

IF OBJECT_ID('dbo.KullaniciMagazalari', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.KullaniciMagazalari', 'Gorev') IS NULL
BEGIN
    ALTER TABLE dbo.KullaniciMagazalari
    ADD Gorev NVARCHAR(30) NOT NULL
        CONSTRAINT DF_KullaniciMagazalari_Gorev DEFAULT (N'Personel');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CustomerStores_SorumluKullanici')
BEGIN
    ALTER TABLE dbo.CustomerStores
    ADD CONSTRAINT FK_CustomerStores_SorumluKullanici
        FOREIGN KEY (SorumluKullaniciId) REFERENCES dbo.Kullanicilar(KullaniciId);
END
GO

IF OBJECT_ID('dbo.BayiYetkilileri', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BayiYetkilileri
    (
        BayiYetkiliId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BayiYetkilileri PRIMARY KEY,
        KullaniciId INT NOT NULL,
        BayiId INT NOT NULL,
        MagazaId INT NOT NULL,
        YetkiTipi NVARCHAR(50) NOT NULL CONSTRAINT DF_BayiYetkilileri_YetkiTipi DEFAULT (N'SiparisYetkilisi'),
        Notlar NVARCHAR(500) NULL,
        AktifMi BIT NOT NULL CONSTRAINT DF_BayiYetkilileri_AktifMi DEFAULT (1),
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_BayiYetkilileri_OlusturmaTarihi DEFAULT (SYSDATETIME()),
        GuncellemeTarihi DATETIME2 NOT NULL CONSTRAINT DF_BayiYetkilileri_GuncellemeTarihi DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_BayiYetkilileri_Kullanicilar FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(KullaniciId),
        CONSTRAINT FK_BayiYetkilileri_Customers FOREIGN KEY (BayiId) REFERENCES dbo.Customers(CustomerId),
        CONSTRAINT FK_BayiYetkilileri_CustomerStores FOREIGN KEY (MagazaId) REFERENCES dbo.CustomerStores(CustomerStoreId)
    );
END
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'KullaniciId') IS NULL
BEGIN
    ALTER TABLE dbo.BayiYetkilileri ADD KullaniciId INT NULL;
END
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'YetkiTipi') IS NULL
BEGIN
    ALTER TABLE dbo.BayiYetkilileri ADD YetkiTipi NVARCHAR(50) NULL;
END
GO

IF COL_LENGTH('dbo.Orders', 'BayiYetkiliId') IS NULL
BEGIN
    ALTER TABLE dbo.Orders ADD BayiYetkiliId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Orders_BayiYetkilileri')
BEGIN
    ALTER TABLE dbo.Orders
    ADD CONSTRAINT FK_Orders_BayiYetkilileri
        FOREIGN KEY (BayiYetkiliId) REFERENCES dbo.BayiYetkilileri(BayiYetkiliId);
END
GO

IF OBJECT_ID('dbo.vw_BayiYetkili_Liste', 'V') IS NOT NULL
    DROP VIEW dbo.vw_BayiYetkili_Liste;
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'Email') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE byk
        SET KullaniciId = k.KullaniciId
        FROM dbo.BayiYetkilileri byk
        INNER JOIN dbo.Kullanicilar k
            ON LOWER(k.KullaniciAdi) = LOWER(LEFT(NULLIF(LTRIM(RTRIM(byk.Email)), ''''), CHARINDEX(''@'', NULLIF(LTRIM(RTRIM(byk.Email)), '''') + ''@'') - 1))
        WHERE byk.KullaniciId IS NULL
          AND NULLIF(LTRIM(RTRIM(byk.Email)), '''') IS NOT NULL;
    ');
END
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'Email') IS NOT NULL
BEGIN
    EXEC(N'
        INSERT INTO dbo.Kullanicilar (KullaniciAdi, Sifre, AdSoyad, Rol, Telefon, Email, ImageUrl, AktifMi)
        SELECT
            LEFT(NULLIF(LTRIM(RTRIM(byk.Email)), ''''), CHARINDEX(''@'', NULLIF(LTRIM(RTRIM(byk.Email)), '''') + ''@'') - 1),
            N''1234'',
            NULLIF(LTRIM(RTRIM(ISNULL(byk.AdSoyad, N''''))), ''''),
            CASE
                WHEN byk.Gorev IN (N''Admin'') THEN N''Admin''
                WHEN byk.Gorev IN (N''Yönetici'', N''Yonetici'', N''StoreManager'') THEN N''StoreManager''
                ELSE N''SalesPerson''
            END,
            NULLIF(LTRIM(RTRIM(byk.Telefon)), ''''),
            NULLIF(LTRIM(RTRIM(byk.Email)), ''''),
            NULL,
            byk.AktifMi
        FROM dbo.BayiYetkilileri byk
        WHERE byk.KullaniciId IS NULL
          AND NULLIF(LTRIM(RTRIM(byk.Email)), '''') IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Kullanicilar k
              WHERE LOWER(k.KullaniciAdi) = LOWER(LEFT(NULLIF(LTRIM(RTRIM(byk.Email)), ''''), CHARINDEX(''@'', NULLIF(LTRIM(RTRIM(byk.Email)), '''') + ''@'') - 1))
          );
    ');
END
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'Email') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE byk
        SET KullaniciId = k.KullaniciId
        FROM dbo.BayiYetkilileri byk
        INNER JOIN dbo.Kullanicilar k
            ON LOWER(k.KullaniciAdi) = LOWER(LEFT(NULLIF(LTRIM(RTRIM(byk.Email)), ''''), CHARINDEX(''@'', NULLIF(LTRIM(RTRIM(byk.Email)), '''') + ''@'') - 1))
        WHERE byk.KullaniciId IS NULL
          AND NULLIF(LTRIM(RTRIM(byk.Email)), '''') IS NOT NULL;
    ');
END
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'AdSoyad') IS NOT NULL
BEGIN
    EXEC(N'
        INSERT INTO dbo.Kullanicilar (KullaniciAdi, Sifre, AdSoyad, Rol, AktifMi)
        SELECT
            N''yetkili'' + CONVERT(NVARCHAR(20), byk.BayiYetkiliId),
            N''1234'',
            NULLIF(LTRIM(RTRIM(byk.AdSoyad)), ''''),
            N''SalesPerson'',
            byk.AktifMi
        FROM dbo.BayiYetkilileri byk
        WHERE byk.KullaniciId IS NULL
          AND NULLIF(LTRIM(RTRIM(byk.AdSoyad)), '''') IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Kullanicilar k
              WHERE k.KullaniciAdi = N''yetkili'' + CONVERT(NVARCHAR(20), byk.BayiYetkiliId)
          );

        UPDATE byk
        SET KullaniciId = k.KullaniciId
        FROM dbo.BayiYetkilileri byk
        INNER JOIN dbo.Kullanicilar k
            ON k.KullaniciAdi = N''yetkili'' + CONVERT(NVARCHAR(20), byk.BayiYetkiliId)
        WHERE byk.KullaniciId IS NULL;
    ');
END
GO

UPDATE byk
SET MagazaId = cs.CustomerStoreId
FROM dbo.BayiYetkilileri byk
CROSS APPLY
(
    SELECT TOP 1 CustomerStoreId
    FROM dbo.CustomerStores cs
    WHERE cs.CustomerId = byk.BayiId
      AND cs.IsActive = 1
    ORDER BY CustomerStoreId
) cs
WHERE byk.MagazaId IS NULL;
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'Gorev') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE dbo.BayiYetkilileri
        SET YetkiTipi = CASE
            WHEN Gorev IN (N''Satın Alma Yetkilisi'', N''Satin Alma Yetkilisi'') THEN N''Satinalma''
            WHEN Gorev IN (N''Operasyon Yetkilisi'') THEN N''Operasyon''
            ELSE N''SiparisYetkilisi''
        END
        WHERE NULLIF(LTRIM(RTRIM(YetkiTipi)), '''') IS NULL;
    ');
END
GO

UPDATE dbo.BayiYetkilileri
SET YetkiTipi = N'SiparisYetkilisi'
WHERE NULLIF(LTRIM(RTRIM(YetkiTipi)), '') IS NULL;
GO

UPDATE dbo.BayiYetkilileri
SET AktifMi = 0,
    GuncellemeTarihi = SYSDATETIME()
WHERE YetkiTipi <> N'SiparisYetkilisi'
  AND AktifMi = 1;
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'Email') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE k
        SET
            Telefon = COALESCE(NULLIF(k.Telefon, ''''), NULLIF(LTRIM(RTRIM(byk.Telefon)), '''')),
            Email = COALESCE(NULLIF(k.Email, ''''), NULLIF(LTRIM(RTRIM(byk.Email)), ''''))
        FROM dbo.Kullanicilar k
        INNER JOIN dbo.BayiYetkilileri byk ON byk.KullaniciId = k.KullaniciId;
    ');
END
GO

IF COL_LENGTH('dbo.BayiYetkilileri', 'ImageUrl') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE k
        SET ImageUrl = COALESCE(NULLIF(k.ImageUrl, ''''), NULLIF(LTRIM(RTRIM(byk.ImageUrl)), ''''))
        FROM dbo.Kullanicilar k
        INNER JOIN dbo.BayiYetkilileri byk ON byk.KullaniciId = k.KullaniciId;
    ');
END
GO

IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL
BEGIN
    UPDATE o
    SET BayiYetkiliId = NULL
    FROM dbo.Orders o
    INNER JOIN dbo.BayiYetkilileri byk
        ON byk.BayiYetkiliId = o.BayiYetkiliId
    WHERE byk.KullaniciId IS NULL
       OR byk.MagazaId IS NULL
       OR byk.BayiId IS NULL;
END

DELETE FROM dbo.BayiYetkilileri
WHERE KullaniciId IS NULL
   OR MagazaId IS NULL
   OR BayiId IS NULL;
GO

;WITH duplicates AS
(
    SELECT
        BayiYetkiliId,
        FIRST_VALUE(BayiYetkiliId) OVER
        (
            PARTITION BY KullaniciId, BayiId, MagazaId
            ORDER BY AktifMi DESC, BayiYetkiliId
        ) AS SurvivorId,
        ROW_NUMBER() OVER
        (
            PARTITION BY KullaniciId, BayiId, MagazaId
            ORDER BY AktifMi DESC, BayiYetkiliId
        ) AS RowNo
    FROM dbo.BayiYetkilileri
)
UPDATE o
SET BayiYetkiliId = d.SurvivorId
FROM dbo.Orders o
INNER JOIN duplicates d
    ON d.BayiYetkiliId = o.BayiYetkiliId
WHERE d.RowNo > 1;

;WITH duplicates AS
(
    SELECT
        BayiYetkiliId,
        ROW_NUMBER() OVER
        (
            PARTITION BY KullaniciId, BayiId, MagazaId
            ORDER BY AktifMi DESC, BayiYetkiliId
        ) AS RowNo
    FROM dbo.BayiYetkilileri
)
DELETE FROM duplicates WHERE RowNo > 1;
GO

IF OBJECT_ID('dbo.KullaniciMagazalari', 'U') IS NOT NULL
BEGIN
    UPDATE km
    SET AktifMi = 1
    FROM dbo.KullaniciMagazalari km
    INNER JOIN dbo.BayiYetkilileri byk
        ON byk.KullaniciId = km.KullaniciId
       AND byk.MagazaId = km.MagazaId
       AND byk.AktifMi = 1;

    INSERT INTO dbo.KullaniciMagazalari (KullaniciId, MagazaId, AktifMi)
    SELECT byk.KullaniciId, byk.MagazaId, 1
    FROM dbo.BayiYetkilileri byk
    WHERE byk.AktifMi = 1
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.KullaniciMagazalari km
          WHERE km.KullaniciId = byk.KullaniciId
            AND km.MagazaId = byk.MagazaId
      );
END
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_BayiYetkilileri_Bayi'
      AND object_id = OBJECT_ID('dbo.BayiYetkilileri')
)
BEGIN
    DROP INDEX IX_BayiYetkilileri_Bayi ON dbo.BayiYetkilileri;
END
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_BayiYetkilileri_Magaza'
      AND object_id = OBJECT_ID('dbo.BayiYetkilileri')
)
BEGIN
    DROP INDEX IX_BayiYetkilileri_Magaza ON dbo.BayiYetkilileri;
END
GO

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_BayiYetkilileri_Kullanici_Bayi_Magaza'
      AND object_id = OBJECT_ID('dbo.BayiYetkilileri')
)
BEGIN
    DROP INDEX UX_BayiYetkilileri_Kullanici_Bayi_Magaza ON dbo.BayiYetkilileri;
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'KullaniciId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.BayiYetkilileri ALTER COLUMN KullaniciId INT NOT NULL;
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'MagazaId' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.BayiYetkilileri ALTER COLUMN MagazaId INT NOT NULL;
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'YetkiTipi' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.BayiYetkilileri ALTER COLUMN YetkiTipi NVARCHAR(50) NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BayiYetkilileri_Kullanicilar')
BEGIN
    ALTER TABLE dbo.BayiYetkilileri
    ADD CONSTRAINT FK_BayiYetkilileri_Kullanicilar
        FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(KullaniciId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BayiYetkilileri_Customers')
BEGIN
    ALTER TABLE dbo.BayiYetkilileri
    ADD CONSTRAINT FK_BayiYetkilileri_Customers
        FOREIGN KEY (BayiId) REFERENCES dbo.Customers(CustomerId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BayiYetkilileri_CustomerStores')
BEGIN
    ALTER TABLE dbo.BayiYetkilileri
    ADD CONSTRAINT FK_BayiYetkilileri_CustomerStores
        FOREIGN KEY (MagazaId) REFERENCES dbo.CustomerStores(CustomerStoreId);
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'AdSoyad')
    ALTER TABLE dbo.BayiYetkilileri DROP COLUMN AdSoyad;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'Telefon')
    ALTER TABLE dbo.BayiYetkilileri DROP COLUMN Telefon;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'Email')
    ALTER TABLE dbo.BayiYetkilileri DROP COLUMN Email;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'Gorev')
    ALTER TABLE dbo.BayiYetkilileri DROP COLUMN Gorev;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.BayiYetkilileri') AND name = 'ImageUrl')
    ALTER TABLE dbo.BayiYetkilileri DROP COLUMN ImageUrl;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_BayiYetkilileri_Kullanici_Bayi_Magaza'
      AND object_id = OBJECT_ID('dbo.BayiYetkilileri')
)
BEGIN
    CREATE UNIQUE INDEX UX_BayiYetkilileri_Kullanici_Bayi_Magaza
    ON dbo.BayiYetkilileri(KullaniciId, BayiId, MagazaId);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_BayiYetkilileri_Magaza'
      AND object_id = OBJECT_ID('dbo.BayiYetkilileri')
)
BEGIN
    CREATE INDEX IX_BayiYetkilileri_Magaza
    ON dbo.BayiYetkilileri(MagazaId, AktifMi);
END
GO

CREATE OR ALTER VIEW dbo.vw_Magaza_Secim
AS
SELECT
    c.CustomerId AS MusteriId,
    cs.CustomerStoreId AS MagazaId,
    ISNULL(NULLIF(c.CompanyName, ''), c.FullName) AS MusteriAdi,
    cs.StoreName AS MagazaAdi,
    cs.City AS Sehir,
    cs.District AS Ilce,
    cs.Phone AS Telefon,
    COALESCE(NULLIF(mudur.AdSoyad, ''), NULLIF(supervisor.AdSoyad, ''), NULLIF(k.AdSoyad, ''), NULLIF(cs.ResponsiblePerson, '')) AS SorumluKisi,
    cs.SorumluKullaniciId,
    ISNULL(mudur.AdSoyad, N'') AS MagazaMuduru,
    ISNULL(supervisor.AdSoyad, N'') AS Supervisor,
    ISNULL(personel.PersonelSayisi, 0) AS PersonelSayisi,
    ISNULL(siparisYetkilisi.SiparisYetkilisiSayisi, 0) AS SiparisYetkilisiSayisi,
    c.CustomerType AS MusteriTipi,
    c.IsActive AS MusteriAktifMi,
    cs.IsActive AS MagazaAktifMi,
    ISNULL(siparisOzet.SiparisSayisi, 0) AS SiparisSayisi,
    ISNULL(siparisOzet.ToplamCiro, 0) AS ToplamCiro,
    siparisOzet.SonSiparisTarihi
FROM dbo.CustomerStores cs
INNER JOIN dbo.Customers c ON c.CustomerId = cs.CustomerId
LEFT JOIN dbo.Kullanicilar k ON k.KullaniciId = cs.SorumluKullaniciId
OUTER APPLY
(
    SELECT TOP 1 ku.AdSoyad
    FROM dbo.KullaniciMagazalari km
    INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciId = km.KullaniciId
    WHERE km.MagazaId = cs.CustomerStoreId
      AND km.AktifMi = 1
      AND ku.AktifMi = 1
      AND ku.Rol <> N'Admin'
      AND km.Gorev = N'MagazaMuduru'
    ORDER BY ku.AdSoyad
) mudur
OUTER APPLY
(
    SELECT TOP 1 ku.AdSoyad
    FROM dbo.KullaniciMagazalari km
    INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciId = km.KullaniciId
    WHERE km.MagazaId = cs.CustomerStoreId
      AND km.AktifMi = 1
      AND ku.AktifMi = 1
      AND ku.Rol <> N'Admin'
      AND km.Gorev = N'Supervisor'
    ORDER BY ku.AdSoyad
) supervisor
OUTER APPLY
(
    SELECT COUNT(*) AS PersonelSayisi
    FROM dbo.KullaniciMagazalari km
    INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciId = km.KullaniciId
    WHERE km.MagazaId = cs.CustomerStoreId
      AND km.AktifMi = 1
      AND ku.AktifMi = 1
      AND ku.Rol <> N'Admin'
) personel
OUTER APPLY
(
    SELECT COUNT(*) AS SiparisYetkilisiSayisi
    FROM dbo.BayiYetkilileri byk
    INNER JOIN dbo.KullaniciMagazalari km
        ON km.KullaniciId = byk.KullaniciId
       AND km.MagazaId = byk.MagazaId
       AND km.AktifMi = 1
    INNER JOIN dbo.Kullanicilar ku ON ku.KullaniciId = byk.KullaniciId
    WHERE byk.MagazaId = cs.CustomerStoreId
      AND byk.AktifMi = 1
      AND byk.YetkiTipi = N'SiparisYetkilisi'
      AND ku.AktifMi = 1
) siparisYetkilisi
OUTER APPLY
(
    SELECT
        COUNT(*) AS SiparisSayisi,
        SUM(ISNULL(NULLIF(o.GrandTotal, 0), ISNULL(o.TotalPrice, 0))) AS ToplamCiro,
        MAX(o.OrderDate) AS SonSiparisTarihi
    FROM dbo.Orders o
    WHERE o.CustomerStoreId = cs.CustomerStoreId
      AND ISNULL(o.IsCancelled, 0) = 0
      AND ISNULL(o.IsArchived, 0) = 0
) siparisOzet;
GO

CREATE OR ALTER VIEW dbo.vw_BayiYetkili_Liste
AS
SELECT
    byk.BayiYetkiliId,
    byk.KullaniciId,
    k.KullaniciAdi,
    k.AdSoyad,
    k.Rol,
    CASE
        WHEN k.Rol = N'Admin' THEN N'Admin'
        WHEN k.Rol IN (N'Yonetici', N'StoreManager') THEN N'Yönetici'
        WHEN k.Rol = N'MagazaYetkilisi' THEN N'Mağaza Yetkilisi'
        WHEN k.Rol IN (N'SalesPerson', N'Personel') THEN N'Personel'
        ELSE k.Rol
    END AS RolGorunenAd,
    k.Telefon,
    k.Email,
    k.ImageUrl,
    byk.BayiId,
    ISNULL(NULLIF(c.CompanyName, ''), c.FullName) AS BayiAdi,
    byk.MagazaId,
    cs.StoreName AS MagazaAdi,
    cs.City AS Sehir,
    cs.District AS Ilce,
    byk.YetkiTipi,
    N'Sipariş Yetkilisi' AS YetkiTipiGorunenAd,
    N'Sipariş Yetkilisi' AS Gorev,
    byk.Notlar,
    byk.AktifMi,
    byk.OlusturmaTarihi,
    byk.GuncellemeTarihi,
    ISNULL(siparis.SiparisSayisi, 0) AS SiparisSayisi,
    siparis.SonSiparisTarihi
FROM dbo.BayiYetkilileri byk
INNER JOIN dbo.Kullanicilar k ON k.KullaniciId = byk.KullaniciId
INNER JOIN dbo.Customers c ON c.CustomerId = byk.BayiId
INNER JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = byk.MagazaId
OUTER APPLY
(
    SELECT COUNT(*) AS SiparisSayisi, MAX(o.OrderDate) AS SonSiparisTarihi
    FROM dbo.Orders o
    WHERE o.BayiYetkiliId = byk.BayiYetkiliId
      AND ISNULL(o.IsCancelled, 0) = 0
      AND ISNULL(o.IsArchived, 0) = 0
) siparis;
GO

CREATE OR ALTER PROCEDURE dbo.sp_BayiYetkili_Listele
    @Arama NVARCHAR(200) = '',
    @Durum INT = -1,
    @BayiId INT = NULL,
    @MagazaId INT = NULL,
    @GoruntuleyenKullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.BayiYetkiliId,
        v.KullaniciId,
        v.KullaniciAdi,
        v.AdSoyad,
        v.Rol,
        v.RolGorunenAd,
        v.Telefon,
        v.Email,
        v.ImageUrl,
        v.BayiId,
        v.BayiAdi,
        v.MagazaId,
        v.MagazaAdi,
        v.Sehir,
        v.Ilce,
        v.YetkiTipi,
        v.YetkiTipiGorunenAd,
        v.Gorev,
        v.Notlar,
        v.AktifMi,
        v.OlusturmaTarihi,
        v.GuncellemeTarihi,
        v.SiparisSayisi,
        v.SonSiparisTarihi
    FROM dbo.vw_BayiYetkili_Liste v
    WHERE
        (@BayiId IS NULL OR v.BayiId = @BayiId)
        AND (@MagazaId IS NULL OR v.MagazaId = @MagazaId)
        AND (@Durum = -1 OR v.AktifMi = @Durum)
        AND v.YetkiTipi = N'SiparisYetkilisi'
        AND
        (
            @AdminMi = 1
            OR EXISTS
            (
                SELECT 1
                FROM dbo.KullaniciMagazalari km
                WHERE km.KullaniciId = @GoruntuleyenKullaniciId
                  AND km.MagazaId = v.MagazaId
                  AND km.AktifMi = 1
            )
        )
        AND
        (
            @Arama = ''
            OR v.AdSoyad LIKE '%' + @Arama + '%'
            OR v.KullaniciAdi LIKE '%' + @Arama + '%'
            OR v.BayiAdi LIKE '%' + @Arama + '%'
            OR v.MagazaAdi LIKE '%' + @Arama + '%'
            OR v.Telefon LIKE '%' + @Arama + '%'
            OR v.Email LIKE '%' + @Arama + '%'
            OR v.YetkiTipi LIKE '%' + @Arama + '%'
        )
    ORDER BY v.BayiAdi, v.MagazaAdi, v.AdSoyad;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_BayiYetkili_Getir
    @BayiYetkiliId INT,
    @GoruntuleyenKullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM dbo.vw_BayiYetkili_Liste v
    WHERE v.BayiYetkiliId = @BayiYetkiliId
      AND
      (
            @AdminMi = 1
            OR EXISTS
            (
                SELECT 1
                FROM dbo.KullaniciMagazalari km
                WHERE km.KullaniciId = @GoruntuleyenKullaniciId
                  AND km.MagazaId = v.MagazaId
                  AND km.AktifMi = 1
            )
      );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_BayiYetkili_Kaydet
    @BayiYetkiliId INT = NULL,
    @KullaniciId INT,
    @BayiId INT,
    @MagazaId INT,
    @YetkiTipi NVARCHAR(50) = N'SiparisYetkilisi',
    @Notlar NVARCHAR(500) = NULL,
    @AktifMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SET @YetkiTipi = N'SiparisYetkilisi';

    IF NOT EXISTS (SELECT 1 FROM dbo.Kullanicilar WHERE KullaniciId = @KullaniciId AND AktifMi = 1)
        THROW 52000, 'Aktif bir personel/kullanıcı seçilmelidir.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @BayiId AND IsActive = 1)
        THROW 52001, 'Aktif bayi seçilmelidir.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.CustomerStores WHERE CustomerStoreId = @MagazaId AND CustomerId = @BayiId AND IsActive = 1)
        THROW 52002, 'Seçili mağaza aktif olmalı ve bayiye bağlı olmalıdır.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.KullaniciMagazalari
        WHERE KullaniciId = @KullaniciId
          AND MagazaId = @MagazaId
          AND AktifMi = 1
    )
        THROW 52003, 'Sipariş yetkilisi yapılacak kullanıcı önce seçili mağazaya personel olarak bağlı olmalıdır.', 1;

    IF @BayiYetkiliId IS NULL OR @BayiYetkiliId <= 0
    BEGIN
        SELECT @BayiYetkiliId = BayiYetkiliId
        FROM dbo.BayiYetkilileri
        WHERE KullaniciId = @KullaniciId
          AND BayiId = @BayiId
          AND MagazaId = @MagazaId;

        IF @BayiYetkiliId IS NULL
        BEGIN
            INSERT INTO dbo.BayiYetkilileri (KullaniciId, BayiId, MagazaId, YetkiTipi, Notlar, AktifMi)
            VALUES (@KullaniciId, @BayiId, @MagazaId, N'SiparisYetkilisi', NULLIF(LTRIM(RTRIM(@Notlar)), ''), @AktifMi);

            SET @BayiYetkiliId = CONVERT(INT, SCOPE_IDENTITY());
        END
        ELSE
        BEGIN
            UPDATE dbo.BayiYetkilileri
            SET YetkiTipi = N'SiparisYetkilisi',
                Notlar = NULLIF(LTRIM(RTRIM(@Notlar)), ''),
                AktifMi = @AktifMi,
                GuncellemeTarihi = SYSDATETIME()
            WHERE BayiYetkiliId = @BayiYetkiliId;
        END
    END
    ELSE
    BEGIN
        UPDATE dbo.BayiYetkilileri
        SET KullaniciId = @KullaniciId,
            BayiId = @BayiId,
            MagazaId = @MagazaId,
            YetkiTipi = N'SiparisYetkilisi',
            Notlar = NULLIF(LTRIM(RTRIM(@Notlar)), ''),
            AktifMi = @AktifMi,
            GuncellemeTarihi = SYSDATETIME()
        WHERE BayiYetkiliId = @BayiYetkiliId;
    END

    SELECT @BayiYetkiliId AS BayiYetkiliId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_BayiYetkili_DurumGuncelle
    @BayiYetkiliId INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.BayiYetkilileri
    SET AktifMi = @AktifMi,
        GuncellemeTarihi = SYSDATETIME()
    WHERE BayiYetkiliId = @BayiYetkiliId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Magaza_SiparisYetkilileri_Listele
    @MagazaId INT,
    @GoruntuleyenKullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        v.BayiYetkiliId,
        v.KullaniciId,
        v.KullaniciAdi,
        v.AdSoyad,
        v.Rol,
        v.RolGorunenAd,
        v.Telefon,
        v.Email,
        v.ImageUrl,
        v.BayiId,
        v.BayiAdi,
        v.MagazaId,
        v.MagazaAdi,
        v.YetkiTipi,
        v.YetkiTipiGorunenAd
    FROM dbo.vw_BayiYetkili_Liste v
    INNER JOIN dbo.KullaniciMagazalari km
        ON km.KullaniciId = v.KullaniciId
       AND km.MagazaId = v.MagazaId
       AND km.AktifMi = 1
    WHERE v.MagazaId = @MagazaId
      AND v.AktifMi = 1
      AND v.YetkiTipi = N'SiparisYetkilisi'
      AND (@AdminMi = 1 OR v.KullaniciId = @GoruntuleyenKullaniciId)
      AND
      (
            @AdminMi = 1
            OR EXISTS
            (
                SELECT 1
                FROM dbo.KullaniciMagazalari yetki
                WHERE yetki.KullaniciId = @GoruntuleyenKullaniciId
                  AND yetki.MagazaId = @MagazaId
                  AND yetki.AktifMi = 1
            )
      )
    ORDER BY v.AdSoyad;
END
GO
