# eMarketing Bayi Portalı Yol Haritası

Bu dosya, web bayi portalı ve AdminPanel tarafında sırayla ele alınacak operasyon başlıklarını takip etmek için tutulur.

## Mimari Kural: Dinamik Sistem

- Uygulamadaki dashboard, carousel, bildirim, stok, sipariş ve admin ekranları kalıcı olarak DB/API kaynaklı çalışacak.
- Statik demo/fallback veri kalıcı çözüm olarak kullanılmayacak.
- Geçici fallback gerekiyorsa açıkça geçici olarak işaretlenecek ve kaldırma maddesi yol haritasına eklenecek.
- UI'da görünen tüm aksiyonlar backend policy/service katmanında da doğrulanacak.

## 1. Sipariş Yönetimi

- Web `/orders` ekranında sipariş detay drawer üzerinden durum yönetimi.
- Durumlar: `Hazirlaniyor`, `Kargoda`, `Teslim Edildi`, `Iptal`.
- `Teslim Edildi` akışı mevcut backend prosedürüyle bayi stoğuna giriş yapmalı.
- `Iptal` akışı mevcut backend prosedürüyle merkez stok iadesi kurallarını çalıştırmalı.
- Admin/Yönetici rolleri web tarafında sipariş durumunu yönetebilmeli.

## 2. Merkez ve Bayi Stok Yönetimi

- Merkez stok artırma/düzeltme akışı AdminPanel içinde net bir formdan yapılmalı.
- Bayi stok hareketleri merkez stoktan transfer, manuel giriş, manuel çıkış ve minimum stok güncelleme olarak ayrılmalı.
- Kritik stok uyarıları dashboard ve bildirim altyapısına bağlanmalı.

## 3. Kampanya ve Carousel Yönetimi

- `Kampanyalar` tablosu, kampanya kapsam tabloları ve stored procedure seti eklendi.
- Kampanya CRUD Web Admin üzerinde yapılır; AdminPanel kampanya yönetim kapsamından çıkarıldı.
- Web Admin kampanya görseli yükleme, cropper ile banner kırpma, başlık/açıklama/detay/katılım şartı, aktiflik ve tarih aralığı yönetir.
- Web Admin kampanya hedefini `Yok`, `URL`, `Ürün` veya `Kategori` olarak yönetir.
- Web dashboard carousel sadece API verisiyle beslenir; veri yoksa eski statik fallback gösterilmez.
- Carousel görseline tıklanınca kampanya detay sayfası açılır.
- Kampanya detay sayfasında görsel, başlık, açıklama, geçerlilik tarihleri, detay metni, katılım şartları ve varsa hedefe git aksiyonu gösterilir.
- Admin notları kullanıcı detayında gösterilmez; ileride admin iç notu olarak kalabilir.
- Kampanya hedefi ürün/kategori ise detay sayfasından ilgili ürün listesine yönlendirme yapılır.

## 4. Bildirim ve Mesajlaşma

- `Bildirimler`, `BildirimMagazalari` ve `BildirimOkumalari` tabloları Web/API altyapısına eklendi.
- Header bildirim rozeti, bildirim dropdown'ı ve `/notifications` kullanıcı bildirim merkezi eklendi.
- Web Admin `/admin/notifications` üzerinden mağaza hedefli özel mesaj bildirimi gönderebilir.
- Manuel bildirimlerde serbest URL yerine ürün, kategori, kampanya veya güvenli sistem sayfası hedefi seçilir.
- Kampanyalar mağazalara günlük özet bildirimi olarak otomatik düşer.
- Kampanya özeti tek kampanya ise kampanya detayına, birden fazla kampanya varsa `/campaigns` listesine gider.
- Kritik stok bildirimi ürün kritik eşiğe ilk düştüğünde üretilir; aynı kritik durumda tekrar tekrar gönderilmez.
- Kritik stok bildirimi hedef ürün biliniyorsa `/products?urunId=...` adresine gider.
- Bildirim sayıları seçili mağazadaki tüm bildirim dağılımını gösterir; filtreler sadece listeyi süzer.
- İlk etap gerçek zamanlı chat değil, bildirim tabanlı mağaza mesajları olarak tutulur.

## 5. Admin Yetki Tamamlama

- AdminPanel formlarında adminin tam yetkileri görünür ve kullanılabilir olacak.
- Bayi/personel/sipariş/stok/kampanya formlarında rol bazlı aksiyon görünürlüğü netleştirilecek.
- UI'da görünen tüm aksiyonlar backend policy/service katmanında da doğrulanacak.
