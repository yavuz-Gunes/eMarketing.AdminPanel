# eMarketing Bayi Portalı Yol Haritası

Bu dosya, web bayi portalı ve AdminPanel tarafında sırayla ele alınacak operasyon başlıklarını takip etmek için tutulur.

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

- `Kampanyalar` tablosu eklenecek.
- AdminPanel üzerinde kampanya CRUD yapılacak.
- Kampanya görseli yükleme, hedef URL/ürün/kategori seçimi, öncelik ve tarih aralığı yönetilecek.
- Web dashboard carousel API verisiyle beslenecek; veri yoksa fallback çalışmaya devam edecek.
- Carousel görseline veya CTA butonuna tıklanınca ayrı bir kampanya detay sayfası açılacak.
- Kampanya detay sayfasında başlık, açıklama, büyük görsel, geçerlilik tarihleri, hedef ürün/kategori, katılım şartları ve admin notları gösterilecek.
- Kampanyaya katılma şartları yapılandırılabilir olacak; örnek: minimum sipariş tutarı, belirli ürün/kategori zorunluluğu, mağaza/bayi kapsamı, stokla sınırlı kampanya.
- Kampanya hedefi ürün/kategori ise detay sayfasından ilgili ürün listesine yönlendirme yapılacak.

## 4. Bildirim ve Mesajlaşma

- `Bildirimler` ve `BildirimOkumalari` tabloları eklenecek.
- Admin mağazalara kampanya, kritik stok ve özel mesaj bildirimi gönderebilecek.
- `MagazaMesajlari` veya `EkipMesajlari` ile ekip içi not/mesaj akışı kurulacak.
- İlk etap gerçek zamanlı chat değil, bildirim tabanlı mağaza mesajları olacak.

## 5. Admin Yetki Tamamlama

- AdminPanel formlarında adminin tam yetkileri görünür ve kullanılabilir olacak.
- Bayi/personel/sipariş/stok/kampanya formlarında rol bazlı aksiyon görünürlüğü netleştirilecek.
- UI’da görünen tüm aksiyonlar backend policy/service katmanında da doğrulanacak.
