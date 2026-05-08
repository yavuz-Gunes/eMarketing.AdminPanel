CREATE OR ALTER VIEW dbo.vw_Siparis_Liste
AS
SELECT
    o.OrderId AS SiparisId,
    o.OrderNo,

    o.CustomerId,
    o.CustomerStoreId,
    o.BayiYetkiliId,

    ISNULL(NULLIF(c.CompanyName, ''), ISNULL(c.FullName, o.CustomerName)) AS MusteriAdi,
    ISNULL(c.Email, o.CustomerEmail) AS MusteriEmail,
    ISNULL(c.Phone, o.CustomerPhone) AS MusteriTelefon,

    cs.StoreName AS MagazaAdi,
    cs.City AS Sehir,
    cs.District AS Ilce,

    byk.AdSoyad AS YetkiliAdi,
    byk.Telefon AS YetkiliTelefon,
    byk.Email AS YetkiliEmail,
    COALESCE(itemStats.FirstImageUrl, p.ImageUrl) AS GorselUrl,

    CASE
        WHEN itemStats.ItemCount IS NULL OR itemStats.ItemCount = 0 THEN p.ProductName
        WHEN itemStats.ItemCount = 1 THEN itemStats.FirstProductName
        ELSE CAST(itemStats.ItemCount AS NVARCHAR(20)) + N' ürün'
    END AS UrunAdi,

    ISNULL(itemStats.TotalQuantity, o.Quantity) AS Adet,
    ISNULL(itemStats.ItemCount, 1) AS UrunKalemi,
    ISNULL(bayiStok.BayiStok, 0) AS BayiStok,

    ISNULL(NULLIF(o.GrandTotal, 0), ISNULL(o.TotalPrice, 0)) AS ToplamTutar,

    ISNULL(o.PaymentStatus, N'Bekliyor') AS PaymentStatus,
    ISNULL(o.OrderType, N'Toptan') AS OrderType,
    ISNULL(o.OrderSource, N'Admin') AS OrderSource,

    o.OrderStatus AS SiparisDurumu,
    o.OrderDate AS SiparisTarihi,

    ISNULL(o.IsCancelled, 0) AS IsCancelled,
    ISNULL(o.IsArchived, 0) AS IsArchived
FROM dbo.Orders o
LEFT JOIN dbo.Customers c
    ON c.CustomerId = o.CustomerId
LEFT JOIN dbo.CustomerStores cs
    ON cs.CustomerStoreId = o.CustomerStoreId
LEFT JOIN dbo.BayiYetkilileri byk
    ON byk.BayiYetkiliId = o.BayiYetkiliId
LEFT JOIN dbo.Products p
    ON p.ProductId = o.ProductId
OUTER APPLY
(
    SELECT
        COUNT(*) AS ItemCount,
        SUM(oi.Quantity) AS TotalQuantity,
        MIN(pr.ProductName) AS FirstProductName,
        MIN(pr.ImageUrl) AS FirstImageUrl
    FROM dbo.OrderItems oi
    INNER JOIN dbo.Products pr
        ON pr.ProductId = oi.ProductId
    WHERE oi.OrderId = o.OrderId
) itemStats
OUTER APPLY
(
    SELECT SUM(ISNULL(ms.StokAdedi, 0)) AS BayiStok
    FROM
    (
        SELECT oi.ProductId
        FROM dbo.OrderItems oi
        WHERE oi.OrderId = o.OrderId

        UNION ALL

        SELECT o.ProductId
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.OrderItems oi2
            WHERE oi2.OrderId = o.OrderId
        )
    ) urunler
    LEFT JOIN dbo.MagazaStoklari ms
        ON ms.MagazaId = o.CustomerStoreId
       AND ms.ProductId = urunler.ProductId
) bayiStok;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Siparis_Listele
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SiparisId,
        OrderNo,
        CustomerId,
        CustomerStoreId,
        BayiYetkiliId,
        MusteriAdi,
        MusteriEmail,
        MusteriTelefon,
        MagazaAdi,
        Sehir,
        Ilce,
        YetkiliAdi,
        YetkiliTelefon,
        YetkiliEmail,
        GorselUrl,
        UrunAdi,
        Adet,
        UrunKalemi,
        BayiStok,
        ToplamTutar,
        PaymentStatus,
        OrderType,
        OrderSource,
        SiparisDurumu,
        SiparisTarihi,
        IsCancelled,
        IsArchived
    FROM dbo.vw_Siparis_Liste
    WHERE IsArchived = 0
      AND
      (
          @TumMagazalar = 1
          OR CustomerStoreId = @MagazaId
      )
    ORDER BY SiparisId DESC;
END
GO
