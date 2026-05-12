IF OBJECT_ID('dbo.KullaniciMagazalari', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.KullaniciMagazalari
    (
        KullaniciMagazaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KullaniciMagazalari PRIMARY KEY,
        KullaniciId INT NOT NULL,
        MagazaId INT NOT NULL,
        Gorev NVARCHAR(30) NOT NULL CONSTRAINT DF_KullaniciMagazalari_Gorev DEFAULT (N'Personel'),
        AktifMi BIT NOT NULL CONSTRAINT DF_KullaniciMagazalari_AktifMi DEFAULT (1),
        OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_KullaniciMagazalari_OlusturmaTarihi DEFAULT (SYSDATETIME()),
        CONSTRAINT UQ_KullaniciMagazalari_Kullanici_Magaza UNIQUE (KullaniciId, MagazaId),
        CONSTRAINT FK_KullaniciMagazalari_Kullanicilar FOREIGN KEY (KullaniciId)
            REFERENCES dbo.Kullanicilar(KullaniciId),
        CONSTRAINT FK_KullaniciMagazalari_CustomerStores FOREIGN KEY (MagazaId)
            REFERENCES dbo.CustomerStores(CustomerStoreId)
    );
END
GO

IF COL_LENGTH('dbo.KullaniciMagazalari', 'Gorev') IS NULL
BEGIN
    ALTER TABLE dbo.KullaniciMagazalari
    ADD Gorev NVARCHAR(30) NOT NULL
        CONSTRAINT DF_KullaniciMagazalari_Gorev DEFAULT (N'Personel');
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Magaza_Secim_Listele
    @Arama NVARCHAR(200) = '',
    @SadeceAktif BIT = 1,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
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
        m.MagazaMuduru,
        m.Supervisor,
        m.PersonelSayisi,
        m.SiparisYetkilisiSayisi,
        m.MusteriTipi,
        m.MusteriAktifMi,
        m.MagazaAktifMi,
        m.SiparisSayisi,
        m.ToplamCiro,
        m.SonSiparisTarihi
    FROM dbo.vw_Magaza_Secim m
    WHERE
        (
            @Arama = ''
            OR m.MusteriAdi LIKE '%' + @Arama + '%'
            OR m.MagazaAdi LIKE '%' + @Arama + '%'
            OR m.Sehir LIKE '%' + @Arama + '%'
            OR m.Ilce LIKE '%' + @Arama + '%'
            OR m.Telefon LIKE '%' + @Arama + '%'
            OR m.SorumluKisi LIKE '%' + @Arama + '%'
        )
        AND
        (
            @SadeceAktif = 0
            OR
            (
                m.MusteriAktifMi = 1
                AND m.MagazaAktifMi = 1
            )
        )
        AND
        (
            @AdminMi = 1
            OR @KullaniciId IS NULL
            OR EXISTS
            (
                SELECT 1
                FROM dbo.KullaniciMagazalari km
                WHERE km.MagazaId = m.MagazaId
                  AND km.KullaniciId = @KullaniciId
                  AND km.AktifMi = 1
            )
        )
    ORDER BY m.MusteriAdi, m.MagazaAdi;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Magaza_Secim_Getir
    @MagazaId INT,
    @KullaniciId INT = NULL,
    @AdminMi BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        m.MusteriId,
        m.MagazaId,
        m.MusteriAdi,
        m.MagazaAdi,
        m.Sehir,
        m.Ilce,
        m.Telefon,
        m.SorumluKisi,
        m.MagazaMuduru,
        m.Supervisor,
        m.PersonelSayisi,
        m.SiparisYetkilisiSayisi,
        m.MusteriTipi,
        m.MusteriAktifMi,
        m.MagazaAktifMi,
        m.SiparisSayisi,
        m.ToplamCiro,
        m.SonSiparisTarihi
    FROM dbo.vw_Magaza_Secim m
    WHERE m.MagazaId = @MagazaId
      AND
      (
            @AdminMi = 1
            OR @KullaniciId IS NULL
            OR EXISTS
            (
                SELECT 1
                FROM dbo.KullaniciMagazalari km
                WHERE km.MagazaId = m.MagazaId
                  AND km.KullaniciId = @KullaniciId
                  AND km.AktifMi = 1
            )
      );
END
GO
