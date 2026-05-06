IF COL_LENGTH('dbo.CustomerStores', 'SorumluKullaniciId') IS NULL
BEGIN
    ALTER TABLE dbo.CustomerStores
    ADD SorumluKullaniciId INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_CustomerStores_SorumluKullanici'
)
BEGIN
    ALTER TABLE dbo.CustomerStores
    ADD CONSTRAINT FK_CustomerStores_SorumluKullanici
        FOREIGN KEY (SorumluKullaniciId)
        REFERENCES dbo.Kullanicilar(KullaniciId);
END
GO

IF OBJECT_ID('dbo.BayiYetkilileri', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BayiYetkilileri
    (
        BayiYetkiliId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BayiYetkilileri PRIMARY KEY,
        BayiId INT NOT NULL,
        MagazaId INT NULL,
        AdSoyad NVARCHAR(200) NOT NULL,
        Telefon NVARCHAR(60) NULL,
        Email NVARCHAR(400) NULL,
        Gorev NVARCHAR(100) NULL,
        Notlar NVARCHAR(500) NULL,
        AktifMi BIT NOT NULL CONSTRAINT DF_BayiYetkilileri_AktifMi DEFAULT (1),
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_BayiYetkilileri_OlusturmaTarihi DEFAULT (SYSDATETIME()),
        GuncellemeTarihi DATETIME2 NOT NULL CONSTRAINT DF_BayiYetkilileri_GuncellemeTarihi DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_BayiYetkilileri_Customers FOREIGN KEY (BayiId)
            REFERENCES dbo.Customers(CustomerId),
        CONSTRAINT FK_BayiYetkilileri_CustomerStores FOREIGN KEY (MagazaId)
            REFERENCES dbo.CustomerStores(CustomerStoreId)
    );
END
GO

IF COL_LENGTH('dbo.Orders', 'BayiYetkiliId') IS NULL
BEGIN
    ALTER TABLE dbo.Orders
    ADD BayiYetkiliId INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Orders_BayiYetkilileri'
)
BEGIN
    ALTER TABLE dbo.Orders
    ADD CONSTRAINT FK_Orders_BayiYetkilileri
        FOREIGN KEY (BayiYetkiliId)
        REFERENCES dbo.BayiYetkilileri(BayiYetkiliId);
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_BayiYetkilileri_Bayi'
      AND object_id = OBJECT_ID('dbo.BayiYetkilileri')
)
BEGIN
    CREATE INDEX IX_BayiYetkilileri_Bayi
    ON dbo.BayiYetkilileri(BayiId, MagazaId, AktifMi);
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
    COALESCE(NULLIF(k.AdSoyad, ''), NULLIF(cs.ResponsiblePerson, '')) AS SorumluKisi,
    cs.SorumluKullaniciId,

    c.CustomerType AS MusteriTipi,

    c.IsActive AS MusteriAktifMi,
    cs.IsActive AS MagazaAktifMi,

    ISNULL(siparisOzet.SiparisSayisi, 0) AS SiparisSayisi,
    ISNULL(siparisOzet.ToplamCiro, 0) AS ToplamCiro,
    siparisOzet.SonSiparisTarihi
FROM dbo.CustomerStores cs
INNER JOIN dbo.Customers c
    ON c.CustomerId = cs.CustomerId
LEFT JOIN dbo.Kullanicilar k
    ON k.KullaniciId = cs.SorumluKullaniciId
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
    byk.BayiId,
    ISNULL(NULLIF(c.CompanyName, ''), c.FullName) AS BayiAdi,
    byk.MagazaId,
    cs.StoreName AS MagazaAdi,
    cs.City AS Sehir,
    cs.District AS Ilce,
    byk.AdSoyad,
    byk.Telefon,
    byk.Email,
    byk.Gorev,
    byk.Notlar,
    byk.AktifMi,
    byk.OlusturmaTarihi,
    byk.GuncellemeTarihi,
    ISNULL(siparis.SiparisSayisi, 0) AS SiparisSayisi,
    siparis.SonSiparisTarihi
FROM dbo.BayiYetkilileri byk
INNER JOIN dbo.Customers c
    ON c.CustomerId = byk.BayiId
LEFT JOIN dbo.CustomerStores cs
    ON cs.CustomerStoreId = byk.MagazaId
OUTER APPLY
(
    SELECT
        COUNT(*) AS SiparisSayisi,
        MAX(o.OrderDate) AS SonSiparisTarihi
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
    @MagazaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        BayiYetkiliId,
        BayiId,
        BayiAdi,
        MagazaId,
        MagazaAdi,
        Sehir,
        Ilce,
        AdSoyad,
        Telefon,
        Email,
        Gorev,
        Notlar,
        AktifMi,
        OlusturmaTarihi,
        GuncellemeTarihi,
        SiparisSayisi,
        SonSiparisTarihi
    FROM dbo.vw_BayiYetkili_Liste
    WHERE
        (@BayiId IS NULL OR BayiId = @BayiId)
        AND (@MagazaId IS NULL OR MagazaId = @MagazaId OR MagazaId IS NULL)
        AND (@Durum = -1 OR AktifMi = @Durum)
        AND
        (
            @Arama = ''
            OR AdSoyad LIKE '%' + @Arama + '%'
            OR BayiAdi LIKE '%' + @Arama + '%'
            OR MagazaAdi LIKE '%' + @Arama + '%'
            OR Telefon LIKE '%' + @Arama + '%'
            OR Email LIKE '%' + @Arama + '%'
            OR Gorev LIKE '%' + @Arama + '%'
        )
    ORDER BY BayiAdi, AdSoyad;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_BayiYetkili_Getir
    @BayiYetkiliId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        BayiYetkiliId,
        BayiId,
        BayiAdi,
        MagazaId,
        MagazaAdi,
        Sehir,
        Ilce,
        AdSoyad,
        Telefon,
        Email,
        Gorev,
        Notlar,
        AktifMi,
        OlusturmaTarihi,
        GuncellemeTarihi,
        SiparisSayisi,
        SonSiparisTarihi
    FROM dbo.vw_BayiYetkili_Liste
    WHERE BayiYetkiliId = @BayiYetkiliId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_BayiYetkili_Kaydet
    @BayiYetkiliId INT = NULL,
    @BayiId INT,
    @MagazaId INT = NULL,
    @AdSoyad NVARCHAR(200),
    @Telefon NVARCHAR(60) = NULL,
    @Email NVARCHAR(400) = NULL,
    @Gorev NVARCHAR(100) = NULL,
    @Notlar NVARCHAR(500) = NULL,
    @AktifMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF @BayiId IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.Customers WHERE CustomerId = @BayiId AND IsActive = 1)
    BEGIN
        RAISERROR('Aktif bayi seçilmelidir.', 16, 1);
        RETURN;
    END

    IF @MagazaId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CustomerStores WHERE CustomerStoreId = @MagazaId AND CustomerId = @BayiId)
    BEGIN
        RAISERROR('Seçili mağaza bayiye bağlı değildir.', 16, 1);
        RETURN;
    END

    IF NULLIF(LTRIM(RTRIM(@AdSoyad)), '') IS NULL
    BEGIN
        RAISERROR('Ad soyad zorunludur.', 16, 1);
        RETURN;
    END

    IF @BayiYetkiliId IS NULL OR @BayiYetkiliId <= 0
    BEGIN
        INSERT INTO dbo.BayiYetkilileri
        (
            BayiId,
            MagazaId,
            AdSoyad,
            Telefon,
            Email,
            Gorev,
            Notlar,
            AktifMi
        )
        VALUES
        (
            @BayiId,
            @MagazaId,
            LTRIM(RTRIM(@AdSoyad)),
            NULLIF(LTRIM(RTRIM(@Telefon)), ''),
            NULLIF(LTRIM(RTRIM(@Email)), ''),
            NULLIF(LTRIM(RTRIM(@Gorev)), ''),
            NULLIF(LTRIM(RTRIM(@Notlar)), ''),
            @AktifMi
        );

        SELECT SCOPE_IDENTITY();
        RETURN;
    END

    UPDATE dbo.BayiYetkilileri
    SET
        BayiId = @BayiId,
        MagazaId = @MagazaId,
        AdSoyad = LTRIM(RTRIM(@AdSoyad)),
        Telefon = NULLIF(LTRIM(RTRIM(@Telefon)), ''),
        Email = NULLIF(LTRIM(RTRIM(@Email)), ''),
        Gorev = NULLIF(LTRIM(RTRIM(@Gorev)), ''),
        Notlar = NULLIF(LTRIM(RTRIM(@Notlar)), ''),
        AktifMi = @AktifMi,
        GuncellemeTarihi = SYSDATETIME()
    WHERE BayiYetkiliId = @BayiYetkiliId;

    SELECT @BayiYetkiliId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_BayiYetkili_DurumGuncelle
    @BayiYetkiliId INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.BayiYetkilileri
    SET
        AktifMi = @AktifMi,
        GuncellemeTarihi = SYSDATETIME()
    WHERE BayiYetkiliId = @BayiYetkiliId;
END
GO
