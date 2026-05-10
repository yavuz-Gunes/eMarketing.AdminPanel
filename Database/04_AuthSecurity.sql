IF COL_LENGTH('dbo.Kullanicilar', 'PasswordHashVersion') IS NULL
BEGIN
    ALTER TABLE dbo.Kullanicilar
    ADD PasswordHashVersion INT NOT NULL CONSTRAINT DF_Kullanicilar_PasswordHashVersion DEFAULT (0);
END
GO

ALTER TABLE dbo.Kullanicilar
ALTER COLUMN Sifre NVARCHAR(100) NOT NULL;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kullanici_LoginBilgisi_Getir
    @KullaniciAdi NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        KullaniciId,
        KullaniciAdi,
        Sifre,
        AdSoyad,
        Rol
    FROM dbo.Kullanicilar
    WHERE KullaniciAdi = LTRIM(RTRIM(@KullaniciAdi))
      AND AktifMi = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Kullanici_SifreHash_Guncelle
    @KullaniciId INT,
    @Sifre NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Kullanicilar
    SET
        Sifre = @Sifre,
        PasswordHashVersion = 1
    WHERE KullaniciId = @KullaniciId;
END
GO
