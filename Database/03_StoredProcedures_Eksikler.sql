IF OBJECT_ID('dbo.sp_Kullanici_GirisYap', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Kullanici_GirisYap;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kategori_Listele
    @Arama NVARCHAR(150) = '',
    @Durum INT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CategoryId AS KategoriId,
        CategoryName AS KategoriAdi,
        IsActive AS AktifMi,
        CreatedAt AS OlusturmaTarihi
    FROM dbo.Categories
    WHERE
        (@Durum = -1 OR IsActive = @Durum)
        AND
        (
            @Arama = ''
            OR CategoryName LIKE '%' + @Arama + '%'
        )
    ORDER BY CategoryName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kategori_Getir
    @KategoriId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        CategoryId AS KategoriId,
        CategoryName AS KategoriAdi,
        IsActive AS AktifMi,
        CreatedAt AS OlusturmaTarihi
    FROM dbo.Categories
    WHERE CategoryId = @KategoriId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kategori_Ekle
    @KategoriAdi NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@KategoriAdi)), '') IS NULL
    BEGIN
        RAISERROR('Kategori adı zorunludur.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Categories WHERE CategoryName = LTRIM(RTRIM(@KategoriAdi)))
    BEGIN
        RAISERROR('Bu kategori adı zaten kullanılıyor.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Categories (CategoryName, IsActive)
    VALUES (LTRIM(RTRIM(@KategoriAdi)), 1);

    SELECT SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kategori_Guncelle
    @KategoriId INT,
    @KategoriAdi NVARCHAR(150),
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@KategoriAdi)), '') IS NULL
    BEGIN
        RAISERROR('Kategori adı zorunludur.', 16, 1);
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Categories
        WHERE CategoryName = LTRIM(RTRIM(@KategoriAdi))
          AND CategoryId <> @KategoriId
    )
    BEGIN
        RAISERROR('Bu kategori adı zaten kullanılıyor.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Categories
    SET
        CategoryName = LTRIM(RTRIM(@KategoriAdi)),
        IsActive = @AktifMi
    WHERE CategoryId = @KategoriId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kategori_Sil
    @KategoriId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Categories
    SET IsActive = 0
    WHERE CategoryId = @KategoriId;
END
GO

CREATE OR ALTER VIEW dbo.vw_UrunListesi
AS
SELECT
    p.ProductId AS UrunId,
    p.CategoryId AS KategoriId,
    c.CategoryName AS KategoriAdi,
    p.ProductName AS UrunAdi,
    p.Description AS Aciklama,
    p.Price AS Fiyat,
    p.Stock AS Stok,
    p.ImageUrl AS GorselUrl,
    p.IsActive AS AktifMi,
    CASE
        WHEN p.Stock <= 0 THEN N'Tukendi'
        WHEN p.Stock <= 5 THEN N'Kritik'
        ELSE N'Yeterli'
    END AS StokDurumu,
    p.CreatedDate AS OlusturmaTarihi
FROM dbo.Products p
LEFT JOIN dbo.Categories c
    ON c.CategoryId = p.CategoryId;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Urun_Listele
    @Arama NVARCHAR(200) = '',
    @Durum INT = 1,
    @KategoriId INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UrunId,
        KategoriId,
        KategoriAdi,
        UrunAdi,
        Aciklama,
        Fiyat,
        Stok,
        GorselUrl,
        AktifMi,
        StokDurumu,
        OlusturmaTarihi
    FROM dbo.vw_UrunListesi
    WHERE
        (@Durum = -1 OR AktifMi = @Durum)
        AND (@KategoriId = 0 OR KategoriId = @KategoriId)
        AND
        (
            @Arama = ''
            OR UrunAdi LIKE '%' + @Arama + '%'
            OR Aciklama LIKE '%' + @Arama + '%'
            OR KategoriAdi LIKE '%' + @Arama + '%'
        )
    ORDER BY UrunAdi;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Urun_Getir
    @UrunId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        UrunId,
        KategoriId,
        KategoriAdi,
        UrunAdi,
        Aciklama,
        Fiyat,
        Stok,
        GorselUrl,
        AktifMi,
        StokDurumu,
        OlusturmaTarihi
    FROM dbo.vw_UrunListesi
    WHERE UrunId = @UrunId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Urun_Ekle
    @KategoriId INT,
    @UrunAdi NVARCHAR(200),
    @Aciklama NVARCHAR(MAX) = NULL,
    @Fiyat DECIMAL(18,2),
    @Stok INT,
    @GorselUrl NVARCHAR(500) = NULL,
    @AktifMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@UrunAdi)), '') IS NULL
    BEGIN
        RAISERROR('Ürün adı zorunludur.', 16, 1);
        RETURN;
    END

    IF @Fiyat < 0 OR @Stok < 0
    BEGIN
        RAISERROR('Fiyat ve stok negatif olamaz.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Products
    (
        CategoryId,
        ProductName,
        Description,
        Price,
        Stock,
        ImageUrl,
        IsActive
    )
    VALUES
    (
        NULLIF(@KategoriId, 0),
        LTRIM(RTRIM(@UrunAdi)),
        NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(MAX), @Aciklama))), ''),
        @Fiyat,
        @Stok,
        NULLIF(LTRIM(RTRIM(@GorselUrl)), ''),
        @AktifMi
    );

    SELECT SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Urun_Guncelle
    @UrunId INT,
    @KategoriId INT,
    @UrunAdi NVARCHAR(200),
    @Aciklama NVARCHAR(MAX) = NULL,
    @Fiyat DECIMAL(18,2),
    @Stok INT,
    @GorselUrl NVARCHAR(500) = NULL,
    @AktifMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@UrunAdi)), '') IS NULL
    BEGIN
        RAISERROR('Ürün adı zorunludur.', 16, 1);
        RETURN;
    END

    IF @Fiyat < 0 OR @Stok < 0
    BEGIN
        RAISERROR('Fiyat ve stok negatif olamaz.', 16, 1);
        RETURN;
    END

    UPDATE dbo.Products
    SET
        CategoryId = NULLIF(@KategoriId, 0),
        ProductName = LTRIM(RTRIM(@UrunAdi)),
        Description = NULLIF(LTRIM(RTRIM(CONVERT(NVARCHAR(MAX), @Aciklama))), ''),
        Price = @Fiyat,
        Stock = @Stok,
        ImageUrl = NULLIF(LTRIM(RTRIM(@GorselUrl)), ''),
        IsActive = @AktifMi
    WHERE ProductId = @UrunId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Urun_DurumGuncelle
    @UrunId INT,
    @AktifMi BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Products
    SET IsActive = @AktifMi
    WHERE ProductId = @UrunId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Urun_Sil
    @UrunId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Products
    SET IsActive = 0
    WHERE ProductId = @UrunId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_SiparisIcin_Urun_Listele
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UrunId,
        KategoriId,
        KategoriAdi,
        UrunAdi,
        Aciklama,
        Fiyat,
        Stok,
        GorselUrl,
        UrunAdi + N' (Stok: ' + CONVERT(NVARCHAR(20), Stok) + N')' AS UrunGosterim
    FROM dbo.vw_UrunListesi
    WHERE AktifMi = 1
      AND Stok > 0
    ORDER BY UrunAdi;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Musteri_Listele
    @Arama NVARCHAR(200) = '',
    @Durum INT = -1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.CustomerId,
        c.FullName,
        c.CompanyName,
        c.AuthorizedPerson,
        c.Phone,
        c.Email,
        c.TaxNumber,
        c.TaxOffice,
        c.Address,
        c.CustomerType,
        c.IsActive,
        c.CreatedAt,
        ISNULL(storeStats.StoreCount, 0) AS StoreCount,
        ISNULL(orderStats.OrderCount, 0) AS OrderCount,
        ISNULL(orderStats.TotalRevenue, 0) AS TotalRevenue
    FROM dbo.Customers c
    OUTER APPLY
    (
        SELECT COUNT(*) AS StoreCount
        FROM dbo.CustomerStores cs
        WHERE cs.CustomerId = c.CustomerId
    ) storeStats
    OUTER APPLY
    (
        SELECT
            COUNT(*) AS OrderCount,
            SUM(ISNULL(NULLIF(o.GrandTotal, 0), ISNULL(o.TotalPrice, 0))) AS TotalRevenue
        FROM dbo.Orders o
        WHERE o.CustomerId = c.CustomerId
          AND ISNULL(o.IsCancelled, 0) = 0
          AND ISNULL(o.IsArchived, 0) = 0
    ) orderStats
    WHERE
        (@Durum = -1 OR c.IsActive = @Durum)
        AND
        (
            @Arama = ''
            OR c.FullName LIKE '%' + @Arama + '%'
            OR c.CompanyName LIKE '%' + @Arama + '%'
            OR c.AuthorizedPerson LIKE '%' + @Arama + '%'
            OR c.Phone LIKE '%' + @Arama + '%'
            OR c.Email LIKE '%' + @Arama + '%'
        )
    ORDER BY ISNULL(NULLIF(c.CompanyName, ''), c.FullName);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Musteri_Getir
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 *
    FROM
    (
        SELECT
            c.CustomerId,
            c.FullName,
            c.CompanyName,
            c.AuthorizedPerson,
            c.Phone,
            c.Email,
            c.TaxNumber,
            c.TaxOffice,
            c.Address,
            c.CustomerType,
            c.IsActive,
            c.CreatedAt
        FROM dbo.Customers c
    ) c
    WHERE CustomerId = @CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Musteri_Ekle
    @FullName NVARCHAR(300),
    @CompanyName NVARCHAR(300) = NULL,
    @AuthorizedPerson NVARCHAR(200) = NULL,
    @Phone NVARCHAR(60) = NULL,
    @Email NVARCHAR(400) = NULL,
    @TaxNumber NVARCHAR(50) = NULL,
    @TaxOffice NVARCHAR(100) = NULL,
    @Address NVARCHAR(500) = NULL,
    @CustomerType NVARCHAR(50) = N'Toptan',
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

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
    (
        LTRIM(RTRIM(@FullName)),
        NULLIF(LTRIM(RTRIM(@CompanyName)), ''),
        NULLIF(LTRIM(RTRIM(@AuthorizedPerson)), ''),
        NULLIF(LTRIM(RTRIM(@Phone)), ''),
        NULLIF(LTRIM(RTRIM(@Email)), ''),
        NULLIF(LTRIM(RTRIM(@TaxNumber)), ''),
        NULLIF(LTRIM(RTRIM(@TaxOffice)), ''),
        NULLIF(LTRIM(RTRIM(@Address)), ''),
        ISNULL(NULLIF(LTRIM(RTRIM(@CustomerType)), ''), N'Toptan'),
        @IsActive
    );

    SELECT SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Musteri_Guncelle
    @CustomerId INT,
    @FullName NVARCHAR(300),
    @CompanyName NVARCHAR(300) = NULL,
    @AuthorizedPerson NVARCHAR(200) = NULL,
    @Phone NVARCHAR(60) = NULL,
    @Email NVARCHAR(400) = NULL,
    @TaxNumber NVARCHAR(50) = NULL,
    @TaxOffice NVARCHAR(100) = NULL,
    @Address NVARCHAR(500) = NULL,
    @CustomerType NVARCHAR(50) = N'Toptan',
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Customers
    SET
        FullName = LTRIM(RTRIM(@FullName)),
        CompanyName = NULLIF(LTRIM(RTRIM(@CompanyName)), ''),
        AuthorizedPerson = NULLIF(LTRIM(RTRIM(@AuthorizedPerson)), ''),
        Phone = NULLIF(LTRIM(RTRIM(@Phone)), ''),
        Email = NULLIF(LTRIM(RTRIM(@Email)), ''),
        TaxNumber = NULLIF(LTRIM(RTRIM(@TaxNumber)), ''),
        TaxOffice = NULLIF(LTRIM(RTRIM(@TaxOffice)), ''),
        Address = NULLIF(LTRIM(RTRIM(@Address)), ''),
        CustomerType = ISNULL(NULLIF(LTRIM(RTRIM(@CustomerType)), ''), N'Toptan'),
        IsActive = @IsActive
    WHERE CustomerId = @CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Musteri_DurumGuncelle
    @CustomerId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Customers
    SET IsActive = @IsActive
    WHERE CustomerId = @CustomerId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MusteriMagaza_Listele
    @CustomerId INT,
    @Durum INT = -1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cs.CustomerStoreId,
        cs.CustomerId,
        cs.StoreName,
        cs.City,
        cs.District,
        cs.Address,
        cs.Phone,
        cs.ResponsiblePerson,
        cs.SorumluKullaniciId,
        COALESCE(NULLIF(k.AdSoyad, ''), NULLIF(cs.ResponsiblePerson, '')) AS SorumluKisi,
        cs.IsActive,
        cs.CreatedAt
    FROM dbo.CustomerStores cs
    LEFT JOIN dbo.Kullanicilar k
        ON k.KullaniciId = cs.SorumluKullaniciId
    WHERE cs.CustomerId = @CustomerId
      AND (@Durum = -1 OR cs.IsActive = @Durum)
    ORDER BY cs.StoreName;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MusteriMagaza_Getir
    @CustomerStoreId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        cs.CustomerStoreId,
        cs.CustomerId,
        cs.StoreName,
        cs.City,
        cs.District,
        cs.Address,
        cs.Phone,
        cs.ResponsiblePerson,
        cs.SorumluKullaniciId,
        COALESCE(NULLIF(k.AdSoyad, ''), NULLIF(cs.ResponsiblePerson, '')) AS SorumluKisi,
        cs.IsActive,
        cs.CreatedAt
    FROM dbo.CustomerStores cs
    LEFT JOIN dbo.Kullanicilar k
        ON k.KullaniciId = cs.SorumluKullaniciId
    WHERE cs.CustomerStoreId = @CustomerStoreId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MusteriMagaza_Ekle
    @CustomerId INT,
    @StoreName NVARCHAR(300),
    @City NVARCHAR(100) = NULL,
    @District NVARCHAR(100) = NULL,
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(60) = NULL,
    @ResponsiblePerson NVARCHAR(200) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

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
    (
        @CustomerId,
        LTRIM(RTRIM(@StoreName)),
        NULLIF(LTRIM(RTRIM(@City)), ''),
        NULLIF(LTRIM(RTRIM(@District)), ''),
        NULLIF(LTRIM(RTRIM(@Address)), ''),
        NULLIF(LTRIM(RTRIM(@Phone)), ''),
        NULLIF(LTRIM(RTRIM(@ResponsiblePerson)), ''),
        @IsActive
    );

    SELECT SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MusteriMagaza_Guncelle
    @CustomerStoreId INT,
    @StoreName NVARCHAR(300),
    @City NVARCHAR(100) = NULL,
    @District NVARCHAR(100) = NULL,
    @Address NVARCHAR(500) = NULL,
    @Phone NVARCHAR(60) = NULL,
    @ResponsiblePerson NVARCHAR(200) = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.CustomerStores
    SET
        StoreName = LTRIM(RTRIM(@StoreName)),
        City = NULLIF(LTRIM(RTRIM(@City)), ''),
        District = NULLIF(LTRIM(RTRIM(@District)), ''),
        Address = NULLIF(LTRIM(RTRIM(@Address)), ''),
        Phone = NULLIF(LTRIM(RTRIM(@Phone)), ''),
        ResponsiblePerson = NULLIF(LTRIM(RTRIM(@ResponsiblePerson)), ''),
        IsActive = @IsActive
    WHERE CustomerStoreId = @CustomerStoreId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_MusteriMagaza_DurumGuncelle
    @CustomerStoreId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.CustomerStores
    SET IsActive = @IsActive
    WHERE CustomerStoreId = @CustomerStoreId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_SonSiparisler_Getir
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 1,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 8
        SiparisId,
        MusteriAdi,
        MagazaAdi,
        YetkiliAdi,
        UrunAdi,
        Adet,
        ToplamTutar,
        SiparisDurumu,
        SiparisTarihi,
        OrderSource,
        OrderType
    FROM dbo.vw_Siparis_Liste
    WHERE IsArchived = 0
      AND (@TumMagazalar = 1 OR CustomerStoreId = @MagazaId)
      AND
      (
          @AdminMi = 1
          OR EXISTS
          (
              SELECT 1
              FROM dbo.KullaniciMagazalari km
              WHERE km.MagazaId = CustomerStoreId
                AND km.KullaniciId = @KullaniciId
                AND km.AktifMi = 1
          )
      )
    ORDER BY SiparisTarihi DESC, SiparisId DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Siparis_DurumOzet_Getir
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 1,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS ToplamSiparis,
        SUM(CASE WHEN SiparisDurumu = N'Hazirlaniyor' THEN 1 ELSE 0 END) AS HazirlaniyorSayisi,
        SUM(CASE WHEN SiparisDurumu = N'Kargoda' THEN 1 ELSE 0 END) AS KargodaSayisi,
        SUM(CASE WHEN SiparisDurumu = N'Teslim Edildi' THEN 1 ELSE 0 END) AS TeslimEdildiSayisi,
        SUM(CASE WHEN SiparisDurumu = N'Iptal' OR IsCancelled = 1 THEN 1 ELSE 0 END) AS IptalSayisi
    FROM dbo.vw_Siparis_Liste
    WHERE IsArchived = 0
      AND
      (
          @MagazaId IS NULL
          OR CustomerStoreId = @MagazaId
      )
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
                  WHERE km.MagazaId = CustomerStoreId
                    AND km.KullaniciId = @KullaniciId
                    AND km.AktifMi = 1
              )
          )
      );
END
GO
