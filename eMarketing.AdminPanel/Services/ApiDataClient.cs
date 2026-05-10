using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using eMarketing.AdminPanel.Core;

namespace eMarketing.AdminPanel.Services
{
    public class ApiDataClient
    {
        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly string _baseUrl;

        public ApiDataClient()
        {
            string configuredUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            _baseUrl = string.IsNullOrWhiteSpace(configuredUrl)
                ? "http://localhost:5088/api"
                : configuredUrl.Trim().TrimEnd('/');
        }

        public Task<DataTable> GetCategoriesAsync(string search = "", int status = 1)
        {
            return GetDataTableAsync(BuildUrl("kategoriler", "arama", search, "durum", status.ToString()));
        }

        public async Task<DataRow> GetCategoryByIdAsync(int categoryId)
        {
            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(_baseUrl + "/kategoriler/" + categoryId, "Kategori detay").ConfigureAwait(false));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public async Task<int> InsertCategoryAsync(string categoryName)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/kategoriler", _serializer.Serialize(new
            {
                KategoriAdi = categoryName,
                AktifMi = true
            }), "Kategori ekleme").ConfigureAwait(false);

            return GetPayloadId(response, "KategoriId");
        }

        public Task UpdateCategoryAsync(int categoryId, string categoryName, bool isActive)
        {
            return SendJsonAsync(HttpMethod.Put, _baseUrl + "/kategoriler/" + categoryId, _serializer.Serialize(new
            {
                KategoriAdi = categoryName,
                AktifMi = isActive
            }), "Kategori güncelleme");
        }

        public Task SetCategoryActiveStatusAsync(int categoryId, bool isActive)
        {
            return SendJsonAsync(new HttpMethod("PATCH"), _baseUrl + "/kategoriler/" + categoryId + "/durum", _serializer.Serialize(new
            {
                KategoriAdi = "",
                AktifMi = isActive
            }), "Kategori durum güncelleme");
        }

        public Task DeleteCategoryAsync(int categoryId)
        {
            return DeleteAsync(_baseUrl + "/kategoriler/" + categoryId, "Kategori silme");
        }

        public async Task<ApiLoginResult> LoginAsync(string kullaniciAdi, string sifre)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/auth/login", _serializer.Serialize(new
            {
                KullaniciAdi = kullaniciAdi,
                Sifre = sifre
            }), "Kullanıcı girişi").ConfigureAwait(false);

            Dictionary<string, object> payload = _serializer.Deserialize<Dictionary<string, object>>(response);
            Dictionary<string, object> kullanici = payload != null && payload.ContainsKey("Kullanici")
                ? payload["Kullanici"] as Dictionary<string, object>
                : null;

            if (kullanici == null)
                return null;

            return new ApiLoginResult
            {
                Token = GetDictionaryText(payload, "Token"),
                Kullanici = new KullaniciGirisModel
                {
                    KullaniciId = Convert.ToInt32(kullanici["KullaniciId"]),
                    KullaniciAdi = GetDictionaryText(kullanici, "KullaniciAdi"),
                    AdSoyad = GetDictionaryText(kullanici, "AdSoyad"),
                    Rol = GetDictionaryText(kullanici, "Rol")
                }
            };
        }

        public Task<DataTable> GetProductsAsync(string search = "", int status = 1, int categoryId = 0)
        {
            return GetDataTableAsync(BuildUrl("urunler", "arama", search, "durum", status.ToString(), "kategoriId", categoryId.ToString()));
        }

        public async Task<DataRow> GetProductByIdAsync(int productId)
        {
            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(_baseUrl + "/urunler/" + productId, "Ürün detay").ConfigureAwait(false));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public async Task<int> InsertProductAsync(string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/urunler",
                BuildProductJson(name, description, price, stock, imageUrl, isActive, categoryId), "Ürün ekleme").ConfigureAwait(false);
            return GetPayloadId(response, "UrunId");
        }

        public Task UpdateProductAsync(int id, string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            return SendJsonAsync(HttpMethod.Put, _baseUrl + "/urunler/" + id,
                BuildProductJson(name, description, price, stock, imageUrl, isActive, categoryId), "Ürün güncelleme");
        }

        public Task SetProductActiveStatusAsync(int productId, bool isActive)
        {
            return SendJsonAsync(new HttpMethod("PATCH"), _baseUrl + "/urunler/" + productId + "/durum",
                BuildProductJson("", "", 0, 0, "", isActive, 1), "Ürün durum güncelleme");
        }

        public Task DeleteProductAsync(int productId)
        {
            return DeleteAsync(_baseUrl + "/urunler/" + productId, "Ürün silme");
        }

        public Task<DataTable> GetOrdersAsync(int? magazaId = null, bool tumMagazalar = true)
        {
            string url = magazaId.HasValue
                ? BuildUrl("siparisler", "magazaId", magazaId.Value.ToString(), "tumMagazalar", BoolText(tumMagazalar))
                : BuildUrl("siparisler", "tumMagazalar", BoolText(tumMagazalar));

            return GetDataTableAsync(url);
        }

        public async Task<OrderSummaryView> GetOrderSummaryAsync(int? magazaId = null, bool tumMagazalar = true)
        {
            string url = magazaId.HasValue
                ? BuildUrl("siparisler/ozet", "magazaId", magazaId.Value.ToString(), "tumMagazalar", BoolText(tumMagazalar))
                : BuildUrl("siparisler/ozet", "tumMagazalar", BoolText(tumMagazalar));

            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(url, "Sipariş özeti").ConfigureAwait(false));
            DataRow row = table.Rows.Count == 0 ? null : table.Rows[0];

            return new OrderSummaryView
            {
                TotalOrders = GetInt(row, "TotalOrders", "ToplamSiparis"),
                PreparingOrders = GetInt(row, "PreparingOrders", "HazirlaniyorSayisi"),
                ShippedOrders = GetInt(row, "ShippedOrders", "KargodaSayisi"),
                DeliveredOrders = GetInt(row, "DeliveredOrders", "TeslimEdildiSayisi"),
                CancelledOrders = GetInt(row, "CancelledOrders", "IptalSayisi")
            };
        }

        public async Task<DashboardSummaryView> GetDashboardSummaryAsync(int? magazaId = null, bool tumMagazalar = true)
        {
            string url = magazaId.HasValue
                ? BuildUrl("dashboard/ozet", "magazaId", magazaId.Value.ToString(), "tumMagazalar", BoolText(tumMagazalar))
                : BuildUrl("dashboard/ozet", "tumMagazalar", BoolText(tumMagazalar));

            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(url, "Dashboard özeti").ConfigureAwait(false));
            DataRow row = table.Rows.Count == 0 ? null : table.Rows[0];

            return new DashboardSummaryView
            {
                TotalRevenue = GetDecimal(row, "TotalRevenue", "ToplamCiro"),
                TotalOrders = GetInt(row, "TotalOrders", "ToplamSiparis"),
                ActiveStores = GetInt(row, "ActiveStores", "AktifMagaza"),
                TotalCustomers = GetInt(row, "TotalCustomers", "ToplamMusteri"),
                PendingPaymentOrders = GetInt(row, "PendingPaymentOrders", "BekleyenOdemeSayisi"),
                LowStockProducts = GetInt(row, "LowStockProducts", "KritikStok"),
                PreparingOrders = GetInt(row, "PreparingOrders", "HazirlaniyorSayisi"),
                ShippedOrders = GetInt(row, "ShippedOrders", "KargodaSayisi"),
                DeliveredOrders = GetInt(row, "DeliveredOrders", "TeslimEdildiSayisi")
            };
        }

        public Task<DataTable> GetDashboardRecentOrdersAsync(int? magazaId = null, bool tumMagazalar = true)
        {
            string url = magazaId.HasValue
                ? BuildUrl("dashboard/son-siparisler", "magazaId", magazaId.Value.ToString(), "tumMagazalar", BoolText(tumMagazalar))
                : BuildUrl("dashboard/son-siparisler", "tumMagazalar", BoolText(tumMagazalar));

            return GetDataTableAsync(url);
        }

        public Task<DataTable> GetDashboardCriticalStockAsync(int? magazaId = null, bool tumMagazalar = true)
        {
            string url = magazaId.HasValue
                ? BuildUrl("dashboard/kritik-stok", "magazaId", magazaId.Value.ToString(), "tumMagazalar", BoolText(tumMagazalar))
                : BuildUrl("dashboard/kritik-stok", "tumMagazalar", BoolText(tumMagazalar));

            return GetDataTableAsync(url);
        }

        public Task<DataTable> GetMagazaSecimListesiAsync(string arama, bool sadeceAktif, int? kullaniciId, bool adminMi)
        {
            return GetDataTableAsync(BuildUrl("magazalar/secim",
                "arama", arama,
                "sadeceAktif", BoolText(sadeceAktif),
                "kullaniciId", kullaniciId.HasValue ? kullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)));
        }

        public async Task<DataRow> GetMagazaByIdAsync(int magazaId, int? kullaniciId, bool adminMi)
        {
            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(BuildUrl("magazalar/secim/" + magazaId,
                "kullaniciId", kullaniciId.HasValue ? kullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)), "Mağaza detay").ConfigureAwait(false));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public Task<DataTable> GetBayiYetkilileriAsync(string arama = "", int durum = -1, int? bayiId = null, int? magazaId = null)
        {
            return GetDataTableAsync(BuildUrl("bayi-yetkilileri",
                "arama", arama,
                "durum", durum.ToString(),
                "bayiId", bayiId.HasValue ? bayiId.Value.ToString() : "",
                "magazaId", magazaId.HasValue ? magazaId.Value.ToString() : ""));
        }

        public async Task<DataRow> GetBayiYetkiliByIdAsync(int bayiYetkiliId)
        {
            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(_baseUrl + "/bayi-yetkilileri/" + bayiYetkiliId, "Yetkili detay").ConfigureAwait(false));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public async Task<int> SaveBayiYetkiliAsync(int? bayiYetkiliId, int bayiId, int? magazaId, string adSoyad, string telefon, string email, string gorev, string notlar, bool aktifMi)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/bayi-yetkilileri", _serializer.Serialize(new
            {
                BayiYetkiliId = bayiYetkiliId,
                BayiId = bayiId,
                MagazaId = magazaId,
                AdSoyad = adSoyad,
                Telefon = telefon,
                Email = email,
                Gorev = gorev,
                Notlar = notlar,
                AktifMi = aktifMi
            }), "Yetkili kaydetme").ConfigureAwait(false);

            return GetPayloadId(response, "BayiYetkiliId");
        }

        public Task SetBayiYetkiliStatusAsync(int bayiYetkiliId, bool aktifMi)
        {
            return SendJsonAsync(new HttpMethod("PATCH"), _baseUrl + "/bayi-yetkilileri/" + bayiYetkiliId + "/durum",
                _serializer.Serialize(new { AktifMi = aktifMi }), "Yetkili durum güncelleme");
        }

        public Task<DataTable> GetBayiStoklariAsync(int? magazaId, string arama, bool sadeceStokta, bool sadeceKritik, bool sadeceAktif, int? kullaniciId, bool adminMi)
        {
            return GetDataTableAsync(BuildUrl("bayi-stoklari",
                "magazaId", magazaId.HasValue ? magazaId.Value.ToString() : "",
                "arama", arama,
                "sadeceStokta", BoolText(sadeceStokta),
                "sadeceKritik", BoolText(sadeceKritik),
                "sadeceAktif", BoolText(sadeceAktif),
                "kullaniciId", kullaniciId.HasValue ? kullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)));
        }

        public async Task<DataRow> GetBayiStokOzetiAsync(int? magazaId, bool tumMagazalar, int? kullaniciId, bool adminMi)
        {
            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(BuildUrl("bayi-stoklari/ozet",
                "magazaId", magazaId.HasValue ? magazaId.Value.ToString() : "",
                "tumMagazalar", BoolText(tumMagazalar),
                "kullaniciId", kullaniciId.HasValue ? kullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)), "Bayi stok özeti").ConfigureAwait(false));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public Task<DataTable> GetBayiStokHareketleriAsync(int magazaId, int urunId, int kayitSayisi, int? kullaniciId, bool adminMi)
        {
            return GetDataTableAsync(BuildUrl("bayi-stoklari/hareketler",
                "magazaId", magazaId.ToString(),
                "urunId", urunId.ToString(),
                "kayitSayisi", kayitSayisi.ToString(),
                "kullaniciId", kullaniciId.HasValue ? kullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)));
        }

        public Task UpdateBayiStokMinimumAsync(int magazaStokId, int minimumStok)
        {
            return SendJsonAsync(new HttpMethod("PATCH"), _baseUrl + "/bayi-stoklari/" + magazaStokId + "/minimum",
                _serializer.Serialize(new { MinimumStok = minimumStok }), "Minimum stok güncelleme");
        }

        public Task ProcessBayiStokMovementAsync(int magazaId, int urunId, string hareketTipi, int miktar, string aciklama, int? minimumStok)
        {
            return SendJsonAsync(HttpMethod.Post, _baseUrl + "/bayi-stoklari/hareket", _serializer.Serialize(new
            {
                MagazaId = magazaId,
                UrunId = urunId,
                HareketTipi = hareketTipi,
                Miktar = miktar,
                Aciklama = aciklama,
                MinimumStok = minimumStok
            }), "Bayi stok hareketi");
        }

        public Task<DataTable> GetPersonellerAsync(string arama, bool sadeceAktif, int? goruntuleyenKullaniciId, bool adminMi)
        {
            return GetDataTableAsync(BuildUrl("personel",
                "arama", arama,
                "sadeceAktif", BoolText(sadeceAktif),
                "goruntuleyenKullaniciId", goruntuleyenKullaniciId.HasValue ? goruntuleyenKullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)));
        }

        public async Task<int> SavePersonelAsync(int? kullaniciId, string kullaniciAdi, string sifre, string adSoyad, string rol, bool aktifMi)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/personel", _serializer.Serialize(new
            {
                KullaniciId = kullaniciId,
                KullaniciAdi = kullaniciAdi,
                Sifre = sifre,
                AdSoyad = adSoyad,
                Rol = rol,
                AktifMi = aktifMi
            }), "Personel kaydetme").ConfigureAwait(false);

            return GetPayloadId(response, "KullaniciId");
        }

        public Task<DataTable> GetPersonelMagazalariAsync(int kullaniciId, int? goruntuleyenKullaniciId, bool adminMi)
        {
            return GetDataTableAsync(BuildUrl("personel/" + kullaniciId + "/magazalar",
                "goruntuleyenKullaniciId", goruntuleyenKullaniciId.HasValue ? goruntuleyenKullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)));
        }

        public Task<DataTable> GetAtanabilirMagazalarAsync(int kullaniciId, string arama, int? goruntuleyenKullaniciId, bool adminMi)
        {
            return GetDataTableAsync(BuildUrl("personel/" + kullaniciId + "/atanabilir-magazalar",
                "arama", arama,
                "goruntuleyenKullaniciId", goruntuleyenKullaniciId.HasValue ? goruntuleyenKullaniciId.Value.ToString() : "",
                "adminMi", BoolText(adminMi)));
        }

        public Task AssignPersonelMagazaAsync(int kullaniciId, int magazaId)
        {
            return SendJsonAsync(HttpMethod.Post, _baseUrl + "/personel/" + kullaniciId + "/magazalar/" + magazaId, "{}", "Personel mağaza atama");
        }

        public Task RemovePersonelMagazaAsync(int kullaniciMagazaId)
        {
            return DeleteAsync(_baseUrl + "/personel/magaza-yetkileri/" + kullaniciMagazaId, "Personel mağaza yetkisi kaldırma");
        }

        public Task<DataTable> GetBayilerAsync(string arama = "", int durum = -1)
        {
            return GetDataTableAsync(BuildUrl("bayiler", "arama", arama, "durum", durum.ToString()));
        }

        public async Task<DataRow> GetBayiByIdAsync(int customerId)
        {
            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(_baseUrl + "/bayiler/" + customerId, "Bayi detay").ConfigureAwait(false));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public async Task<int> InsertBayiAsync(string fullName, string companyName, string authorizedPerson, string phone, string email, string taxNumber, string taxOffice, string address, string customerType, bool isActive)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/bayiler",
                BuildBayiJson(fullName, companyName, authorizedPerson, phone, email, taxNumber, taxOffice, address, customerType, isActive), "Bayi ekleme").ConfigureAwait(false);
            return GetPayloadId(response, "CustomerId");
        }

        public Task UpdateBayiAsync(int customerId, string fullName, string companyName, string authorizedPerson, string phone, string email, string taxNumber, string taxOffice, string address, string customerType, bool isActive)
        {
            return SendJsonAsync(HttpMethod.Put, _baseUrl + "/bayiler/" + customerId,
                BuildBayiJson(fullName, companyName, authorizedPerson, phone, email, taxNumber, taxOffice, address, customerType, isActive), "Bayi güncelleme");
        }

        public Task SetBayiStatusAsync(int customerId, bool isActive)
        {
            return SendJsonAsync(new HttpMethod("PATCH"), _baseUrl + "/bayiler/" + customerId + "/durum",
                _serializer.Serialize(new { AktifMi = isActive }), "Bayi durum güncelleme");
        }

        public Task<DataTable> GetBayiMagazalariAsync(int customerId, int durum = -1)
        {
            return GetDataTableAsync(BuildUrl("bayiler/" + customerId + "/magazalar", "durum", durum.ToString()));
        }

        public async Task<DataRow> GetBayiMagazaByIdAsync(int customerStoreId)
        {
            DataTable table = ConvertJsonObjectToDataTable(await GetStringAsync(_baseUrl + "/bayi-magazalar/" + customerStoreId, "Bayi mağaza detay").ConfigureAwait(false));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public async Task<int> InsertBayiMagazaAsync(int customerId, string storeName, string city, string district, string address, string phone, string responsiblePerson, bool isActive)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/bayiler/" + customerId + "/magazalar",
                BuildBayiMagazaJson(storeName, city, district, address, phone, responsiblePerson, isActive), "Bayi mağaza ekleme").ConfigureAwait(false);
            return GetPayloadId(response, "CustomerStoreId");
        }

        public Task UpdateBayiMagazaAsync(int customerStoreId, string storeName, string city, string district, string address, string phone, string responsiblePerson, bool isActive)
        {
            return SendJsonAsync(HttpMethod.Put, _baseUrl + "/bayi-magazalar/" + customerStoreId,
                BuildBayiMagazaJson(storeName, city, district, address, phone, responsiblePerson, isActive), "Bayi mağaza güncelleme");
        }

        public Task SetBayiMagazaStatusAsync(int customerStoreId, bool isActive)
        {
            return SendJsonAsync(new HttpMethod("PATCH"), _baseUrl + "/bayi-magazalar/" + customerStoreId + "/durum",
                _serializer.Serialize(new { AktifMi = isActive }), "Bayi mağaza durum güncelleme");
        }

        public async Task<int> AddOrderAsync(string customerName, string customerEmail, string customerPhone, int productId, int quantity, decimal totalPrice, int? magazaId, string siparisTipi, string siparisKaynagi, int? bayiYetkiliId)
        {
            string response = await SendJsonAsync(HttpMethod.Post, _baseUrl + "/siparisler", _serializer.Serialize(new
            {
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerPhone = customerPhone,
                ProductId = productId,
                Quantity = quantity,
                TotalPrice = totalPrice,
                CustomerStoreId = magazaId,
                OrderType = siparisTipi,
                OrderSource = siparisKaynagi,
                BayiYetkiliId = bayiYetkiliId
            }), "Sipariş ekleme").ConfigureAwait(false);

            return GetPayloadId(response, "SiparisId");
        }

        public Task UpdateOrderStatusAsync(int orderId, string status)
        {
            return SendJsonAsync(new HttpMethod("PATCH"), _baseUrl + "/siparisler/" + orderId + "/durum",
                _serializer.Serialize(new { SiparisDurumu = status }), "Sipariş durum güncelleme");
        }

        public Task CancelOrderAsync(int orderId)
        {
            return SendJsonAsync(HttpMethod.Post, _baseUrl + "/siparisler/" + orderId + "/iptal", "{}", "Sipariş iptal");
        }

        private async Task<DataTable> GetDataTableAsync(string url)
        {
            return ConvertJsonArrayToDataTable(await GetStringAsync(url, "Veri listeleme").ConfigureAwait(false));
        }

        private async Task<string> GetStringAsync(string url, string operation)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddAuthorizationHeader(request);

                    using (HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false))
                    {
                        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        EnsureSuccess(response, body, operation);
                        return body;
                    }
                }
            }
            catch (Exception ex)
            {
                throw CreateApiException(operation, ex);
            }
        }

        private async Task<string> SendJsonAsync(HttpMethod method, string url, string json, string operation)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(method, url))
                using (StringContent content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = content;
                    AddAuthorizationHeader(request);

                    using (HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false))
                    {
                        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        EnsureSuccess(response, body, operation);
                        return body;
                    }
                }
            }
            catch (Exception ex)
            {
                throw CreateApiException(operation, ex);
            }
        }

        private async Task DeleteAsync(string url, string operation)
        {
            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url))
                {
                    AddAuthorizationHeader(request);

                    using (HttpResponseMessage response = await Client.SendAsync(request).ConfigureAwait(false))
                    {
                        string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        EnsureSuccess(response, body, operation);
                    }
                }
            }
            catch (Exception ex)
            {
                throw CreateApiException(operation, ex);
            }
        }

        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            string token = AppSession.ApiToken;
            if (string.IsNullOrWhiteSpace(token))
                return;

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private void EnsureSuccess(HttpResponseMessage response, string body, string operation)
        {
            if (response.IsSuccessStatusCode)
                return;

            string message = response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden
                ? "Bu işlem için yetkiniz bulunmuyor."
                : "Sunucu işlemi tamamlayamadı. Lütfen daha sonra tekrar deneyin.";

            throw new ApiClientException(message, operation, new HttpRequestException(response.StatusCode + " - " + body));
        }

        private ApiClientException CreateApiException(string operation, Exception exception)
        {
            ApiClientException apiException = exception as ApiClientException;
            if (apiException != null)
            {
                ApiErrorLogger.Log(operation, apiException);
                return apiException;
            }

            string userMessage = "API bağlantısı kurulamadı. Lütfen servisin çalıştığından emin olun.";

            if (exception is TaskCanceledException)
                userMessage = "API bağlantısı zaman aşımına uğradı. Lütfen servis durumunu kontrol edin.";
            else if (!(exception is HttpRequestException) && !(exception is WebException))
                userMessage = "Sunucudan beklenmeyen yanıt alındı. Lütfen işlemi tekrar deneyin.";

            ApiErrorLogger.Log(operation, exception);
            return new ApiClientException(userMessage, operation, exception);
        }

        private int GetPayloadId(string response, string key)
        {
            Dictionary<string, object> payload = _serializer.Deserialize<Dictionary<string, object>>(response);
            return payload != null && payload.ContainsKey(key) ? Convert.ToInt32(payload[key]) : 0;
        }

        private DataTable ConvertJsonObjectToDataTable(string json)
        {
            DataTable table = new DataTable();
            Dictionary<string, object> dictionary = _serializer.Deserialize<Dictionary<string, object>>(json);

            if (dictionary == null)
                return table;

            EnsureColumns(table, dictionary);

            DataRow row = table.NewRow();
            foreach (KeyValuePair<string, object> pair in dictionary)
                row[pair.Key] = NormalizeValue(pair.Value);

            table.Rows.Add(row);
            return table;
        }

        private DataTable ConvertJsonArrayToDataTable(string json)
        {
            DataTable table = new DataTable();
            object parsed = _serializer.DeserializeObject(json);

            object[] rows = parsed as object[];
            if (rows == null)
                return table;

            foreach (object item in rows)
            {
                Dictionary<string, object> dictionary = item as Dictionary<string, object>;
                if (dictionary == null)
                    continue;

                EnsureColumns(table, dictionary);

                DataRow row = table.NewRow();
                foreach (KeyValuePair<string, object> pair in dictionary)
                    row[pair.Key] = NormalizeValue(pair.Value);

                table.Rows.Add(row);
            }

            return table;
        }

        private void EnsureColumns(DataTable table, Dictionary<string, object> dictionary)
        {
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (!table.Columns.Contains(pair.Key))
                    table.Columns.Add(pair.Key, GetColumnType(pair.Value));
            }
        }

        private Type GetColumnType(object value)
        {
            if (value == null)
                return typeof(string);

            if (value is bool)
                return typeof(bool);

            if (value is int)
                return typeof(int);

            if (value is long)
                return typeof(long);

            if (value is decimal || value is double || value is float)
                return typeof(decimal);

            return typeof(string);
        }

        private object NormalizeValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            if (value is ArrayList)
                return value.ToString();

            if (value is double || value is float)
                return Convert.ToDecimal(value);

            return value;
        }

        private int GetInt(DataRow row, params string[] columnNames)
        {
            if (row == null)
                return 0;

            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                    return Convert.ToInt32(row[columnName]);
            }

            return 0;
        }

        private decimal GetDecimal(DataRow row, params string[] columnNames)
        {
            if (row == null)
                return 0;

            foreach (string columnName in columnNames)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                    return Convert.ToDecimal(row[columnName]);
            }

            return 0;
        }

        private string BuildUrl(string endpoint, params string[] queryParts)
        {
            var query = new List<string>();

            for (int i = 0; i + 1 < queryParts.Length; i += 2)
            {
                string key = queryParts[i];
                string value = queryParts[i + 1] ?? string.Empty;
                query.Add(Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value));
            }

            return _baseUrl + "/" + endpoint + (query.Count == 0 ? string.Empty : "?" + string.Join("&", query));
        }

        private string BoolText(bool value)
        {
            return value.ToString().ToLowerInvariant();
        }

        private string BuildProductJson(string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            return _serializer.Serialize(new
            {
                KategoriId = categoryId,
                UrunAdi = name,
                Aciklama = description,
                Fiyat = price,
                Stok = stock,
                GorselUrl = imageUrl,
                AktifMi = isActive
            });
        }

        private string BuildBayiJson(string fullName, string companyName, string authorizedPerson, string phone, string email, string taxNumber, string taxOffice, string address, string customerType, bool isActive)
        {
            return _serializer.Serialize(new
            {
                FullName = fullName,
                CompanyName = companyName,
                AuthorizedPerson = authorizedPerson,
                Phone = phone,
                Email = email,
                TaxNumber = taxNumber,
                TaxOffice = taxOffice,
                Address = address,
                CustomerType = customerType,
                IsActive = isActive
            });
        }

        private string BuildBayiMagazaJson(string storeName, string city, string district, string address, string phone, string responsiblePerson, bool isActive)
        {
            return _serializer.Serialize(new
            {
                StoreName = storeName,
                City = city,
                District = district,
                Address = address,
                Phone = phone,
                ResponsiblePerson = responsiblePerson,
                IsActive = isActive
            });
        }

        private string GetDictionaryText(Dictionary<string, object> dictionary, string key)
        {
            if (dictionary == null || !dictionary.ContainsKey(key) || dictionary[key] == null)
                return string.Empty;

            return Convert.ToString(dictionary[key]) ?? string.Empty;
        }
    }

    public sealed class ApiClientException : Exception
    {
        public ApiClientException(string userMessage, string operation, Exception innerException)
            : base(userMessage, innerException)
        {
            Operation = operation ?? "";
        }

        public string Operation { get; private set; }
    }

    public sealed class ApiLoginResult
    {
        public string Token { get; set; }
        public KullaniciGirisModel Kullanici { get; set; }
    }

    public sealed class KullaniciGirisModel
    {
        public int KullaniciId { get; set; }
        public string KullaniciAdi { get; set; }
        public string AdSoyad { get; set; }
        public string Rol { get; set; }
    }

    public sealed class DashboardSummaryView
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveStores { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingPaymentOrders { get; set; }
        public int LowStockProducts { get; set; }
        public int PreparingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
    }

    public sealed class OrderSummaryView
    {
        public int TotalOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}
