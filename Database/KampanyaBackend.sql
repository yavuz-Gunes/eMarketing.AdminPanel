IF OBJECT_ID('dbo.Kampanyalar', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Kampanyalar
    (
        KampanyaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Kampanyalar PRIMARY KEY,
        Baslik NVARCHAR(200) NOT NULL,
        Aciklama NVARCHAR(500) NULL,
        DetayMetni NVARCHAR(MAX) NULL,
        KatilimSartlari NVARCHAR(MAX) NULL,
        AdminNotlari NVARCHAR(MAX) NULL,
        GorselUrl NVARCHAR(500) NULL,
        CtaMetni NVARCHAR(80) NULL,
        HedefUrl NVARCHAR(500) NULL,
        HedefUrunId INT NULL,
        HedefKategoriId INT NULL,
        BaslangicTarihi DATETIME2 NULL,
        BitisTarihi DATETIME2 NULL,
        Oncelik INT NOT NULL CONSTRAINT DF_Kampanyalar_Oncelik DEFAULT 10,
        AktifMi BIT NOT NULL CONSTRAINT DF_Kampanyalar_AktifMi DEFAULT 1,
        GlobalMi BIT NOT NULL CONSTRAINT DF_Kampanyalar_GlobalMi DEFAULT 1,
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_Kampanyalar_Olusturma DEFAULT SYSUTCDATETIME(),
        GuncellemeTarihi DATETIME2 NULL,
        CONSTRAINT FK_Kampanyalar_Products FOREIGN KEY (HedefUrunId) REFERENCES dbo.Products(ProductId),
        CONSTRAINT FK_Kampanyalar_Categories FOREIGN KEY (HedefKategoriId) REFERENCES dbo.Categories(CategoryId)
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Kampanyalar', 'DetayMetni') IS NULL ALTER TABLE dbo.Kampanyalar ADD DetayMetni NVARCHAR(MAX) NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'KatilimSartlari') IS NULL ALTER TABLE dbo.Kampanyalar ADD KatilimSartlari NVARCHAR(MAX) NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'AdminNotlari') IS NULL ALTER TABLE dbo.Kampanyalar ADD AdminNotlari NVARCHAR(MAX) NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'GorselUrl') IS NULL ALTER TABLE dbo.Kampanyalar ADD GorselUrl NVARCHAR(500) NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'CtaMetni') IS NULL ALTER TABLE dbo.Kampanyalar ADD CtaMetni NVARCHAR(80) NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'HedefUrl') IS NULL ALTER TABLE dbo.Kampanyalar ADD HedefUrl NVARCHAR(500) NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'HedefUrunId') IS NULL ALTER TABLE dbo.Kampanyalar ADD HedefUrunId INT NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'HedefKategoriId') IS NULL ALTER TABLE dbo.Kampanyalar ADD HedefKategoriId INT NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'BaslangicTarihi') IS NULL ALTER TABLE dbo.Kampanyalar ADD BaslangicTarihi DATETIME2 NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'BitisTarihi') IS NULL ALTER TABLE dbo.Kampanyalar ADD BitisTarihi DATETIME2 NULL;
    IF COL_LENGTH('dbo.Kampanyalar', 'Oncelik') IS NULL ALTER TABLE dbo.Kampanyalar ADD Oncelik INT NOT NULL CONSTRAINT DF_Kampanyalar_Oncelik DEFAULT 10;
    IF COL_LENGTH('dbo.Kampanyalar', 'AktifMi') IS NULL ALTER TABLE dbo.Kampanyalar ADD AktifMi BIT NOT NULL CONSTRAINT DF_Kampanyalar_AktifMi DEFAULT 1;
    IF COL_LENGTH('dbo.Kampanyalar', 'GlobalMi') IS NULL ALTER TABLE dbo.Kampanyalar ADD GlobalMi BIT NOT NULL CONSTRAINT DF_Kampanyalar_GlobalMi DEFAULT 1;
    IF COL_LENGTH('dbo.Kampanyalar', 'OlusturmaTarihi') IS NULL ALTER TABLE dbo.Kampanyalar ADD OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_Kampanyalar_Olusturma DEFAULT SYSUTCDATETIME();
    IF COL_LENGTH('dbo.Kampanyalar', 'GuncellemeTarihi') IS NULL ALTER TABLE dbo.Kampanyalar ADD GuncellemeTarihi DATETIME2 NULL;
END
GO

IF OBJECT_ID('dbo.KampanyaBayileri', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KampanyaBayileri
    (
        KampanyaBayiId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KampanyaBayileri PRIMARY KEY,
        KampanyaId INT NOT NULL,
        BayiId INT NOT NULL,
        CONSTRAINT FK_KampanyaBayileri_Kampanyalar FOREIGN KEY (KampanyaId) REFERENCES dbo.Kampanyalar(KampanyaId),
        CONSTRAINT FK_KampanyaBayileri_Customers FOREIGN KEY (BayiId) REFERENCES dbo.Customers(CustomerId),
        CONSTRAINT UX_KampanyaBayileri UNIQUE (KampanyaId, BayiId)
    );
END
GO

IF OBJECT_ID('dbo.KampanyaMagazalari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KampanyaMagazalari
    (
        KampanyaMagazaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KampanyaMagazalari PRIMARY KEY,
        KampanyaId INT NOT NULL,
        MagazaId INT NOT NULL,
        CONSTRAINT FK_KampanyaMagazalari_Kampanyalar FOREIGN KEY (KampanyaId) REFERENCES dbo.Kampanyalar(KampanyaId),
        CONSTRAINT FK_KampanyaMagazalari_CustomerStores FOREIGN KEY (MagazaId) REFERENCES dbo.CustomerStores(CustomerStoreId),
        CONSTRAINT UX_KampanyaMagazalari UNIQUE (KampanyaId, MagazaId)
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Admin_Listele
    @Arama NVARCHAR(200) = N'',
    @Durum INT = -1,
    @KampanyaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        k.KampanyaId,
        k.Baslik,
        ISNULL(k.Aciklama, N'') AS Aciklama,
        ISNULL(k.DetayMetni, N'') AS DetayMetni,
        ISNULL(k.KatilimSartlari, N'') AS KatilimSartlari,
        ISNULL(k.AdminNotlari, N'') AS AdminNotlari,
        ISNULL(k.GorselUrl, N'') AS GorselUrl,
        ISNULL(k.CtaMetni, N'Detaya Git') AS CtaMetni,
        ISNULL(k.HedefUrl, N'/products') AS HedefUrl,
        k.HedefUrunId,
        k.HedefKategoriId,
        k.BaslangicTarihi,
        k.BitisTarihi,
        k.Oncelik,
        k.AktifMi,
        k.GlobalMi,
        CASE
            WHEN k.GlobalMi = 1 THEN N'Global'
            WHEN EXISTS (SELECT 1 FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId)
             AND EXISTS (SELECT 1 FROM dbo.KampanyaMagazalari km WHERE km.KampanyaId = k.KampanyaId) THEN N'Karma'
            WHEN EXISTS (SELECT 1 FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId) THEN N'Bayi'
            ELSE N'Mağaza'
        END AS Kapsam,
        ISNULL((SELECT STRING_AGG(CONVERT(NVARCHAR(20), kb.BayiId), N',') FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId), N'') AS BayiIds,
        ISNULL((SELECT STRING_AGG(ISNULL(c.CompanyName, c.FullName), N', ') FROM dbo.KampanyaBayileri kb INNER JOIN dbo.Customers c ON c.CustomerId = kb.BayiId WHERE kb.KampanyaId = k.KampanyaId), N'') AS BayiAdlari,
        ISNULL((SELECT STRING_AGG(CONVERT(NVARCHAR(20), km.MagazaId), N',') FROM dbo.KampanyaMagazalari km WHERE km.KampanyaId = k.KampanyaId), N'') AS MagazaIds,
        ISNULL((SELECT STRING_AGG(cs.StoreName, N', ') FROM dbo.KampanyaMagazalari km INNER JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = km.MagazaId WHERE km.KampanyaId = k.KampanyaId), N'') AS MagazaAdlari,
        k.OlusturmaTarihi,
        k.GuncellemeTarihi
    FROM dbo.Kampanyalar k
    WHERE (@KampanyaId IS NULL OR k.KampanyaId = @KampanyaId)
      AND (@Durum = -1 OR (@Durum = 1 AND k.AktifMi = 1) OR (@Durum = 0 AND k.AktifMi = 0))
      AND (
            NULLIF(LTRIM(RTRIM(@Arama)), N'') IS NULL
            OR k.Baslik LIKE N'%' + @Arama + N'%'
            OR ISNULL(k.Aciklama, N'') LIKE N'%' + @Arama + N'%'
          )
    ORDER BY k.Oncelik, k.KampanyaId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Admin_Getir
    @KampanyaId INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.sp_Kampanya_Admin_Listele @Arama = N'', @Durum = -1, @KampanyaId = @KampanyaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Ekle
    @Baslik NVARCHAR(200),
    @Aciklama NVARCHAR(500) = NULL,
    @DetayMetni NVARCHAR(MAX) = NULL,
    @KatilimSartlari NVARCHAR(MAX) = NULL,
    @AdminNotlari NVARCHAR(MAX) = NULL,
    @GorselUrl NVARCHAR(500) = NULL,
    @CtaMetni NVARCHAR(80) = NULL,
    @HedefUrl NVARCHAR(500) = NULL,
    @HedefUrunId INT = NULL,
    @HedefKategoriId INT = NULL,
    @BaslangicTarihi DATETIME2 = NULL,
    @BitisTarihi DATETIME2 = NULL,
    @Oncelik INT = 10,
    @AktifMi BIT = 1,
    @GlobalMi BIT = 1,
    @BayiIds NVARCHAR(MAX) = NULL,
    @MagazaIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Kampanyalar
    (
        Baslik, Aciklama, DetayMetni, KatilimSartlari, AdminNotlari, GorselUrl, CtaMetni, HedefUrl,
        HedefUrunId, HedefKategoriId, BaslangicTarihi, BitisTarihi, Oncelik, AktifMi, GlobalMi
    )
    VALUES
    (
        LTRIM(RTRIM(@Baslik)), NULLIF(LTRIM(RTRIM(@Aciklama)), N''), NULLIF(@DetayMetni, N''),
        NULLIF(@KatilimSartlari, N''), NULLIF(@AdminNotlari, N''), NULLIF(LTRIM(RTRIM(@GorselUrl)), N''),
        COALESCE(NULLIF(LTRIM(RTRIM(@CtaMetni)), N''), N'Detaya Git'),
        COALESCE(NULLIF(LTRIM(RTRIM(@HedefUrl)), N''), N'/products'),
        @HedefUrunId, @HedefKategoriId, @BaslangicTarihi, @BitisTarihi,
        ISNULL(@Oncelik, 10), ISNULL(@AktifMi, 1), ISNULL(@GlobalMi, 1)
    );

    DECLARE @KampanyaId INT = SCOPE_IDENTITY();
    EXEC dbo.sp_Kampanya_Hedefleri_Kaydet @KampanyaId, @GlobalMi, @BayiIds, @MagazaIds;
    SELECT @KampanyaId AS KampanyaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Guncelle
    @KampanyaId INT,
    @Baslik NVARCHAR(200),
    @Aciklama NVARCHAR(500) = NULL,
    @DetayMetni NVARCHAR(MAX) = NULL,
    @KatilimSartlari NVARCHAR(MAX) = NULL,
    @AdminNotlari NVARCHAR(MAX) = NULL,
    @GorselUrl NVARCHAR(500) = NULL,
    @CtaMetni NVARCHAR(80) = NULL,
    @HedefUrl NVARCHAR(500) = NULL,
    @HedefUrunId INT = NULL,
    @HedefKategoriId INT = NULL,
    @BaslangicTarihi DATETIME2 = NULL,
    @BitisTarihi DATETIME2 = NULL,
    @Oncelik INT = 10,
    @AktifMi BIT = 1,
    @GlobalMi BIT = 1,
    @BayiIds NVARCHAR(MAX) = NULL,
    @MagazaIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Kampanyalar
    SET Baslik = LTRIM(RTRIM(@Baslik)),
        Aciklama = NULLIF(LTRIM(RTRIM(@Aciklama)), N''),
        DetayMetni = NULLIF(@DetayMetni, N''),
        KatilimSartlari = NULLIF(@KatilimSartlari, N''),
        AdminNotlari = NULLIF(@AdminNotlari, N''),
        GorselUrl = NULLIF(LTRIM(RTRIM(@GorselUrl)), N''),
        CtaMetni = COALESCE(NULLIF(LTRIM(RTRIM(@CtaMetni)), N''), N'Detaya Git'),
        HedefUrl = COALESCE(NULLIF(LTRIM(RTRIM(@HedefUrl)), N''), N'/products'),
        HedefUrunId = @HedefUrunId,
        HedefKategoriId = @HedefKategoriId,
        BaslangicTarihi = @BaslangicTarihi,
        BitisTarihi = @BitisTarihi,
        Oncelik = ISNULL(@Oncelik, 10),
        AktifMi = ISNULL(@AktifMi, 1),
        GlobalMi = ISNULL(@GlobalMi, 1),
        GuncellemeTarihi = SYSUTCDATETIME()
    WHERE KampanyaId = @KampanyaId;

    IF @@ROWCOUNT = 0
        THROW 51000, N'Kampanya bulunamadı.', 1;

    EXEC dbo.sp_Kampanya_Hedefleri_Kaydet @KampanyaId, @GlobalMi, @BayiIds, @MagazaIds;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Hedefleri_Kaydet
    @KampanyaId INT,
    @GlobalMi BIT,
    @BayiIds NVARCHAR(MAX) = NULL,
    @MagazaIds NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.KampanyaBayileri WHERE KampanyaId = @KampanyaId;
    DELETE FROM dbo.KampanyaMagazalari WHERE KampanyaId = @KampanyaId;

    IF ISNULL(@GlobalMi, 1) = 1
        RETURN;

    INSERT INTO dbo.KampanyaBayileri (KampanyaId, BayiId)
    SELECT DISTINCT @KampanyaId, TRY_CONVERT(INT, value)
    FROM STRING_SPLIT(ISNULL(@BayiIds, N''), N',')
    WHERE TRY_CONVERT(INT, value) IS NOT NULL
      AND EXISTS (SELECT 1 FROM dbo.Customers c WHERE c.CustomerId = TRY_CONVERT(INT, value));

    INSERT INTO dbo.KampanyaMagazalari (KampanyaId, MagazaId)
    SELECT DISTINCT @KampanyaId, TRY_CONVERT(INT, value)
    FROM STRING_SPLIT(ISNULL(@MagazaIds, N''), N',')
    WHERE TRY_CONVERT(INT, value) IS NOT NULL
      AND EXISTS (SELECT 1 FROM dbo.CustomerStores cs WHERE cs.CustomerStoreId = TRY_CONVERT(INT, value));
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_DurumGuncelle
    @KampanyaId INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Kampanyalar
    SET AktifMi = @AktifMi,
        GuncellemeTarihi = SYSUTCDATETIME()
    WHERE KampanyaId = @KampanyaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Sil
    @KampanyaId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.KampanyaBayileri WHERE KampanyaId = @KampanyaId;
    DELETE FROM dbo.KampanyaMagazalari WHERE KampanyaId = @KampanyaId;
    DELETE FROM dbo.Kampanyalar WHERE KampanyaId = @KampanyaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Aktif_Listele
    @MagazaId INT = NULL,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        k.KampanyaId,
        k.Baslik,
        ISNULL(k.Aciklama, N'') AS Aciklama,
        ISNULL(k.GorselUrl, N'') AS GorselUrl,
        ISNULL(k.CtaMetni, N'Detaya Git') AS CtaMetni,
        ISNULL(k.HedefUrl, N'/products') AS HedefUrl,
        k.Oncelik,
        k.BaslangicTarihi,
        k.BitisTarihi
    FROM dbo.Kampanyalar k
    LEFT JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = @MagazaId
    WHERE k.AktifMi = 1
      AND (k.BaslangicTarihi IS NULL OR k.BaslangicTarihi <= SYSUTCDATETIME())
      AND (k.BitisTarihi IS NULL OR k.BitisTarihi >= SYSUTCDATETIME())
      AND (
            @AdminMi = 1
            OR k.GlobalMi = 1
            OR EXISTS (SELECT 1 FROM dbo.KampanyaMagazalari km WHERE km.KampanyaId = k.KampanyaId AND km.MagazaId = @MagazaId)
            OR EXISTS (SELECT 1 FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId AND kb.BayiId = cs.CustomerId)
          )
    ORDER BY k.Oncelik, k.KampanyaId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kampanya_Detay_Getir
    @KampanyaId INT,
    @MagazaId INT = NULL,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM
    (
        SELECT
            k.KampanyaId,
            k.Baslik,
            ISNULL(k.Aciklama, N'') AS Aciklama,
            ISNULL(k.DetayMetni, N'') AS DetayMetni,
            ISNULL(k.KatilimSartlari, N'') AS KatilimSartlari,
            ISNULL(k.AdminNotlari, N'') AS AdminNotlari,
            ISNULL(k.GorselUrl, N'') AS GorselUrl,
            ISNULL(k.CtaMetni, N'Detaya Git') AS CtaMetni,
            ISNULL(k.HedefUrl, N'/products') AS HedefUrl,
            k.HedefUrunId,
            k.HedefKategoriId,
            k.BaslangicTarihi,
            k.BitisTarihi,
            k.Oncelik,
            k.AktifMi,
            k.GlobalMi,
            CASE
                WHEN k.GlobalMi = 1 THEN N'Global'
                WHEN EXISTS (SELECT 1 FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId)
                 AND EXISTS (SELECT 1 FROM dbo.KampanyaMagazalari km WHERE km.KampanyaId = k.KampanyaId) THEN N'Karma'
                WHEN EXISTS (SELECT 1 FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId) THEN N'Bayi'
                ELSE N'Mağaza'
            END AS Kapsam,
            ISNULL((SELECT STRING_AGG(CONVERT(NVARCHAR(20), kb.BayiId), N',') FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId), N'') AS BayiIds,
            ISNULL((SELECT STRING_AGG(ISNULL(c.CompanyName, c.FullName), N', ') FROM dbo.KampanyaBayileri kb INNER JOIN dbo.Customers c ON c.CustomerId = kb.BayiId WHERE kb.KampanyaId = k.KampanyaId), N'') AS BayiAdlari,
            ISNULL((SELECT STRING_AGG(CONVERT(NVARCHAR(20), km.MagazaId), N',') FROM dbo.KampanyaMagazalari km WHERE km.KampanyaId = k.KampanyaId), N'') AS MagazaIds,
            ISNULL((SELECT STRING_AGG(cs.StoreName, N', ') FROM dbo.KampanyaMagazalari km INNER JOIN dbo.CustomerStores cs ON cs.CustomerStoreId = km.MagazaId WHERE km.KampanyaId = k.KampanyaId), N'') AS MagazaAdlari,
            k.OlusturmaTarihi,
            k.GuncellemeTarihi
        FROM dbo.Kampanyalar k
        LEFT JOIN dbo.CustomerStores selectedStore ON selectedStore.CustomerStoreId = @MagazaId
        WHERE k.KampanyaId = @KampanyaId
          AND (
                @AdminMi = 1
                OR (
                    k.AktifMi = 1
                    AND (k.BaslangicTarihi IS NULL OR k.BaslangicTarihi <= SYSUTCDATETIME())
                    AND (k.BitisTarihi IS NULL OR k.BitisTarihi >= SYSUTCDATETIME())
                    AND (
                        k.GlobalMi = 1
                        OR EXISTS (SELECT 1 FROM dbo.KampanyaMagazalari km WHERE km.KampanyaId = k.KampanyaId AND km.MagazaId = @MagazaId)
                        OR EXISTS (SELECT 1 FROM dbo.KampanyaBayileri kb WHERE kb.KampanyaId = k.KampanyaId AND kb.BayiId = selectedStore.CustomerId)
                    )
                )
              )
    ) AS campaign;
END
GO
