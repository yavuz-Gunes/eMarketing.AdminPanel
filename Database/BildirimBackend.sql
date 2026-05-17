IF OBJECT_ID('dbo.Bildirimler', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bildirimler
    (
        BildirimId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bildirimler PRIMARY KEY,
        Baslik NVARCHAR(200) NOT NULL,
        Mesaj NVARCHAR(MAX) NULL,
        Tip NVARCHAR(30) NOT NULL CONSTRAINT DF_Bildirimler_Tip DEFAULT (N'Sistem'),
        HedefUrl NVARCHAR(500) NULL,
        HedefTipi NVARCHAR(30) NOT NULL CONSTRAINT DF_Bildirimler_HedefTipi DEFAULT (N'Yok'),
        HedefId INT NULL,
        KampanyaId INT NULL,
        KaynakAnahtari NVARCHAR(200) NULL,
        GlobalMi BIT NOT NULL CONSTRAINT DF_Bildirimler_GlobalMi DEFAULT (0),
        AktifMi BIT NOT NULL CONSTRAINT DF_Bildirimler_AktifMi DEFAULT (1),
        BaslangicTarihi DATETIME2 NULL,
        BitisTarihi DATETIME2 NULL,
        OlusturanKullaniciId INT NULL,
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_Bildirimler_Olusturma DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Bildirimler_Kullanicilar FOREIGN KEY (OlusturanKullaniciId) REFERENCES dbo.Kullanicilar(KullaniciId),
        CONSTRAINT FK_Bildirimler_Kampanyalar FOREIGN KEY (KampanyaId) REFERENCES dbo.Kampanyalar(KampanyaId)
    );
END
GO

IF COL_LENGTH('dbo.Bildirimler', 'KaynakAnahtari') IS NULL ALTER TABLE dbo.Bildirimler ADD KaynakAnahtari NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.Bildirimler', 'KampanyaId') IS NULL ALTER TABLE dbo.Bildirimler ADD KampanyaId INT NULL;
IF COL_LENGTH('dbo.Bildirimler', 'HedefTipi') IS NULL ALTER TABLE dbo.Bildirimler ADD HedefTipi NVARCHAR(30) NOT NULL CONSTRAINT DF_Bildirimler_HedefTipi DEFAULT (N'Yok');
IF COL_LENGTH('dbo.Bildirimler', 'HedefId') IS NULL ALTER TABLE dbo.Bildirimler ADD HedefId INT NULL;
GO

IF OBJECT_ID('dbo.BildirimMagazalari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BildirimMagazalari
    (
        BildirimMagazaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BildirimMagazalari PRIMARY KEY,
        BildirimId INT NOT NULL,
        MagazaId INT NOT NULL,
        CONSTRAINT FK_BildirimMagazalari_Bildirimler FOREIGN KEY (BildirimId) REFERENCES dbo.Bildirimler(BildirimId),
        CONSTRAINT FK_BildirimMagazalari_CustomerStores FOREIGN KEY (MagazaId) REFERENCES dbo.CustomerStores(CustomerStoreId),
        CONSTRAINT UX_BildirimMagazalari UNIQUE (BildirimId, MagazaId)
    );
END
GO

IF OBJECT_ID('dbo.BildirimOkumalari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BildirimOkumalari
    (
        BildirimOkumaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BildirimOkumalari PRIMARY KEY,
        BildirimId INT NOT NULL,
        KullaniciId INT NOT NULL,
        OkunmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_BildirimOkumalari_Okunma DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_BildirimOkumalari_Bildirimler FOREIGN KEY (BildirimId) REFERENCES dbo.Bildirimler(BildirimId),
        CONSTRAINT FK_BildirimOkumalari_Kullanicilar FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(KullaniciId),
        CONSTRAINT UX_BildirimOkumalari UNIQUE (BildirimId, KullaniciId)
    );
END
GO

IF OBJECT_ID('dbo.BildirimKritikStokDurumlari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.BildirimKritikStokDurumlari
    (
        BildirimKritikStokDurumId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BildirimKritikStokDurumlari PRIMARY KEY,
        MagazaId INT NOT NULL,
        UrunId INT NOT NULL,
        AktifMi BIT NOT NULL CONSTRAINT DF_BildirimKritikStokDurumlari_AktifMi DEFAULT (1),
        SonStok INT NOT NULL CONSTRAINT DF_BildirimKritikStokDurumlari_SonStok DEFAULT (0),
        KritikBaslangicTarihi DATETIME2 NULL,
        SonKontrolTarihi DATETIME2 NOT NULL CONSTRAINT DF_BildirimKritikStokDurumlari_SonKontrol DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UX_BildirimKritikStokDurumlari UNIQUE (MagazaId, UrunId),
        CONSTRAINT FK_BildirimKritikStokDurumlari_CustomerStores FOREIGN KEY (MagazaId) REFERENCES dbo.CustomerStores(CustomerStoreId),
        CONSTRAINT FK_BildirimKritikStokDurumlari_Products FOREIGN KEY (UrunId) REFERENCES dbo.Products(ProductId)
    );
END
GO

DECLARE @GunlukKampanyaOzet NVARCHAR(200) = N'G' + NCHAR(252) + N'nl' + NCHAR(252) + N'k kampanya ' + NCHAR(246) + N'zeti';
DECLARE @KritikStokUyarisi NVARCHAR(200) = N'Kritik stok uyar' + NCHAR(305) + N's' + NCHAR(305);
DECLARE @KritikStokMesajParcasi NVARCHAR(100) = N' kritik stok seviyesine d' + NCHAR(252) + NCHAR(351) + N't' + NCHAR(252) + N'. Mevcut stok: ';

UPDATE dbo.Bildirimler
SET Baslik = CASE
        WHEN Tip = N'Kampanya' AND KaynakAnahtari LIKE N'kampanya-ozet:%' THEN @GunlukKampanyaOzet
        WHEN Tip = N'KritikStok' THEN @KritikStokUyarisi
        WHEN Baslik IN (N'GÃ¼nlÃ¼k kampanya Ã¶zeti', N'GÃ¼nlÃ¼k kampanya özeti') THEN @GunlukKampanyaOzet
        WHEN Baslik IN (N'Kritik stok uyarÄ±sÄ±', N'Kritik stok uyarısı') THEN @KritikStokUyarisi
        ELSE Baslik
    END,
    Mesaj = CASE
        WHEN Tip = N'KritikStok' AND HedefId IS NOT NULL THEN ISNULL((SELECT TOP 1 p.ProductName FROM dbo.Products p WHERE p.ProductId = HedefId), N'Ürün') + @KritikStokMesajParcasi + N''
        ELSE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(Mesaj, N''), N'gÃ¼ncellendi', N'güncellendi'), N'Ã¼rÃ¼n', N'ürün'), N'dÃ¼ÅŸtÃ¼', N'düştü'), N'Ã¶zet', N'özet'), N'Ã¼', N'ü'), N'Ä±', N'ı')
    END,
    HedefTipi = CASE WHEN HedefTipi IN (N'ÃœrÃ¼n', N'Ürün') THEN N'Urun' ELSE HedefTipi END
WHERE Baslik LIKE N'%Ã%'
   OR Baslik LIKE N'%Ä%'
   OR Baslik LIKE N'%Å%'
   OR ISNULL(Mesaj, N'') LIKE N'%Ã%'
   OR ISNULL(Mesaj, N'') LIKE N'%Ä%'
   OR ISNULL(Mesaj, N'') LIKE N'%Å%'
   OR HedefTipi LIKE N'%Ã%'
   OR KaynakAnahtari LIKE N'kampanya-ozet:%'
   OR Tip = N'KritikStok';

UPDATE dbo.Bildirimler
SET HedefUrl = N'/notifications',
    HedefTipi = N'Sayfa',
    HedefId = NULL,
    KampanyaId = NULL
WHERE Tip = N'Kampanya'
  AND KaynakAnahtari LIKE N'kampanya-ozet:%'
  AND ISNULL(HedefTipi, N'') = N'Sayfa';

UPDATE b
SET HedefUrl = N'/campaigns',
    HedefTipi = N'Sayfa',
    HedefId = NULL,
    KampanyaId = NULL
FROM dbo.Bildirimler b
WHERE b.Tip = N'Kampanya'
  AND b.KaynakAnahtari LIKE N'kampanya-ozet:%';
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_Ekle
    @Baslik NVARCHAR(200),
    @Mesaj NVARCHAR(MAX) = NULL,
    @Tip NVARCHAR(30) = N'Sistem',
    @HedefUrl NVARCHAR(500) = NULL,
    @HedefTipi NVARCHAR(30) = N'Yok',
    @HedefId INT = NULL,
    @KampanyaId INT = NULL,
    @KaynakAnahtari NVARCHAR(200) = NULL,
    @GlobalMi BIT = 0,
    @AktifMi BIT = 1,
    @BaslangicTarihi DATETIME2 = NULL,
    @BitisTarihi DATETIME2 = NULL,
    @OlusturanKullaniciId INT = NULL,
    @MagazaIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Bildirimler
    (
        Baslik, Mesaj, Tip, HedefUrl, HedefTipi, HedefId, KampanyaId, KaynakAnahtari,
        GlobalMi, AktifMi, BaslangicTarihi, BitisTarihi, OlusturanKullaniciId
    )
    VALUES
    (
        LTRIM(RTRIM(@Baslik)),
        NULLIF(@Mesaj, N''),
        COALESCE(NULLIF(LTRIM(RTRIM(@Tip)), N''), N'Sistem'),
        NULLIF(LTRIM(RTRIM(@HedefUrl)), N''),
        COALESCE(NULLIF(LTRIM(RTRIM(@HedefTipi)), N''), N'Yok'),
        @HedefId,
        @KampanyaId,
        NULLIF(LTRIM(RTRIM(@KaynakAnahtari)), N''),
        ISNULL(@GlobalMi, 0),
        ISNULL(@AktifMi, 1),
        @BaslangicTarihi,
        @BitisTarihi,
        @OlusturanKullaniciId
    );

    DECLARE @BildirimId INT = SCOPE_IDENTITY();

    IF ISNULL(@GlobalMi, 0) = 0
    BEGIN
        INSERT INTO dbo.BildirimMagazalari (BildirimId, MagazaId)
        SELECT DISTINCT @BildirimId, TRY_CONVERT(INT, value)
        FROM STRING_SPLIT(ISNULL(@MagazaIds, N''), N',')
        WHERE TRY_CONVERT(INT, value) IS NOT NULL
          AND EXISTS (SELECT 1 FROM dbo.CustomerStores cs WHERE cs.CustomerStoreId = TRY_CONVERT(INT, value));
    END

    SELECT @BildirimId AS BildirimId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_Listele
    @KullaniciId INT,
    @AdminMi BIT = 0,
    @MagazaId INT = NULL,
    @SadeceOkunmamis BIT = 0,
    @Limit INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Gorunur AS
    (
        SELECT DISTINCT TOP (ISNULL(NULLIF(@Limit, 0), 50))
            b.BildirimId,
            b.Baslik,
            ISNULL(b.Mesaj, N'') AS Mesaj,
            b.Tip,
            ISNULL(b.HedefUrl, N'') AS HedefUrl,
            ISNULL(b.HedefTipi, N'Yok') AS HedefTipi,
            b.HedefId,
            b.KampanyaId,
            b.GlobalMi,
            b.AktifMi,
            b.BaslangicTarihi,
            b.BitisTarihi,
            b.OlusturmaTarihi,
            CASE WHEN bo.BildirimOkumaId IS NULL THEN CONVERT(BIT, 0) ELSE CONVERT(BIT, 1) END AS OkunduMu,
            bo.OkunmaTarihi,
            ISNULL((SELECT STRING_AGG(CONVERT(NVARCHAR(20), bm2.MagazaId), N',') FROM dbo.BildirimMagazalari bm2 WHERE bm2.BildirimId = b.BildirimId), N'') AS MagazaIds,
            ISNULL((SELECT STRING_AGG(cs.StoreName, N', ') FROM dbo.BildirimMagazalari bm3 INNER JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = bm3.MagazaId WHERE bm3.BildirimId = b.BildirimId), N'') AS MagazaAdlari
        FROM dbo.Bildirimler b
        LEFT JOIN dbo.BildirimOkumalari bo
            ON bo.BildirimId = b.BildirimId
           AND bo.KullaniciId = @KullaniciId
        WHERE b.AktifMi = 1
          AND (b.BaslangicTarihi IS NULL OR b.BaslangicTarihi <= SYSUTCDATETIME())
          AND (b.BitisTarihi IS NULL OR b.BitisTarihi >= SYSUTCDATETIME())
          AND (@SadeceOkunmamis = 0 OR bo.BildirimOkumaId IS NULL)
          AND
          (
              b.GlobalMi = 1
              OR (@AdminMi = 1 AND @MagazaId IS NULL)
              OR EXISTS
              (
                  SELECT 1
                  FROM dbo.BildirimMagazalari bm
                  WHERE bm.BildirimId = b.BildirimId
                    AND (@MagazaId IS NULL OR bm.MagazaId = @MagazaId)
                    AND
                    (
                        @AdminMi = 1
                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.KullaniciMagazalari km
                            WHERE km.MagazaId = bm.MagazaId
                              AND km.KullaniciId = @KullaniciId
                              AND km.AktifMi = 1
                        )
                    )
              )
          )
        ORDER BY b.OlusturmaTarihi DESC, b.BildirimId DESC
    )
    SELECT *
    FROM Gorunur
    ORDER BY OlusturmaTarihi DESC, BildirimId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_OkunmamisSayisi_Getir
    @KullaniciId INT,
    @AdminMi BIT = 0,
    @MagazaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(1) AS OkunmamisSayisi
    FROM
    (
        SELECT DISTINCT b.BildirimId
        FROM dbo.Bildirimler b
        LEFT JOIN dbo.BildirimOkumalari bo
            ON bo.BildirimId = b.BildirimId
           AND bo.KullaniciId = @KullaniciId
        WHERE bo.BildirimOkumaId IS NULL
          AND b.AktifMi = 1
          AND (b.BaslangicTarihi IS NULL OR b.BaslangicTarihi <= SYSUTCDATETIME())
          AND (b.BitisTarihi IS NULL OR b.BitisTarihi >= SYSUTCDATETIME())
          AND
          (
              b.GlobalMi = 1
              OR (@AdminMi = 1 AND @MagazaId IS NULL)
              OR EXISTS
              (
                  SELECT 1
                  FROM dbo.BildirimMagazalari bm
                  WHERE bm.BildirimId = b.BildirimId
                    AND (@MagazaId IS NULL OR bm.MagazaId = @MagazaId)
                    AND
                    (
                        @AdminMi = 1
                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.KullaniciMagazalari km
                            WHERE km.MagazaId = bm.MagazaId
                              AND km.KullaniciId = @KullaniciId
                              AND km.AktifMi = 1
                        )
                    )
              )
          )
    ) AS Gorunur;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_Okundu_Isaretle
    @BildirimId INT,
    @KullaniciId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.BildirimOkumalari
        WHERE BildirimId = @BildirimId
          AND KullaniciId = @KullaniciId
    )
    BEGIN
        INSERT INTO dbo.BildirimOkumalari (BildirimId, KullaniciId)
        VALUES (@BildirimId, @KullaniciId);
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_TumunuOkundu_Isaretle
    @KullaniciId INT,
    @AdminMi BIT = 0,
    @MagazaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.BildirimOkumalari (BildirimId, KullaniciId)
    SELECT DISTINCT b.BildirimId, @KullaniciId
    FROM dbo.Bildirimler b
    LEFT JOIN dbo.BildirimOkumalari bo
        ON bo.BildirimId = b.BildirimId
       AND bo.KullaniciId = @KullaniciId
    WHERE bo.BildirimOkumaId IS NULL
      AND b.AktifMi = 1
      AND (b.BaslangicTarihi IS NULL OR b.BaslangicTarihi <= SYSUTCDATETIME())
      AND (b.BitisTarihi IS NULL OR b.BitisTarihi >= SYSUTCDATETIME())
      AND
      (
          b.GlobalMi = 1
          OR (@AdminMi = 1 AND @MagazaId IS NULL)
          OR EXISTS
          (
              SELECT 1
              FROM dbo.BildirimMagazalari bm
              WHERE bm.BildirimId = b.BildirimId
                AND (@MagazaId IS NULL OR bm.MagazaId = @MagazaId)
                AND
                (
                    @AdminMi = 1
                    OR EXISTS
                    (
                        SELECT 1
                        FROM dbo.KullaniciMagazalari km
                        WHERE km.MagazaId = bm.MagazaId
                          AND km.KullaniciId = @KullaniciId
                          AND km.AktifMi = 1
                    )
                )
          )
      );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_Admin_Listele
    @Arama NVARCHAR(200) = N'',
    @Tip NVARCHAR(30) = N'',
    @Durum INT = -1,
    @MagazaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 200
        b.BildirimId,
        b.Baslik,
        ISNULL(b.Mesaj, N'') AS Mesaj,
        b.Tip,
        ISNULL(b.HedefUrl, N'') AS HedefUrl,
        ISNULL(b.HedefTipi, N'Yok') AS HedefTipi,
        b.HedefId,
        b.KampanyaId,
        b.GlobalMi,
        b.AktifMi,
        b.BaslangicTarihi,
        b.BitisTarihi,
        b.OlusturmaTarihi,
        CONVERT(BIT, 0) AS OkunduMu,
        CONVERT(DATETIME2, NULL) AS OkunmaTarihi,
        ISNULL((SELECT STRING_AGG(CONVERT(NVARCHAR(20), bm.MagazaId), N',') FROM dbo.BildirimMagazalari bm WHERE bm.BildirimId = b.BildirimId), N'') AS MagazaIds,
        ISNULL((SELECT STRING_AGG(cs.StoreName, N', ') FROM dbo.BildirimMagazalari bm INNER JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = bm.MagazaId WHERE bm.BildirimId = b.BildirimId), N'') AS MagazaAdlari
    FROM dbo.Bildirimler b
    WHERE (@Durum = -1 OR (@Durum = 1 AND b.AktifMi = 1) OR (@Durum = 0 AND b.AktifMi = 0))
      AND (NULLIF(LTRIM(RTRIM(@Tip)), N'') IS NULL OR b.Tip = @Tip)
      AND
      (
          NULLIF(LTRIM(RTRIM(@Arama)), N'') IS NULL
          OR b.Baslik LIKE N'%' + @Arama + N'%'
          OR ISNULL(b.Mesaj, N'') LIKE N'%' + @Arama + N'%'
      )
      AND
      (
          @MagazaId IS NULL
          OR b.GlobalMi = 1
          OR EXISTS
          (
              SELECT 1
              FROM dbo.BildirimMagazalari bm
              WHERE bm.BildirimId = b.BildirimId
                AND bm.MagazaId = @MagazaId
          )
      )
    ORDER BY b.OlusturmaTarihi DESC, b.BildirimId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_DurumGuncelle
    @BildirimId INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Bildirimler
    SET AktifMi = @AktifMi
    WHERE BildirimId = @BildirimId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_Sil
    @BildirimId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.BildirimOkumalari WHERE BildirimId = @BildirimId;
    DELETE FROM dbo.BildirimMagazalari WHERE BildirimId = @BildirimId;
    DELETE FROM dbo.Bildirimler WHERE BildirimId = @BildirimId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_KampanyaDegisiklik_Uret
    @KampanyaId INT,
    @OlusturanKullaniciId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Kampanya bildirimleri V2'de günlük özet olarak üretilir.
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_GunlukKampanyaOzet_Uret
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 0,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Bugun CHAR(8) = CONVERT(CHAR(8), SYSUTCDATETIME(), 112);
    DECLARE @GunlukKampanyaOzet NVARCHAR(200) = N'G' + NCHAR(252) + N'nl' + NCHAR(252) + N'k kampanya ' + NCHAR(246) + N'zeti';

    IF OBJECT_ID('tempdb..#KampanyaOzet') IS NOT NULL DROP TABLE #KampanyaOzet;
    CREATE TABLE #KampanyaOzet
    (
        MagazaId INT NOT NULL,
        KampanyaSayisi INT NOT NULL,
        IlkKampanyaId INT NULL,
        KaynakAnahtari NVARCHAR(200) NOT NULL
    );

    ;WITH HedefMagazalar AS
    (
        SELECT DISTINCT cs.CustomerStoreId AS MagazaId, cs.CustomerId
        FROM dbo.CustomerStores cs
        WHERE cs.IsActive = 1
          AND (@TumMagazalar = 1 OR @MagazaId IS NULL OR cs.CustomerStoreId = @MagazaId)
          AND
          (
              @AdminMi = 1
              OR EXISTS
              (
                  SELECT 1
                  FROM dbo.KullaniciMagazalari km
                  WHERE km.MagazaId = cs.CustomerStoreId
                    AND km.KullaniciId = @KullaniciId
                    AND km.AktifMi = 1
              )
          )
    ),
    AktifKampanyalar AS
    (
        SELECT
            hm.MagazaId,
            COUNT(DISTINCT k.KampanyaId) AS KampanyaSayisi,
            MIN(k.KampanyaId) AS IlkKampanyaId
        FROM HedefMagazalar hm
        INNER JOIN dbo.Kampanyalar k
            ON k.AktifMi = 1
           AND (k.BaslangicTarihi IS NULL OR k.BaslangicTarihi <= SYSUTCDATETIME())
           AND (k.BitisTarihi IS NULL OR k.BitisTarihi >= SYSUTCDATETIME())
           AND
           (
               k.GlobalMi = 1
               OR EXISTS (SELECT 1 FROM dbo.KampanyaMagazalari km WHERE km.KampanyaId = k.KampanyaId AND km.MagazaId = hm.MagazaId)
               OR EXISTS (SELECT 1 FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId AND kb.BayiId = hm.CustomerId)
           )
        GROUP BY hm.MagazaId
    )
    INSERT INTO #KampanyaOzet (MagazaId, KampanyaSayisi, IlkKampanyaId, KaynakAnahtari)
    SELECT
        ak.MagazaId,
        ak.KampanyaSayisi,
        ak.IlkKampanyaId,
        CONCAT(N'kampanya-ozet:', ak.MagazaId, N':', @Bugun)
    FROM AktifKampanyalar ak;

    INSERT INTO dbo.Bildirimler
    (
        Baslik, Mesaj, Tip, HedefUrl, HedefTipi, HedefId, KampanyaId, KaynakAnahtari,
        GlobalMi, AktifMi
    )
    SELECT
        @GunlukKampanyaOzet,
        CONCAT(o.KampanyaSayisi, N' aktif kampanya sizi bekliyor.'),
        N'Kampanya',
        CASE WHEN o.KampanyaSayisi = 1 THEN CONCAT(N'/campaigns/', o.IlkKampanyaId) ELSE N'/campaigns' END,
        CASE WHEN o.KampanyaSayisi = 1 THEN N'Kampanya' ELSE N'Sayfa' END,
        CASE WHEN o.KampanyaSayisi = 1 THEN o.IlkKampanyaId ELSE NULL END,
        CASE WHEN o.KampanyaSayisi = 1 THEN o.IlkKampanyaId ELSE NULL END,
        o.KaynakAnahtari,
        0,
        1
    FROM #KampanyaOzet o
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Bildirimler b
        INNER JOIN dbo.BildirimMagazalari bm ON bm.BildirimId = b.BildirimId
        WHERE b.KaynakAnahtari = o.KaynakAnahtari
          AND bm.MagazaId = o.MagazaId
    );

    INSERT INTO dbo.BildirimMagazalari (BildirimId, MagazaId)
    SELECT b.BildirimId, o.MagazaId
    FROM #KampanyaOzet o
    INNER JOIN dbo.Bildirimler b ON b.KaynakAnahtari = o.KaynakAnahtari
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.BildirimMagazalari bm
        WHERE bm.BildirimId = b.BildirimId
          AND bm.MagazaId = o.MagazaId
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Bildirim_KritikStokDegisiklik_Uret
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 1,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @KritikStokUyarisi NVARCHAR(200) = N'Kritik stok uyar' + NCHAR(305) + N's' + NCHAR(305);
    DECLARE @KritikStokMesajParcasi NVARCHAR(100) = N' kritik stok seviyesine d' + NCHAR(252) + NCHAR(351) + N't' + NCHAR(252) + N'. Mevcut stok: ';

    IF OBJECT_ID('tempdb..#KritikSon') IS NOT NULL DROP TABLE #KritikSon;
    CREATE TABLE #KritikSon
    (
        MagazaId INT NOT NULL,
        UrunId INT NOT NULL,
        UrunAdi NVARCHAR(300) NOT NULL,
        BayiStok INT NOT NULL,
        KaynakAnahtari NVARCHAR(200) NOT NULL
    );

    INSERT INTO #KritikSon (MagazaId, UrunId, UrunAdi, BayiStok, KaynakAnahtari)
    SELECT
        msl.MagazaId,
        msl.UrunId,
        msl.UrunAdi,
        msl.BayiStok,
        CONCAT(N'kritik-stok:', msl.MagazaId, N':', msl.UrunId, N':', CONVERT(NVARCHAR(30), SYSUTCDATETIME(), 126))
    FROM dbo.vw_MagazaStok_Liste msl
    WHERE msl.AktifMi = 1
      AND msl.BayiStok > 0
      AND (msl.StokDurumu = N'Kritik' OR (msl.MinimumStok <= 0 AND msl.BayiStok <= 5))
      AND (@TumMagazalar = 1 OR msl.MagazaId = @MagazaId)
      AND
      (
          @AdminMi = 1
          OR EXISTS
          (
              SELECT 1
              FROM dbo.KullaniciMagazalari km
              WHERE km.MagazaId = msl.MagazaId
                AND km.KullaniciId = @KullaniciId
                AND km.AktifMi = 1
          )
      );

    INSERT INTO dbo.Bildirimler (Baslik, Mesaj, Tip, HedefUrl, HedefTipi, HedefId, KaynakAnahtari, GlobalMi, AktifMi)
    SELECT
        @KritikStokUyarisi,
        CONCAT(k.UrunAdi, @KritikStokMesajParcasi, k.BayiStok),
        N'KritikStok',
        CONCAT(N'/products?urunId=', k.UrunId),
        N'Urun',
        k.UrunId,
        k.KaynakAnahtari,
        0,
        1
    FROM #KritikSon k
    LEFT JOIN dbo.BildirimKritikStokDurumlari d
        ON d.MagazaId = k.MagazaId
       AND d.UrunId = k.UrunId
       AND d.AktifMi = 1
    WHERE d.BildirimKritikStokDurumId IS NULL;

    INSERT INTO dbo.BildirimMagazalari (BildirimId, MagazaId)
    SELECT b.BildirimId, k.MagazaId
    FROM #KritikSon k
    INNER JOIN dbo.Bildirimler b ON b.KaynakAnahtari = k.KaynakAnahtari
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.BildirimMagazalari bm
        WHERE bm.BildirimId = b.BildirimId
          AND bm.MagazaId = k.MagazaId
    );

    MERGE dbo.BildirimKritikStokDurumlari AS target
    USING #KritikSon AS source
        ON target.MagazaId = source.MagazaId
       AND target.UrunId = source.UrunId
    WHEN MATCHED THEN
        UPDATE SET AktifMi = 1,
                   SonStok = source.BayiStok,
                   SonKontrolTarihi = SYSUTCDATETIME(),
                   KritikBaslangicTarihi = COALESCE(target.KritikBaslangicTarihi, SYSUTCDATETIME())
    WHEN NOT MATCHED THEN
        INSERT (MagazaId, UrunId, AktifMi, SonStok, KritikBaslangicTarihi, SonKontrolTarihi)
        VALUES (source.MagazaId, source.UrunId, 1, source.BayiStok, SYSUTCDATETIME(), SYSUTCDATETIME());

    UPDATE target
    SET AktifMi = 0,
        SonKontrolTarihi = SYSUTCDATETIME()
    FROM dbo.BildirimKritikStokDurumlari target
    WHERE (@TumMagazalar = 1 OR target.MagazaId = @MagazaId)
      AND target.AktifMi = 1
      AND NOT EXISTS
      (
          SELECT 1
          FROM #KritikSon source
          WHERE source.MagazaId = target.MagazaId
            AND source.UrunId = target.UrunId
      );
END
GO
