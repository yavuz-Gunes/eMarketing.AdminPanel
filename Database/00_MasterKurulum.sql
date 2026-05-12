/*
    eMarketing veritabanı kurulum sırası.

    Not: Bu dosyayı SSMS içinde SQLCMD Mode açıkken çalıştırın.
    Mevcut çalışan veriyi silen demo scripti en sonda ve bilinçli şekilde tutulur.
*/

:r .\BayiYetkiliBackend.sql
:r .\MagazaYetki.sql
:r .\SiparisListeBackend.sql
:r .\SiparisKanalSozlesmesi.sql
:r .\MagazaStokBackend.sql
:r .\DashboardBackend.sql
:r .\04_AuthSecurity.sql
:r .\PersonelYetki.sql
:r .\03_StoredProcedures_Eksikler.sql

-- Demo veriyi sıfırlamak isterseniz ayrı çalıştırın:
-- :r .\DemoVeriTemizKurulum.sql
