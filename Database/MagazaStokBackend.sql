IF OBJECT_ID('dbo.MagazaStoklari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MagazaStoklari
    (
        MagazaStokId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MagazaStoklari PRIMARY KEY,
        MagazaId INT NOT NULL,
        ProductId INT NOT NULL,
        StokAdedi INT NOT NULL CONSTRAINT DF_MagazaStoklari_StokAdedi DEFAULT (0),
        MinimumStok INT NOT NULL CONSTRAINT DF_MagazaStoklari_MinimumStok DEFAULT (0),
        AktifMi BIT NOT NULL CONSTRAINT DF_MagazaStoklari_AktifMi DEFAULT (1),
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_MagazaStoklari_OlusturmaTarihi DEFAULT (SYSDATETIME()),
        GuncellemeTarihi DATETIME2 NOT NULL CONSTRAINT DF_MagazaStoklari_GuncellemeTarihi DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_MagazaStoklari_Magaza_Urun UNIQUE (MagazaId, ProductId),
        CONSTRAINT CK_MagazaStoklari_StokAdedi CHECK (StokAdedi >= 0),
        CONSTRAINT CK_MagazaStoklari_MinimumStok CHECK (MinimumStok >= 0),
        CONSTRAINT FK_MagazaStoklari_CustomerStores FOREIGN KEY (MagazaId)
            REFERENCES dbo.CustomerStores(CustomerStoreId),
        CONSTRAINT FK_MagazaStoklari_Products FOREIGN KEY (ProductId)
            REFERENCES dbo.Products(ProductId)
    );
END
GO

IF OBJECT_ID('dbo.MagazaStokHareketleri', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MagazaStokHareketleri
    (
        MagazaStokHareketId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MagazaStokHareketleri PRIMARY KEY,
        MagazaId INT NOT NULL,
        ProductId INT NOT NULL,
        HareketTipi NVARCHAR(50) NOT NULL,
        Miktar INT NOT NULL,
        OncekiStok INT NOT NULL,
        SonrakiStok INT NOT NULL,
        KaynakSiparisId INT NULL,
        KaynakSiparisKalemId INT NULL,
        Aciklama NVARCHAR(500) NULL,
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_MagazaStokHareketleri_OlusturmaTarihi DEFAULT (SYSDATETIME()),
        CONSTRAINT CK_MagazaStokHareketleri_Miktar CHECK (Miktar > 0),
        CONSTRAINT FK_MagazaStokHareketleri_CustomerStores FOREIGN KEY (MagazaId)
            REFERENCES dbo.CustomerStores(CustomerStoreId),
        CONSTRAINT FK_MagazaStokHareketleri_Products FOREIGN KEY (ProductId)
            REFERENCES dbo.Products(ProductId),
        CONSTRAINT FK_MagazaStokHareketleri_Orders FOREIGN KEY (KaynakSiparisId)
            REFERENCES dbo.Orders(OrderId),
        CONSTRAINT FK_MagazaStokHareketleri_OrderItems FOREIGN KEY (KaynakSiparisKalemId)
            REFERENCES dbo.OrderItems(OrderItemId)
    );
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_MagazaStokHareketleri_TeslimKalem'
      AND object_id = OBJECT_ID('dbo.MagazaStokHareketleri')
)
BEGIN
    CREATE UNIQUE INDEX UX_MagazaStokHareketleri_TeslimKalem
    ON dbo.MagazaStokHareketleri(KaynakSiparisKalemId, HareketTipi)
    WHERE KaynakSiparisKalemId IS NOT NULL
      AND HareketTipi = N'SiparisTeslimGiris';
END
GO

CREATE OR ALTER VIEW dbo.vw_MagazaStok_Liste
AS
SELECT
    ms.MagazaStokId,
    ms.MagazaId,
    cs.CustomerId AS MusteriId,
    ISNULL(NULLIF(c.CompanyName, ''), c.FullName) AS MusteriAdi,
    cs.StoreName AS MagazaAdi,
    cs.City AS Sehir,
    cs.District AS Ilce,
    cs.Phone AS Telefon,
    COALESCE(NULLIF(k.AdSoyad, ''), NULLIF(cs.ResponsiblePerson, '')) AS SorumluKisi,
    ms.ProductId AS UrunId,
    p.ProductName AS UrunAdi,
    p.Description AS Aciklama,
    p.Price AS Fiyat,
    p.Stock AS MerkezStok,
    p.ImageUrl AS GorselUrl,
    p.CategoryId AS KategoriId,
    cat.CategoryName AS KategoriAdi,
    ms.StokAdedi AS BayiStok,
    ms.MinimumStok,
    CASE
        WHEN ms.StokAdedi <= 0 THEN N'Tukendi'
        WHEN ms.MinimumStok > 0 AND ms.StokAdedi <= ms.MinimumStok THEN N'Kritik'
        ELSE N'Yeterli'
    END AS StokDurumu,
    ms.AktifMi,
    ms.OlusturmaTarihi,
    ms.GuncellemeTarihi,
    hareket.SonHareketTarihi,
    hareket.SonGirisTarihi,
    hareket.SonCikisTarihi
FROM dbo.MagazaStoklari ms
INNER JOIN dbo.CustomerStores cs
    ON cs.CustomerStoreId = ms.MagazaId
INNER JOIN dbo.Customers c
    ON c.CustomerId = cs.CustomerId
INNER JOIN dbo.Products p
    ON p.ProductId = ms.ProductId
LEFT JOIN dbo.Categories cat
    ON cat.CategoryId = p.CategoryId
LEFT JOIN dbo.Kullanicilar k
    ON k.KullaniciId = cs.SorumluKullaniciId
OUTER APPLY
(
    SELECT
        MAX(msh.OlusturmaTarihi) AS SonHareketTarihi,
        MAX(CASE WHEN msh.HareketTipi IN (N'SiparisTeslimGiris', N'ManuelGiris', N'DuzeltmeGiris') THEN msh.OlusturmaTarihi END) AS SonGirisTarihi,
        MAX(CASE WHEN msh.HareketTipi IN (N'BayiSatisCikis', N'ManuelCikis', N'DuzeltmeCikis') THEN msh.OlusturmaTarihi END) AS SonCikisTarihi
    FROM dbo.MagazaStokHareketleri msh
    WHERE msh.MagazaId = ms.MagazaId
      AND msh.ProductId = ms.ProductId
) hareket;
GO

CREATE OR ALTER PROCEDURE dbo.sp_MagazaStok_Hareket_Isle
    @MagazaId INT,
    @ProductId INT,
    @HareketTipi NVARCHAR(50),
    @Miktar INT,
    @KaynakSiparisId INT = NULL,
    @KaynakSiparisKalemId INT = NULL,
    @Aciklama NVARCHAR(500) = NULL,
    @MinimumStok INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OncekiStok INT;
    DECLARE @SonrakiStok INT;
    DECLARE @Yon INT;

    IF @Miktar <= 0
    BEGIN
        RAISERROR('Stok hareket miktarı sıfırdan büyük olmalıdır.', 16, 1);
        RETURN;
    END

    IF @HareketTipi IN (N'BayiSatisCikis', N'ManuelCikis', N'DuzeltmeCikis')
        SET @Yon = -1;
    ELSE
        SET @Yon = 1;

    IF @HareketTipi = N'SiparisTeslimGiris'
       AND @KaynakSiparisKalemId IS NOT NULL
       AND EXISTS
       (
            SELECT 1
            FROM dbo.MagazaStokHareketleri
            WHERE KaynakSiparisKalemId = @KaynakSiparisKalemId
              AND HareketTipi = N'SiparisTeslimGiris'
       )
    BEGIN
        RETURN;
    END

    BEGIN TRANSACTION;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.MagazaStoklari WITH (UPDLOCK, HOLDLOCK)
        WHERE MagazaId = @MagazaId
          AND ProductId = @ProductId
    )
    BEGIN
        INSERT INTO dbo.MagazaStoklari
        (
            MagazaId,
            ProductId,
            StokAdedi,
            MinimumStok,
            AktifMi
        )
        VALUES
        (
            @MagazaId,
            @ProductId,
            0,
            ISNULL(@MinimumStok, 0),
            1
        );
    END

    SELECT @OncekiStok = StokAdedi
    FROM dbo.MagazaStoklari WITH (UPDLOCK, HOLDLOCK)
    WHERE MagazaId = @MagazaId
      AND ProductId = @ProductId;

    SET @SonrakiStok = @OncekiStok + (@Miktar * @Yon);

    IF @SonrakiStok < 0
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Bayi stoğu eksiye düşemez.', 16, 1);
        RETURN;
    END

    UPDATE dbo.MagazaStoklari
    SET
        StokAdedi = @SonrakiStok,
        MinimumStok = CASE WHEN @MinimumStok IS NULL THEN MinimumStok ELSE @MinimumStok END,
        AktifMi = 1,
        GuncellemeTarihi = SYSDATETIME()
    WHERE MagazaId = @MagazaId
      AND ProductId = @ProductId;

    INSERT INTO dbo.MagazaStokHareketleri
    (
        MagazaId,
        ProductId,
        HareketTipi,
        Miktar,
        OncekiStok,
        SonrakiStok,
        KaynakSiparisId,
        KaynakSiparisKalemId,
        Aciklama
    )
    VALUES
    (
        @MagazaId,
        @ProductId,
        @HareketTipi,
        @Miktar,
        @OncekiStok,
        @SonrakiStok,
        @KaynakSiparisId,
        @KaynakSiparisKalemId,
        @Aciklama
    );

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Siparis_BayiStok_TeslimIsle
    @SiparisId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MagazaId INT;

    SELECT @MagazaId = CustomerStoreId
    FROM dbo.Orders
    WHERE OrderId = @SiparisId
      AND ISNULL(IsCancelled, 0) = 0;

    IF @MagazaId IS NULL
        RETURN;

    DECLARE @OrderItemId INT;
    DECLARE @ProductId INT;
    DECLARE @Quantity INT;

    DECLARE stok_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT
            oi.OrderItemId,
            oi.ProductId,
            oi.Quantity
        FROM dbo.OrderItems oi
        WHERE oi.OrderId = @SiparisId
          AND oi.Quantity > 0;

    OPEN stok_cursor;
    FETCH NEXT FROM stok_cursor INTO @OrderItemId, @ProductId, @Quantity;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.sp_MagazaStok_Hareket_Isle
            @MagazaId = @MagazaId,
            @ProductId = @ProductId,
            @HareketTipi = N'SiparisTeslimGiris',
            @Miktar = @Quantity,
            @KaynakSiparisId = @SiparisId,
            @KaynakSiparisKalemId = @OrderItemId,
            @Aciklama = N'Sipariş teslim edildi, bayi stoğuna giriş yapıldı.';

        FETCH NEXT FROM stok_cursor INTO @OrderItemId, @ProductId, @Quantity;
    END

    CLOSE stok_cursor;
    DEALLOCATE stok_cursor;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Siparis_Durum_Guncelle
    @SiparisId INT,
    @SiparisDurumu NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @OncekiDurum NVARCHAR(50);

    SELECT @OncekiDurum = OrderStatus
    FROM dbo.Orders
    WHERE OrderId = @SiparisId;

    IF @OncekiDurum IS NULL
    BEGIN
        RAISERROR('Sipariş bulunamadı.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Orders
    SET
        OrderStatus = @SiparisDurumu,
        IsCancelled = CASE WHEN @SiparisDurumu = N'Iptal' THEN 1 ELSE IsCancelled END
    WHERE OrderId = @SiparisId;

    IF @SiparisDurumu = N'Teslim Edildi'
       AND ISNULL(@OncekiDurum, '') <> N'Teslim Edildi'
    BEGIN
        EXEC dbo.sp_Siparis_BayiStok_TeslimIsle @SiparisId = @SiparisId;
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MagazaStok_Listele
    @MagazaId INT = NULL,
    @Arama NVARCHAR(200) = '',
    @SadeceStokta BIT = 0,
    @SadeceKritik BIT = 0,
    @SadeceAktif BIT = 1,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MagazaStokId,
        MagazaId,
        MusteriId,
        MusteriAdi,
        MagazaAdi,
        Sehir,
        Ilce,
        Telefon,
        SorumluKisi,
        UrunId,
        UrunAdi,
        Aciklama,
        Fiyat,
        MerkezStok,
        GorselUrl,
        KategoriId,
        KategoriAdi,
        BayiStok,
        MinimumStok,
        StokDurumu,
        AktifMi,
        OlusturmaTarihi,
        GuncellemeTarihi,
        SonHareketTarihi,
        SonGirisTarihi,
        SonCikisTarihi
    FROM dbo.vw_MagazaStok_Liste
    WHERE
        (@MagazaId IS NULL OR MagazaId = @MagazaId)
        AND
        (
            @Arama = ''
            OR MusteriAdi LIKE '%' + @Arama + '%'
            OR MagazaAdi LIKE '%' + @Arama + '%'
            OR UrunAdi LIKE '%' + @Arama + '%'
            OR KategoriAdi LIKE '%' + @Arama + '%'
        )
        AND (@SadeceStokta = 0 OR BayiStok > 0)
        AND (@SadeceKritik = 0 OR StokDurumu IN (N'Tukendi', N'Kritik'))
        AND (@SadeceAktif = 0 OR AktifMi = 1)
        AND
        (
            @AdminMi = 1
            OR
            (
                @KullaniciId IS NOT NULL
                AND EXISTS
                (
                    SELECT 1
                    FROM dbo.KullaniciMagazalari km
                    WHERE km.MagazaId = vw_MagazaStok_Liste.MagazaId
                      AND km.KullaniciId = @KullaniciId
                      AND km.AktifMi = 1
                )
            )
        )
    ORDER BY MusteriAdi, MagazaAdi, UrunAdi;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MagazaStok_Ozet_Getir
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 1,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS ToplamStokKarti,
        COUNT(DISTINCT MagazaId) AS StokluMagazaSayisi,
        COUNT(DISTINCT UrunId) AS StokluUrunSayisi,
        ISNULL(SUM(BayiStok), 0) AS ToplamBayiStok,
        SUM(CASE WHEN StokDurumu = N'Kritik' THEN 1 ELSE 0 END) AS KritikStokKarti,
        SUM(CASE WHEN StokDurumu = N'Tukendi' THEN 1 ELSE 0 END) AS TukenmisStokKarti
    FROM dbo.vw_MagazaStok_Liste
    WHERE
        (@TumMagazalar = 1 OR MagazaId = @MagazaId)
        AND AktifMi = 1
        AND
        (
            @AdminMi = 1
            OR
            (
                @KullaniciId IS NOT NULL
                AND EXISTS
                (
                    SELECT 1
                    FROM dbo.KullaniciMagazalari km
                    WHERE km.MagazaId = vw_MagazaStok_Liste.MagazaId
                      AND km.KullaniciId = @KullaniciId
                      AND km.AktifMi = 1
                )
            )
        );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MagazaStok_TeslimEdilmisSiparisleriIsle
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SiparisId INT;

    DECLARE teslim_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT o.OrderId
        FROM dbo.Orders o
        WHERE o.OrderStatus = N'Teslim Edildi'
          AND o.CustomerStoreId IS NOT NULL
          AND ISNULL(o.IsCancelled, 0) = 0;

    OPEN teslim_cursor;
    FETCH NEXT FROM teslim_cursor INTO @SiparisId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.sp_Siparis_BayiStok_TeslimIsle @SiparisId = @SiparisId;
        FETCH NEXT FROM teslim_cursor INTO @SiparisId;
    END

    CLOSE teslim_cursor;
    DEALLOCATE teslim_cursor;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MagazaStok_MinimumGuncelle
    @MagazaStokId INT,
    @MinimumStok INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @MinimumStok < 0
    BEGIN
        RAISERROR('Minimum stok negatif olamaz.', 16, 1);
        RETURN;
    END

    UPDATE dbo.MagazaStoklari
    SET
        MinimumStok = @MinimumStok,
        GuncellemeTarihi = SYSDATETIME()
    WHERE MagazaStokId = @MagazaStokId;
END
GO
