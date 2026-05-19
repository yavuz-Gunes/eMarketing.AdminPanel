CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_Ozet_Getir
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 0,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Siparisler AS
    (
        SELECT *
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
    ),
    Magazalar AS
    (
        SELECT *
        FROM dbo.vw_Magaza_Secim ms
        WHERE (@TumMagazalar = 1 OR ms.MagazaId = @MagazaId)
          AND
          (
              @AdminMi = 1
              OR EXISTS
              (
                  SELECT 1
                  FROM dbo.KullaniciMagazalari km
                  WHERE km.MagazaId = ms.MagazaId
                    AND km.KullaniciId = @KullaniciId
                    AND km.AktifMi = 1
              )
          )
    ),
    MagazaStoklar AS
    (
        SELECT *
        FROM dbo.vw_MagazaStok_Liste msl
        WHERE (@TumMagazalar = 1 OR msl.MagazaId = @MagazaId)
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
          )
    )
    SELECT
        (SELECT COUNT(DISTINCT UrunId) FROM MagazaStoklar) AS ToplamUrun,
        (SELECT COUNT(DISTINCT UrunId) FROM MagazaStoklar WHERE AktifMi = 1) AS AktifUrun,
        (
            SELECT COUNT(*)
            FROM MagazaStoklar
            WHERE AktifMi = 1
              AND BayiStok > 0
              AND
              (
                  StokDurumu = N'Kritik'
                  OR (MinimumStok <= 0 AND BayiStok <= 5)
              )
        ) AS KritikStok,
        (SELECT COUNT(DISTINCT KategoriId) FROM MagazaStoklar WHERE KategoriId IS NOT NULL) AS ToplamKategori,
        (SELECT COUNT(DISTINCT KategoriId) FROM MagazaStoklar WHERE KategoriId IS NOT NULL AND AktifMi = 1) AS AktifKategori,

        (SELECT COUNT(*) FROM Siparisler) AS ToplamSiparis,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Hazirlaniyor') AS HazirlaniyorSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Kargoda') AS KargodaSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Teslim Edildi') AS TeslimEdildiSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE SiparisDurumu = N'Iptal' OR IsCancelled = 1) AS IptalSayisi,
        (SELECT COUNT(*) FROM Siparisler WHERE PaymentStatus = N'Bekliyor') AS BekleyenOdemeSayisi,

        (SELECT COUNT(DISTINCT CustomerId) FROM Siparisler WHERE CustomerId IS NOT NULL) AS ToplamMusteri,
        (SELECT COUNT(*) FROM Magazalar WHERE MagazaAktifMi = 1 AND MusteriAktifMi = 1) AS AktifMagaza,
        (
            SELECT COUNT(DISTINCT km.KullaniciId)
            FROM dbo.KullaniciMagazalari km
            INNER JOIN Magazalar m
                ON m.MagazaId = km.MagazaId
            INNER JOIN dbo.Kullanicilar k
                ON k.KullaniciId = km.KullaniciId
            WHERE km.AktifMi = 1
              AND k.AktifMi = 1
        ) AS PersonelSayisi,

        ISNULL((SELECT SUM(ToplamTutar) FROM Siparisler WHERE IsCancelled = 0), 0) AS ToplamCiro,
        ISNULL((SELECT SUM(ToplamTutar) FROM Siparisler WHERE IsCancelled = 0 AND CAST(SiparisTarihi AS DATE) = CAST(GETDATE() AS DATE)), 0) AS BugunkuCiro,
        ISNULL((SELECT SUM(ToplamTutar) FROM Siparisler WHERE IsCancelled = 0 AND YEAR(SiparisTarihi) = YEAR(GETDATE()) AND MONTH(SiparisTarihi) = MONTH(GETDATE())), 0) AS AylikCiro;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Dashboard_KritikStok_Getir
    @MagazaId INT = NULL,
    @TumMagazalar BIT = 0,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 10
        msl.UrunId,
        msl.UrunAdi,
        msl.KategoriAdi,
        msl.BayiStok AS Stok,
        msl.Fiyat
    FROM dbo.vw_MagazaStok_Liste msl
    WHERE msl.AktifMi = 1
      AND msl.BayiStok > 0
      AND
      (
          msl.StokDurumu = N'Kritik'
          OR (msl.MinimumStok <= 0 AND msl.BayiStok <= 5)
      )
      AND
      (
            @TumMagazalar = 1
            OR msl.MagazaId = @MagazaId
      )
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
      )
    ORDER BY msl.BayiStok ASC, msl.UrunAdi ASC;
END
GO
