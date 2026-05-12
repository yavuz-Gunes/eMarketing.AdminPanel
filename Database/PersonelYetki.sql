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

IF COL_LENGTH('dbo.KullaniciMagazalari', 'Gorev') IS NULL
BEGIN
    ALTER TABLE dbo.KullaniciMagazalari
    ADD Gorev NVARCHAR(30) NOT NULL
        CONSTRAINT DF_KullaniciMagazalari_Gorev DEFAULT (N'Personel');
END
GO

CREATE OR ALTER VIEW dbo.vw_Kullanici_Liste
AS
SELECT
    k.KullaniciId,
    k.KullaniciAdi,
    k.AdSoyad,
    k.Rol,
    k.Telefon,
    k.Email,
    k.ImageUrl,
    k.AktifMi,
    k.OlusturmaTarihi,
    ISNULL(magazaOzet.MagazaSayisi, 0) AS MagazaSayisi
FROM dbo.Kullanicilar k
OUTER APPLY
(
    SELECT COUNT(*) AS MagazaSayisi
    FROM dbo.KullaniciMagazalari km
    INNER JOIN dbo.CustomerStores cs
        ON cs.CustomerStoreId = km.MagazaId
    WHERE km.KullaniciId = k.KullaniciId
      AND km.AktifMi = 1
      AND cs.IsActive = 1
) magazaOzet;
GO

CREATE OR ALTER VIEW dbo.vw_KullaniciMagaza_Liste
AS
SELECT
    km.KullaniciMagazaId,
    km.KullaniciId,
    k.KullaniciAdi,
    k.AdSoyad,
    k.Rol,
    km.MagazaId,
    km.Gorev,
    CASE
        WHEN km.Gorev = N'MagazaMuduru' THEN N'Mağaza Müdürü'
        WHEN km.Gorev = N'Supervisor' THEN N'Supervisor'
        ELSE N'Personel'
    END AS GorevGorunenAd,
    m.MusteriId,
    m.MusteriAdi,
    m.MagazaAdi,
    m.Sehir,
    m.Ilce,
    m.Telefon,
    m.SorumluKisi,
    m.SiparisSayisi,
    m.ToplamCiro,
    siparisYetki.BayiYetkiliId,
    CAST(CASE WHEN siparisYetki.BayiYetkiliId IS NULL THEN 0 ELSE 1 END AS BIT) AS SiparisYetkilisiMi,
    km.AktifMi,
    km.OlusturmaTarihi
FROM dbo.KullaniciMagazalari km
INNER JOIN dbo.Kullanicilar k
    ON k.KullaniciId = km.KullaniciId
INNER JOIN dbo.vw_Magaza_Secim m
    ON m.MagazaId = km.MagazaId
OUTER APPLY
(
    SELECT TOP 1 byk.BayiYetkiliId
    FROM dbo.BayiYetkilileri byk
    WHERE byk.KullaniciId = km.KullaniciId
      AND byk.MagazaId = km.MagazaId
      AND byk.AktifMi = 1
      AND byk.YetkiTipi = N'SiparisYetkilisi'
    ORDER BY byk.BayiYetkiliId
) siparisYetki
;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kullanici_Listele
    @Arama NVARCHAR(200) = '',
    @SadeceAktif BIT = 0,
    @GoruntuleyenKullaniciId INT = NULL,
    @AdminMi BIT = 1,
    @MagazaId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        k.KullaniciId,
        k.KullaniciAdi,
        k.AdSoyad,
        k.Rol,
        k.Telefon,
        k.Email,
        k.ImageUrl,
        k.AktifMi,
        k.OlusturmaTarihi,
        k.MagazaSayisi,
        ISNULL(aktifMagazaGorev.Gorev, N'') AS AktifMagazaGorev,
        ISNULL(aktifMagazaGorev.GorevGorunenAd, N'') AS AktifMagazaGorevGorunenAd,
        CAST(CASE WHEN ISNULL(siparisYetki.SiparisYetkiliMagazaSayisi, 0) > 0 THEN 1 ELSE 0 END AS BIT) AS SiparisYetkilisiMi,
        ISNULL(siparisYetki.SiparisYetkiliMagazaSayisi, 0) AS SiparisYetkiliMagazaSayisi
    FROM dbo.vw_Kullanici_Liste k
    OUTER APPLY
    (
        SELECT TOP 1
            km.Gorev,
            CASE
                WHEN km.Gorev = N'MagazaMuduru' THEN N'Mağaza Müdürü'
                WHEN km.Gorev = N'Supervisor' THEN N'Supervisor'
                ELSE N'Personel'
            END AS GorevGorunenAd
        FROM dbo.KullaniciMagazalari km
        WHERE km.KullaniciId = k.KullaniciId
          AND km.AktifMi = 1
          AND (@MagazaId IS NULL OR km.MagazaId = @MagazaId)
        ORDER BY CASE WHEN @MagazaId IS NOT NULL AND km.MagazaId = @MagazaId THEN 0 ELSE 1 END, km.KullaniciMagazaId
    ) aktifMagazaGorev
    OUTER APPLY
    (
        SELECT COUNT(DISTINCT byk.MagazaId) AS SiparisYetkiliMagazaSayisi
        FROM dbo.BayiYetkilileri byk
        INNER JOIN dbo.KullaniciMagazalari km
            ON km.KullaniciId = byk.KullaniciId
           AND km.MagazaId = byk.MagazaId
           AND km.AktifMi = 1
        WHERE byk.KullaniciId = k.KullaniciId
          AND byk.AktifMi = 1
          AND byk.YetkiTipi = N'SiparisYetkilisi'
          AND (@MagazaId IS NULL OR byk.MagazaId = @MagazaId)
    ) siparisYetki
    WHERE
        (
            @Arama = ''
            OR k.KullaniciAdi LIKE '%' + @Arama + '%'
            OR k.AdSoyad LIKE '%' + @Arama + '%'
            OR k.Rol LIKE '%' + @Arama + '%'
        )
        AND (@SadeceAktif = 0 OR k.AktifMi = 1)
        AND (@MagazaId IS NULL OR k.Rol <> N'Admin')
        AND
        (
            @MagazaId IS NULL
            OR EXISTS
            (
                SELECT 1
                FROM dbo.KullaniciMagazalari magazaFiltre
                WHERE magazaFiltre.KullaniciId = k.KullaniciId
                  AND magazaFiltre.MagazaId = @MagazaId
                  AND magazaFiltre.AktifMi = 1
            )
        )
        AND
        (
            @AdminMi = 1
            OR
            (
                k.Rol <> N'Admin'
                AND
                (
                    k.KullaniciId = @GoruntuleyenKullaniciId
                    OR EXISTS
                    (
                        SELECT 1
                        FROM dbo.KullaniciMagazalari hedefKm
                        INNER JOIN dbo.CustomerStores hedefMagaza
                            ON hedefMagaza.CustomerStoreId = hedefKm.MagazaId
                        INNER JOIN dbo.KullaniciMagazalari aktifKm
                            ON aktifKm.KullaniciId = @GoruntuleyenKullaniciId
                           AND aktifKm.AktifMi = 1
                        INNER JOIN dbo.CustomerStores aktifMagaza
                            ON aktifMagaza.CustomerStoreId = aktifKm.MagazaId
                        WHERE hedefKm.KullaniciId = k.KullaniciId
                          AND hedefKm.AktifMi = 1
                          AND hedefMagaza.CustomerId = aktifMagaza.CustomerId
                    )
                )
            )
        )
    ORDER BY k.AktifMi DESC, k.AdSoyad, k.KullaniciAdi;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kullanici_Kaydet
    @KullaniciId INT = NULL,
    @KullaniciAdi NVARCHAR(100),
    @Sifre NVARCHAR(100) = NULL,
    @AdSoyad NVARCHAR(150),
    @Rol NVARCHAR(50),
    @Telefon NVARCHAR(60) = NULL,
    @Email NVARCHAR(400) = NULL,
    @ImageUrl NVARCHAR(500) = NULL,
    @AktifMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

    IF @KullaniciId IS NULL OR @KullaniciId = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.Kullanicilar WHERE KullaniciAdi = @KullaniciAdi)
            THROW 51000, 'Bu kullanıcı adı zaten kullanılıyor.', 1;

        INSERT INTO dbo.Kullanicilar
        (
            KullaniciAdi,
            Sifre,
            AdSoyad,
            Rol,
            Telefon,
            Email,
            ImageUrl,
            AktifMi
        )
        VALUES
        (
            @KullaniciAdi,
            ISNULL(NULLIF(@Sifre, ''), '1234'),
            @AdSoyad,
            @Rol,
            NULLIF(LTRIM(RTRIM(@Telefon)), ''),
            NULLIF(LTRIM(RTRIM(@Email)), ''),
            NULLIF(LTRIM(RTRIM(@ImageUrl)), ''),
            @AktifMi
        );

        COMMIT TRANSACTION;
        SELECT CONVERT(INT, SCOPE_IDENTITY()) AS KullaniciId;
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Kullanicilar
        WHERE KullaniciAdi = @KullaniciAdi
          AND KullaniciId <> @KullaniciId
    )
        THROW 51001, 'Bu kullanıcı adı başka bir personelde kullanılıyor.', 1;

    UPDATE dbo.Kullanicilar
    SET
        KullaniciAdi = @KullaniciAdi,
        Sifre = CASE WHEN NULLIF(@Sifre, '') IS NULL THEN Sifre ELSE @Sifre END,
        AdSoyad = @AdSoyad,
        Rol = @Rol,
        Telefon = NULLIF(LTRIM(RTRIM(@Telefon)), ''),
        Email = NULLIF(LTRIM(RTRIM(@Email)), ''),
        ImageUrl = NULLIF(LTRIM(RTRIM(@ImageUrl)), ''),
        AktifMi = @AktifMi
    WHERE KullaniciId = @KullaniciId;

    IF @AktifMi = 0
    BEGIN
        UPDATE dbo.KullaniciMagazalari
        SET AktifMi = 0
        WHERE KullaniciId = @KullaniciId
          AND AktifMi = 1;

        UPDATE dbo.BayiYetkilileri
        SET AktifMi = 0,
            GuncellemeTarihi = SYSDATETIME()
        WHERE KullaniciId = @KullaniciId
          AND AktifMi = 1;
    END

    COMMIT TRANSACTION;
    SELECT @KullaniciId AS KullaniciId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_KullaniciMagaza_Listele
    @KullaniciId INT,
    @GoruntuleyenKullaniciId INT = NULL,
    @AdminMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        KullaniciMagazaId,
        KullaniciId,
        MagazaId,
        Gorev,
        GorevGorunenAd,
        MusteriId,
        MusteriAdi,
        MagazaAdi,
        Sehir,
        Ilce,
        Telefon,
        SorumluKisi,
        SiparisSayisi,
        ToplamCiro,
        BayiYetkiliId,
        SiparisYetkilisiMi,
        AktifMi,
        OlusturmaTarihi
    FROM dbo.vw_KullaniciMagaza_Liste v
    WHERE v.KullaniciId = @KullaniciId
      AND v.AktifMi = 1
      AND
      (
          @AdminMi = 1
          OR EXISTS
          (
              SELECT 1
              FROM dbo.KullaniciMagazalari aktifKm
              INNER JOIN dbo.CustomerStores aktifMagaza
                  ON aktifMagaza.CustomerStoreId = aktifKm.MagazaId
              INNER JOIN dbo.CustomerStores hedefMagaza
                  ON hedefMagaza.CustomerStoreId = v.MagazaId
              WHERE aktifKm.KullaniciId = @GoruntuleyenKullaniciId
                AND aktifKm.AktifMi = 1
                AND hedefMagaza.CustomerId = aktifMagaza.CustomerId
          )
      )
    ORDER BY v.MusteriAdi, v.MagazaAdi;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_KullaniciMagaza_AtanmamisMagaza_Listele
    @KullaniciId INT,
    @Arama NVARCHAR(200) = '',
    @GoruntuleyenKullaniciId INT = NULL,
    @AdminMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.MusteriId,
        m.MagazaId,
        N'Personel' AS Gorev,
        N'Personel' AS GorevGorunenAd,
        m.MusteriAdi,
        m.MagazaAdi,
        m.Sehir,
        m.Ilce,
        m.Telefon,
        m.SorumluKisi,
        m.SiparisSayisi,
        m.ToplamCiro
    FROM dbo.vw_Magaza_Secim m
    WHERE m.MusteriAktifMi = 1
      AND m.MagazaAktifMi = 1
      AND
      (
            @Arama = ''
            OR m.MusteriAdi LIKE '%' + @Arama + '%'
            OR m.MagazaAdi LIKE '%' + @Arama + '%'
            OR m.Sehir LIKE '%' + @Arama + '%'
      )
      AND
      (
            @AdminMi = 1
            OR EXISTS
            (
                SELECT 1
                FROM dbo.KullaniciMagazalari aktifKm
                INNER JOIN dbo.CustomerStores aktifMagaza
                    ON aktifMagaza.CustomerStoreId = aktifKm.MagazaId
                WHERE aktifKm.KullaniciId = @GoruntuleyenKullaniciId
                  AND aktifKm.AktifMi = 1
                  AND aktifMagaza.CustomerId = m.MusteriId
            )
      )
      AND NOT EXISTS
      (
            SELECT 1
            FROM dbo.KullaniciMagazalari km
            WHERE km.KullaniciId = @KullaniciId
              AND km.MagazaId = m.MagazaId
              AND km.AktifMi = 1
      )
    ORDER BY m.MusteriAdi, m.MagazaAdi;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_KullaniciMagaza_Ata
    @KullaniciId INT,
    @MagazaId INT,
    @Gorev NVARCHAR(30) = N'Personel'
AS
BEGIN
    SET NOCOUNT ON;

    SET @Gorev = CASE
        WHEN @Gorev IN (N'MagazaMuduru', N'Supervisor', N'Personel') THEN @Gorev
        ELSE N'Personel'
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.KullaniciMagazalari
        WHERE KullaniciId = @KullaniciId
          AND MagazaId = @MagazaId
    )
    BEGIN
        UPDATE dbo.KullaniciMagazalari
        SET AktifMi = 1,
            Gorev = @Gorev
        WHERE KullaniciId = @KullaniciId
          AND MagazaId = @MagazaId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.KullaniciMagazalari (KullaniciId, MagazaId, Gorev, AktifMi)
        VALUES (@KullaniciId, @MagazaId, @Gorev, 1);
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_KullaniciMagaza_GorevGuncelle
    @KullaniciMagazaId INT,
    @Gorev NVARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SET @Gorev = CASE
        WHEN @Gorev IN (N'MagazaMuduru', N'Supervisor', N'Personel') THEN @Gorev
        ELSE N'Personel'
    END;

    UPDATE dbo.KullaniciMagazalari
    SET Gorev = @Gorev
    WHERE KullaniciMagazaId = @KullaniciMagazaId
      AND AktifMi = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_KullaniciMagaza_Kaldir
    @KullaniciMagazaId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @KullaniciId INT;
        DECLARE @MagazaId INT;

        SELECT
            @KullaniciId = KullaniciId,
            @MagazaId = MagazaId
        FROM dbo.KullaniciMagazalari
        WHERE KullaniciMagazaId = @KullaniciMagazaId;

        UPDATE dbo.KullaniciMagazalari
        SET AktifMi = 0
        WHERE KullaniciMagazaId = @KullaniciMagazaId;

        UPDATE dbo.BayiYetkilileri
        SET AktifMi = 0,
            GuncellemeTarihi = SYSDATETIME()
        WHERE KullaniciId = @KullaniciId
          AND MagazaId = @MagazaId
          AND AktifMi = 1;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO
