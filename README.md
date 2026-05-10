# eMarketing

eMarketing, otomotiv yedek parca satisi yapan bayi/ana firma senaryosu icin hazirlanmis bir okul projesidir. Proje Windows Forms admin paneli, ASP.NET Core Web API, class library servis katmani ve SQL Server stored procedure scriptlerinden olusur.

## Mimari

```text
Windows Forms Admin Panel
        |
        v
eMarketing.Api (Swagger Web API)
        |
        v
eMarketing.Service (Class Library)
        |
        v
MS SQL Server + Stored Procedure
```

## Projeler

- `eMarketing.AdminPanel`: Windows Forms yonetim paneli.
- `eMarketing.Api`: Swagger destekli ASP.NET Core Web API.
- `eMarketing.Service`: API'nin kullandigi class library katmani.
- `eMarketing.Data`: Eski/fallback veri erisim katmani.
- `Database`: SQL Server tablo, view, stored procedure ve demo veri scriptleri.

## Kurulum

1. SQL Server uzerinde `EMarketingDb` veritabanini olusturun.
2. `Database` klasorundeki scriptleri ana kurulumdan eksik stored procedure scriptlerine dogru sirayla calistirin. Baslangic icin `00_MasterKurulum.sql` ve demo veri icin `DemoVeriTemizKurulum.sql` kullanilir.
3. API connection string ayarini ortam degiskeni veya user-secret ile verin:

```powershell
dotnet user-secrets set "ConnectionStrings:DbConnection" "Data Source=.\SQLEXPRESS;Initial Catalog=EMarketingDb;Integrated Security=True;Encrypt=False;TrustServerCertificate=True" --project eMarketing.Api\eMarketing.Api.csproj
dotnet user-secrets set "Jwt:Key" "uzun-ve-gizli-bir-gelistirme-anahtari" --project eMarketing.Api\eMarketing.Api.csproj
```

`appsettings.json` icindeki connection string ve JWT key sadece gelistirme varsayimidir. Gercek ortamda `ConnectionStrings__DbConnection` ve `Jwt__Key` ortam degiskenleriyle ezilmelidir.

## Calistirma

Repo kokunden API'yi calistirmak icin:

```powershell
dotnet run
```

Bu komut `eMarketing.Api` projesini `http://localhost:5088` adresinde baslatir.

Swagger:

```text
http://localhost:5088/swagger
```

Saglik kontrolu:

```text
GET http://localhost:5088/api/sistem/durum
```

Admin panel icin `eMarketing.AdminPanel\eMarketing.AdminPanel.sln` dosyasini Visual Studio ile acip WinForms uygulamasini calistirin. Panelin API adresi `eMarketing.AdminPanel\App.config` icindeki `ApiBaseUrl` ayarindan okunur.

## API Akisi

- Login: `POST /api/auth/login`
- Kategori: `GET/POST/PUT/PATCH/DELETE /api/kategoriler`
- Urun: `GET/POST/PUT/PATCH/DELETE /api/urunler`
- Siparis: `GET/POST/PATCH /api/siparisler`
- Dashboard: `GET /api/dashboard/ozet`, `GET /api/dashboard/son-siparisler`, `GET /api/dashboard/kritik-stok`
- Bayi, magaza, yetkili, bayi stok ve personel islemleri ilgili controller endpointleri uzerinden yapilir.

Login disindaki yazma, guncelleme ve silme islemleri JWT token gerektirir. AdminPanel login sonrasinda token'i `Authorization: Bearer {token}` header'i ile API'ye gonderir. Okuma endpointlerinin bir bolumu okul/demo akisini kolaylastirmak icin anonim kalmistir.

## Okul Checklist Karsiligi

- MS SQL Server veritabani kullaniliyor.
- Class library katmani `eMarketing.Service` projesidir.
- Web service/API uygulamasi `eMarketing.Api` projesidir.
- API, class library projesini `ProjectReference` ile kullanir.
- Connection string `eMarketing.Api/appsettings.json`, user-secrets veya ortam degiskeninden okunur.
- Veri alma, ekleme, silme ve guncelleme islemleri API endpointleriyle yapilir.
- Form uygulamasi API uzerinden login ve CRUD islemleri yapar.
- Rol ve magaza secimi WinForms oturum akisi icinde korunur.

## Notlar

- Sifre dogrulama su anda demo yapisina uyumlu olarak `sp_Kullanici_GirisYap` stored procedure uzerinden calisir.
- Profesyonel surum icin sonraki adimlar sifre hash migration, rol bazli authorization policy'leri ve tum hassas okuma endpointlerinde token zorunlulugudur.
- API kapaliyken eski repository fallback davranisi olan ekranlarda hata kaydi `eMarketing.AdminPanel\bin\Debug\Logs\api-fallback.log` dosyasina yazilir.
