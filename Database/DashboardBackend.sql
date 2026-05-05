ALTER PROCEDURE dbo.sp_Dashboard_Ozet_Getir
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Siparisler AS
    (
        SELECT *
        FROM dbo.vw_Siparis_Liste
        WHERE IsArchived = 0
          AND (@TumMagazalar = 1 OR CustomerStoreId = @MagazaId)
    ),
    Magazalar AS
    (
        SELECT *
        FROM dbo.vw_Magaza_Secim
        WHERE (@TumMagazalar = 1 OR MagazaId = @MagazaId)
    )
    SELECT
        (SELECT COUNT(*) FROM dbo.Products) AS ToplamUrun,
        (SELECT COUNT(*) FROM dbo.Products WHERE IsActive = 1) AS AktifUrun,
        (SELECT COUNT(*) FROM dbo.Products WHERE IsActive = 1 AND Stock > 0 AND Stock <= 5) AS KritikStok,
        (SELECT COUNT(*) FROM dbo.Categories) AS ToplamKategori,
        (SELECT COUNT(*) FROM dbo.Categories WHERE IsActive = 1) AS AktifKategori,

        (SELECT COUNT(*) FROM Siparisler) AS ToplamSiparis,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Hazirlaniyor') AS HazirlaniyorSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Kargoda') AS KargodaSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Teslim Edildi') AS TeslimEdildiSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Iptal' OR IsCancelled = 1) AS IptalSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE PaymentStatus = N'Bekliyor') AS BekleyenOdemeSayisi,

        (SELECT COUNT(DISTINCT CustomerId) FROM Siparisler WHERE CustomerId IS NOT NULL) AS ToplamMusteri,
        (SELECT COUNT(*) FROM Magazalar WHERE MagazaAktifMi = 1 AND MusteriAktifMi = 1) AS AktifMagaza,
        (SELECT COUNT(*) FROM Magazalar WHERE NULLIF(LTRIM(RTRIM(SorumluKisi)), N'') IS NOT NULL) AS PersonelSayisi,

        ISNULL((SELECT SUM(ToplamTutar) FROM Siparisler WHERE IsCancelled = 0), 0) AS ToplamCiro,
        ISNULL((SELECT SUM(ToplamTutar) FROM Siparisler WHERE IsCancelled = 0 AND CAST(SiparisTarihi AS DATE) = CAST(GETDATE() AS DATE)), 0) AS BugunkuCiro,
        ISNULL((SELECT SUM(ToplamTutar) FROM Siparisler WHERE IsCancelled = 0 AND YEAR(SiparisTarihi) = YEAR(GETDATE()) AND MONTH(SiparisTarihi) = MONTH(GETDATE())), 0) AS AylikCiro;
END
GO

ALTER PROCEDURE dbo.sp_Dashboard_KritikStok_Getir
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 10
        p.ProductId AS UrunId,
        p.ProductName AS UrunAdi,
        c.CategoryName AS KategoriAdi,
        p.Stock AS Stok,
        p.Price AS Fiyat
    FROM dbo.Products p
    LEFT JOIN dbo.Categories c
        ON c.CategoryId = p.CategoryId
    WHERE p.IsActive = 1
      AND p.Stock > 0
      AND p.Stock <= 5
      AND
      (
            @TumMagazalar = 1
            OR EXISTS
            (
                SELECT 1
                FROM dbo.vw_Siparis_Liste s
                WHERE s.CustomerStoreId = @MagazaId
                  AND s.IsArchived = 0
                  AND
                  (
                        s.UrunAdi = p.ProductName
                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.Orders o
                            WHERE o.OrderId = s.SiparisId
                              AND o.ProductId = p.ProductId
                        )
                        OR EXISTS
                        (
                            SELECT 1
                            FROM dbo.OrderItems oi
                            WHERE oi.OrderId = s.SiparisId
                              AND oi.ProductId = p.ProductId
                        )
                  )
            )
      )
    ORDER BY p.Stock ASC, p.ProductName ASC;
END
GO
