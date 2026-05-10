# eMarketing Admin Panel ve Web API

Bu proje, otomotiv yedek parça satışı yapan bir toptancı / ana firma için hazırlanmış okul projesidir.

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

- `eMarketing.AdminPanel`: Windows Forms yönetim paneli.
- `eMarketing.Api`: Swagger destekli Web API.
- `eMarketing.Service`: API'nin kullandığı Class Library katmanı.
- `eMarketing.Data`: Mevcut WinForms yedek repository katmanı.
- `Database`: SQL Server tablo, view, stored procedure ve demo veri scriptleri.

## API Çalıştırma

```powershell
dotnet run --project eMarketing.Api\eMarketing.Api.csproj --urls http://localhost:5088
```

Swagger:

```text
http://localhost:5088/swagger
```

## Okul checklist karsiligi

- **MS SQL Server veritabani:** `Database` klasorundeki SQL scriptleri ve mevcut `EMarketingDb`.
- **Class Library:** `eMarketing.Service` projesi. SQL Server stored procedure cagirilari ve servis metotlari burada.
- **WebService / API:** `eMarketing.Api` projesi. Swagger aktiftir.
- **WebService -> Class Library referansi:** `eMarketing.Api` projesi `eMarketing.Service` projesine `ProjectReference` ile baglidir.
- **Connection string:** `eMarketing.Api/appsettings.json` icindeki `ConnectionStrings:DbConnection`.
- **Form uygulamasi:** `eMarketing.AdminPanel` Windows Forms projesi.
- **Form -> API akisi:** AdminPanel dogrudan veritabanina gitmez; `ApiDataClient` ile Web API'ye baglanir.
- **API -> SQL akisi:** `eMarketing.Api -> eMarketing.Service -> MS SQL Stored Procedure`.
- **Login ve rol yonetimi:** Login API uzerinden token alir, rol bilgisi `AppSession` icinde tutulur.
- **CRUD islemleri:** Urun, kategori, siparis, bayi/magaza, yetkili, bayi stok ve personel islemleri API endpointleri uzerinden yapilir.

Not: `eMarketing.Data` klasoru silinmedi; eski/fallback veri erisim katmani olarak repoda durabilir. Ancak AdminPanel artik bu katmana referans vermez. Okul checklist'indeki aktif ClassLibrary yapisi `eMarketing.Service` projesidir.

API sağlık kontrolü:

```text
GET http://localhost:5088/api/sistem/durum
```

## Örnek Endpointler

- `POST /api/auth/login`
- `GET /api/kategoriler`
- `POST /api/kategoriler`
- `PUT /api/kategoriler/{id}`
- `DELETE /api/kategoriler/{id}`
- `GET /api/urunler`
- `POST /api/urunler`
- `PUT /api/urunler/{id}`
- `DELETE /api/urunler/{id}`
- `GET /api/siparisler`
- `POST /api/siparisler`
- `PATCH /api/siparisler/{id}/durum`

## Admin Panel API Kullanımı

Admin panel şu işlemlerde önce API'yi kullanır:

- Kullanıcı girişi
- Kategori listeleme, ekleme, güncelleme, pasife alma ve silme
- Ürün listeleme, ekleme, güncelleme, pasife alma ve silme
- Sipariş listeleme, sipariş oluşturma ve durum güncelleme

API kapalıysa panel tamamen bozulmasın diye eski repository katmanına fallback yapılır. Bu durum şu dosyaya raporlanır:

```text
eMarketing.AdminPanel\bin\Debug\Logs\api-fallback.log
```

## Okul Checklist Karşılığı

- MS SQL Server veritabanı kullanılıyor.
- Class Library katmanı `eMarketing.Service` olarak eklendi.
- Web service uygulaması `eMarketing.Api` olarak eklendi.
- Web API, Class Library projesini referans alıyor.
- Connection string `eMarketing.Api/appsettings.json` içinden okunuyor.
- Veri alma, ekleme, silme ve güncelleme işlemleri API endpointleriyle yapılıyor.
- Form uygulaması API üzerinden login ve CRUD işlemleri yapabiliyor.
- Rol ve mağaza seçimi mevcut WinForms akışında korunuyor.

## Not

Şifre doğrulama şu an mevcut okul/demo yapısına uyumlu olarak eski `sp_Kullanici_GirisYap` stored procedure üzerinden çalışır. Profesyonel sürümde bir sonraki adım şifre hash migration ve JWT yetki zorunluluğudur.
# eMarketing

Repo kokunden API'yi calistirmak icin:

```powershell
dotnet run
```

Bu komut `eMarketing.Api` projesini `http://localhost:5088` adresinde baslatir. Swagger:

```text
http://localhost:5088/swagger
```
