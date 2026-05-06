CREATE OR ALTER VIEW dbo.vw_Kullanici_Liste
AS
SELECT
    k.KullaniciId,
    k.KullaniciAdi,
    k.AdSoyad,
    k.Rol,
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
    m.MusteriId,
    m.MusteriAdi,
    m.MagazaAdi,
    m.Sehir,
    m.Ilce,
    m.Telefon,
    m.SorumluKisi,
    m.SiparisSayisi,
    m.ToplamCiro,
    km.AktifMi,
    km.OlusturmaTarihi
FROM dbo.KullaniciMagazalari km
INNER JOIN dbo.Kullanicilar k
    ON k.KullaniciId = km.KullaniciId
INNER JOIN dbo.vw_Magaza_Secim m
    ON m.MagazaId = km.MagazaId;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kullanici_Listele
    @Arama NVARCHAR(200) = '',
    @SadeceAktif BIT = 0,
    @GoruntuleyenKullaniciId INT = NULL,
    @AdminMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        k.KullaniciId,
        k.KullaniciAdi,
        k.AdSoyad,
        k.Rol,
        k.AktifMi,
        k.OlusturmaTarihi,
        k.MagazaSayisi
    FROM dbo.vw_Kullanici_Liste k
    WHERE
        (
            @Arama = ''
            OR k.KullaniciAdi LIKE '%' + @Arama + '%'
            OR k.AdSoyad LIKE '%' + @Arama + '%'
            OR k.Rol LIKE '%' + @Arama + '%'
        )
        AND (@SadeceAktif = 0 OR k.AktifMi = 1)
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
    @AktifMi BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

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
            AktifMi
        )
        VALUES
        (
            @KullaniciAdi,
            ISNULL(NULLIF(@Sifre, ''), '1234'),
            @AdSoyad,
            @Rol,
            @AktifMi
        );

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
        AktifMi = @AktifMi
    WHERE KullaniciId = @KullaniciId;

    SELECT @KullaniciId AS KullaniciId;
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
        MusteriAdi,
        MagazaAdi,
        Sehir,
        Ilce,
        Telefon,
        SorumluKisi,
        SiparisSayisi,
        ToplamCiro,
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
    @MagazaId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.KullaniciMagazalari
        WHERE KullaniciId = @KullaniciId
          AND MagazaId = @MagazaId
    )
    BEGIN
        UPDATE dbo.KullaniciMagazalari
        SET AktifMi = 1
        WHERE KullaniciId = @KullaniciId
          AND MagazaId = @MagazaId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.KullaniciMagazalari (KullaniciId, MagazaId, AktifMi)
        VALUES (@KullaniciId, @MagazaId, 1);
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_KullaniciMagaza_Kaldir
    @KullaniciMagazaId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.KullaniciMagazalari
    SET AktifMi = 0
    WHERE KullaniciMagazaId = @KullaniciMagazaId;
END
GO
