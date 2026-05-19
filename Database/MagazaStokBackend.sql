SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

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

IF OBJECT_ID('dbo.MagazaStokHareketleri', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.MagazaStokHareketleri
    SET Aciklama = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Aciklama,
        N'Ãœ', N'Ü'), N'Ã¼', N'ü'), N'Ã¶', N'ö'), N'Ã§', N'ç'), N'Ã‡', N'Ç'), N'Ä±', N'ı'),
        N'Ä°', N'İ'), N'ÄŸ', N'ğ'), N'Äž', N'Ğ'), N'ÅŸ', N'ş'), N'Åž', N'Ş'), N'Ã–', N'Ö')
    WHERE Aciklama IS NOT NULL
      AND (Aciklama LIKE N'%Ã%' OR Aciklama LIKE N'%Ä%' OR Aciklama LIKE N'%Å%');
END
GO

IF OBJECT_ID('dbo.StockMovements', 'U') IS NOT NULL
BEGIN
    UPDATE dbo.StockMovements
    SET Description = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(Description,
        N'Ãœ', N'Ü'), N'Ã¼', N'ü'), N'Ã¶', N'ö'), N'Ã§', N'ç'), N'Ã‡', N'Ç'), N'Ä±', N'ı'),
        N'Ä°', N'İ'), N'ÄŸ', N'ğ'), N'Äž', N'Ğ'), N'ÅŸ', N'ş'), N'Åž', N'Ş'), N'Ã–', N'Ö')
    WHERE Description IS NOT NULL
      AND (Description LIKE N'%Ã%' OR Description LIKE N'%Ä%' OR Description LIKE N'%Å%');
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
    ISNULL(toplam.ToplamBayiStok, 0) AS ToplamBayiStok,
    ISNULL(toplam.KritikBayiSayisi, 0) AS KritikBayiSayisi,
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
) hareket
OUTER APPLY
(
    SELECT
        SUM(ms2.StokAdedi) AS ToplamBayiStok,
        SUM(CASE WHEN ms2.MinimumStok > 0 AND ms2.StokAdedi <= ms2.MinimumStok THEN 1 ELSE 0 END) AS KritikBayiSayisi
    FROM dbo.MagazaStoklari ms2
    WHERE ms2.ProductId = ms.ProductId
      AND ms2.AktifMi = 1
) toplam;
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
    DECLARE @MerkezStok INT;
    DECLARE @Yon INT;

    IF @Miktar <= 0
    BEGIN
        RAISERROR('Stok hareket miktarı sıfırdan büyük olmalıdır.', 16, 1);
        RETURN;
    END

    IF @HareketTipi IN (N'BayiSatisCikis', N'ManuelCikis', N'DuzeltmeCikis', N'SiparisIptalCikis')
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

    IF @HareketTipi IN (N'ManuelGiris', N'DuzeltmeGiris')
    BEGIN
        SELECT @MerkezStok = Stock
        FROM dbo.Products WITH (UPDLOCK, HOLDLOCK)
        WHERE ProductId = @ProductId
          AND IsActive = 1;

        IF @MerkezStok IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Merkez stokta ürün bulunamadı veya aktif değil.', 16, 1);
            RETURN;
        END

        IF @MerkezStok < @Miktar
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Merkez stok bayi transferi için yeterli değil.', 16, 1);
            RETURN;
        END
    END

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

    IF @HareketTipi IN (N'ManuelGiris', N'DuzeltmeGiris')
    BEGIN
        UPDATE dbo.Products
        SET Stock = Stock - @Miktar
        WHERE ProductId = @ProductId;

        INSERT INTO dbo.StockMovements
        (
            ProductId,
            MovementType,
            Quantity,
            Description
        )
        VALUES
        (
            @ProductId,
            N'MagazaTransferOut',
            @Miktar,
            ISNULL(@Aciklama, N'Merkezden bayi stoğuna transfer')
        );
    END

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

CREATE OR ALTER PROCEDURE dbo.sp_MerkezStok_Artir
    @ProductId INT,
    @Miktar INT,
    @Aciklama NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @ProductId IS NULL OR @ProductId <= 0
    BEGIN
        RAISERROR('Ürün seçimi zorunludur.', 16, 1);
        RETURN;
    END;

    IF @Miktar IS NULL OR @Miktar <= 0
    BEGIN
        RAISERROR('Merkez stok hareket miktarı sıfırdan büyük olmalıdır.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM dbo.Products WHERE ProductId = @ProductId AND IsActive = 1)
    BEGIN
        RAISERROR('Merkez stokta ürün bulunamadı veya aktif değil.', 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;

    UPDATE dbo.Products
    SET Stock = Stock + @Miktar
    WHERE ProductId = @ProductId
      AND IsActive = 1;

    INSERT INTO dbo.StockMovements
    (
        ProductId,
        OrderId,
        MovementType,
        Quantity,
        Description
    )
    VALUES
    (
        @ProductId,
        NULL,
        N'MerkezGiris',
        @Miktar,
        ISNULL(@Aciklama, N'Admin panel merkez stok artırma')
    );

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MerkezStok_Hareket_Listele
    @ProductId INT,
    @KayitSayisi INT = 25,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @KayitSayisi <= 0
        SET @KayitSayisi = 25;

    IF @KayitSayisi > 100
        SET @KayitSayisi = 100;

    IF @AdminMi = 0
    BEGIN
        RAISERROR('Merkez stok hareketlerini görüntüleme yetkiniz yok.', 16, 1);
        RETURN;
    END

    SELECT TOP (@KayitSayisi)
        0 AS MagazaStokHareketId,
        0 AS MagazaId,
        sm.ProductId AS UrunId,
        p.ProductName AS UrunAdi,
        sm.MovementType AS HareketTipi,
        CASE
            WHEN sm.MovementType IN (N'OrderOut', N'SiparisOlusturma', N'MerkezCikis') THEN N'Çıkış'
            ELSE N'Giriş'
        END AS HareketYonu,
        CASE
            WHEN sm.MovementType = N'MerkezGiris' THEN N'Merkez stok girişi'
            WHEN sm.MovementType = N'OrderOut' THEN N'Sipariş merkez çıkışı'
            WHEN sm.MovementType = N'SiparisIptalIade' THEN N'Sipariş iptal/iade girişi'
            ELSE sm.MovementType
        END AS HareketAciklama,
        sm.Quantity AS Miktar,
        0 AS OncekiStok,
        0 AS SonrakiStok,
        sm.OrderId AS KaynakSiparisId,
        o.OrderNo AS SiparisNo,
        sm.Description AS Aciklama,
        sm.CreatedAt AS OlusturmaTarihi
    FROM dbo.StockMovements sm
    INNER JOIN dbo.Products p
        ON p.ProductId = sm.ProductId
    LEFT JOIN dbo.Orders o
        ON o.OrderId = sm.OrderId
    WHERE sm.ProductId = @ProductId
    ORDER BY sm.CreatedAt DESC;
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

CREATE OR ALTER PROCEDURE dbo.sp_Siparis_IptalStokIade_Isle
    @SiparisId INT,
    @TeslimEdildiMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MagazaId INT;

    SELECT @MagazaId = CustomerStoreId
    FROM dbo.Orders
    WHERE OrderId = @SiparisId;

    IF @TeslimEdildiMi = 1 AND @MagazaId IS NULL
    BEGIN
        RAISERROR('Teslim edilmiş sipariş için bayi/mağaza bilgisi bulunamadı.', 16, 1);
        RETURN;
    END

    DECLARE @OrderItemId INT;
    DECLARE @ProductId INT;
    DECLARE @Quantity INT;
    DECLARE @OncekiBayiStok INT;
    DECLARE @SonrakiBayiStok INT;

    DECLARE iade_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT
            oi.OrderItemId,
            oi.ProductId,
            oi.Quantity
        FROM dbo.OrderItems oi
        WHERE oi.OrderId = @SiparisId
          AND oi.Quantity > 0;

    OPEN iade_cursor;
    FETCH NEXT FROM iade_cursor INTO @OrderItemId, @ProductId, @Quantity;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @TeslimEdildiMi = 1
        BEGIN
            SELECT @OncekiBayiStok = StokAdedi
            FROM dbo.MagazaStoklari WITH (UPDLOCK, HOLDLOCK)
            WHERE MagazaId = @MagazaId
              AND ProductId = @ProductId
              AND AktifMi = 1;

            IF @OncekiBayiStok IS NULL OR @OncekiBayiStok < @Quantity
            BEGIN
                CLOSE iade_cursor;
                DEALLOCATE iade_cursor;
                RAISERROR('Bayi stoğu sipariş iptali için yeterli değil. Önce bayi stoklarını kontrol edin.', 16, 1);
                RETURN;
            END

            SET @SonrakiBayiStok = @OncekiBayiStok - @Quantity;

            UPDATE dbo.MagazaStoklari
            SET
                StokAdedi = @SonrakiBayiStok,
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
                N'SiparisIptalCikis',
                @Quantity,
                @OncekiBayiStok,
                @SonrakiBayiStok,
                @SiparisId,
                @OrderItemId,
                N'Teslim edilen sipariş iptal edildi, bayi stoğundan merkeze iade edildi.'
            );
        END

        UPDATE dbo.Products
        SET Stock = Stock + @Quantity
        WHERE ProductId = @ProductId;

        INSERT INTO dbo.StockMovements
        (
            ProductId,
            OrderId,
            MovementType,
            Quantity,
            Description
        )
        VALUES
        (
            @ProductId,
            @SiparisId,
            CASE WHEN @TeslimEdildiMi = 1 THEN N'DeliveredOrderCancelReturn' ELSE N'OrderCancelIn' END,
            @Quantity,
            CASE
                WHEN @TeslimEdildiMi = 1 THEN N'Teslim edilmiş sipariş iptal edildi, ürün merkez stoğa iade edildi.'
                ELSE N'Sipariş iptal edildi, ayrılan ürün merkez stoğa iade edildi.'
            END
        );

        FETCH NEXT FROM iade_cursor INTO @OrderItemId, @ProductId, @Quantity;
    END

    CLOSE iade_cursor;
    DEALLOCATE iade_cursor;
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
    DECLARE @IptalMi BIT;
    DECLARE @TeslimEdildiMi BIT;

    SELECT
        @OncekiDurum = OrderStatus,
        @IptalMi = ISNULL(IsCancelled, 0)
    FROM dbo.Orders
    WHERE OrderId = @SiparisId;

    IF @OncekiDurum IS NULL
    BEGIN
        RAISERROR('Sipariş bulunamadı.', 16, 1);
        RETURN;
    END

    IF @SiparisDurumu NOT IN (N'Hazirlaniyor', N'Kargoda', N'Teslim Edildi', N'Iptal')
    BEGIN
        RAISERROR('Geçersiz sipariş durumu.', 16, 1);
        RETURN;
    END

    IF @IptalMi = 1 OR @OncekiDurum = N'Iptal'
    BEGIN
        IF @SiparisDurumu = N'Iptal'
            RETURN;

        RAISERROR('İptal edilen sipariş tekrar işleme alınamaz.', 16, 1);
        RETURN;
    END

    IF @OncekiDurum = @SiparisDurumu
        RETURN;

    IF @OncekiDurum = N'Teslim Edildi' AND @SiparisDurumu <> N'Iptal'
    BEGIN
        RAISERROR('Teslim edilen sipariş yalnızca iptal/iade sürecine alınabilir.', 16, 1);
        RETURN;
    END

    IF @OncekiDurum = N'Kargoda' AND @SiparisDurumu = N'Hazirlaniyor'
    BEGIN
        RAISERROR('Kargodaki sipariş hazırlık durumuna geri alınamaz.', 16, 1);
        RETURN;
    END

    SET @TeslimEdildiMi = CASE WHEN @OncekiDurum = N'Teslim Edildi' THEN 1 ELSE 0 END;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @SiparisDurumu = N'Iptal'
        BEGIN
            EXEC dbo.sp_Siparis_IptalStokIade_Isle
                @SiparisId = @SiparisId,
                @TeslimEdildiMi = @TeslimEdildiMi;

            UPDATE dbo.Orders
            SET
                OrderStatus = N'Iptal',
                IsCancelled = 1
            WHERE OrderId = @SiparisId;
        END
        ELSE
        BEGIN
            UPDATE dbo.Orders
            SET OrderStatus = @SiparisDurumu
            WHERE OrderId = @SiparisId;

            IF @SiparisDurumu = N'Teslim Edildi'
            BEGIN
                EXEC dbo.sp_Siparis_BayiStok_TeslimIsle @SiparisId = @SiparisId;
            END
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;

        SELECT
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Siparis_IptalEt
    @SiparisId INT
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.sp_Siparis_Durum_Guncelle
        @SiparisId = @SiparisId,
        @SiparisDurumu = N'Iptal';
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
        ToplamBayiStok,
        KritikBayiSayisi,
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
    @TumMagazalar BIT = 0,
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

CREATE OR ALTER PROCEDURE dbo.sp_MagazaStok_Hareket_Listele
    @MagazaId INT,
    @ProductId INT = NULL,
    @KayitSayisi INT = 25,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @KayitSayisi <= 0
        SET @KayitSayisi = 25;

    IF @KayitSayisi > 100
        SET @KayitSayisi = 100;

    IF @AdminMi = 0
       AND NOT EXISTS
       (
            SELECT 1
            FROM dbo.KullaniciMagazalari km
            WHERE km.MagazaId = @MagazaId
              AND km.KullaniciId = @KullaniciId
              AND km.AktifMi = 1
       )
    BEGIN
        RAISERROR('Bu bayi stoğu için hareket geçmişini görüntüleme yetkiniz yok.', 16, 1);
        RETURN;
    END

    SELECT TOP (@KayitSayisi)
        msh.MagazaStokHareketId,
        msh.MagazaId,
        msh.ProductId AS UrunId,
        p.ProductName AS UrunAdi,
        msh.HareketTipi,
        CASE
            WHEN msh.HareketTipi IN (N'SiparisTeslimGiris', N'ManuelGiris', N'DuzeltmeGiris') THEN N'Giriş'
            WHEN msh.HareketTipi IN (N'BayiSatisCikis', N'ManuelCikis', N'DuzeltmeCikis', N'SiparisIptalCikis') THEN N'Çıkış'
            ELSE N'Hareket'
        END AS HareketYonu,
        CASE
            WHEN msh.HareketTipi = N'SiparisTeslimGiris' THEN N'Sipariş teslim girişi'
            WHEN msh.HareketTipi = N'ManuelGiris' THEN N'Manuel stok girişi'
            WHEN msh.HareketTipi = N'ManuelCikis' THEN N'Manuel stok çıkışı'
            WHEN msh.HareketTipi = N'SiparisIptalCikis' THEN N'Sipariş iptal/iade çıkışı'
            WHEN msh.HareketTipi = N'BayiSatisCikis' THEN N'Bayi satış çıkışı'
            WHEN msh.HareketTipi = N'DuzeltmeGiris' THEN N'Düzeltme girişi'
            WHEN msh.HareketTipi = N'DuzeltmeCikis' THEN N'Düzeltme çıkışı'
            ELSE msh.HareketTipi
        END AS HareketAciklama,
        msh.Miktar,
        msh.OncekiStok,
        msh.SonrakiStok,
        msh.KaynakSiparisId,
        o.OrderNo AS SiparisNo,
        msh.Aciklama,
        msh.OlusturmaTarihi
    FROM dbo.MagazaStokHareketleri msh
    INNER JOIN dbo.Products p
        ON p.ProductId = msh.ProductId
    LEFT JOIN dbo.Orders o
        ON o.OrderId = msh.KaynakSiparisId
    WHERE msh.MagazaId = @MagazaId
      AND (@ProductId IS NULL OR @ProductId = 0 OR msh.ProductId = @ProductId)
    ORDER BY msh.OlusturmaTarihi DESC, msh.MagazaStokHareketId DESC;
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
