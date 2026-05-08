using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;
using eMarketing.Data.Models;

namespace eMarketing.AdminPanel.Services
{
    public class ApiDataClient
    {
        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
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
        
        public DataTable GetCategories(string search = "", int status = 1)
        {
            string url = BuildUrl("kategoriler",
                "arama", search,
                "durum", status.ToString());

            return GetDataTable(url);
        }

        public DataRow GetCategoryById(int categoryId)
        {
            string url = _baseUrl + "/kategoriler/" + categoryId;
            DataTable table = ConvertJsonObjectToDataTable(GetString(url));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public int InsertCategory(string categoryName)
        {
            string json = _serializer.Serialize(new
            {
                KategoriAdi = categoryName,
                AktifMi = true
            });

            string response = SendJson(HttpMethod.Post, _baseUrl + "/kategoriler", json);
            Dictionary<string, object> payload = _serializer.Deserialize<Dictionary<string, object>>(response);
            return payload != null && payload.ContainsKey("KategoriId")
                ? Convert.ToInt32(payload["KategoriId"])
                : 0;
        }

        public void UpdateCategory(int categoryId, string categoryName, bool isActive)
        {
            string json = _serializer.Serialize(new
            {
                KategoriAdi = categoryName,
                AktifMi = isActive
            });

            SendJson(HttpMethod.Put, _baseUrl + "/kategoriler/" + categoryId, json);
        }

        public void SetCategoryActiveStatus(int categoryId, bool isActive)
        {
            string json = _serializer.Serialize(new
            {
                KategoriAdi = "",
                AktifMi = isActive
            });

            SendJson(new HttpMethod("PATCH"), _baseUrl + "/kategoriler/" + categoryId + "/durum", json);
        }

        public void DeleteCategory(int categoryId)
        {
            using (HttpResponseMessage response = Client.DeleteAsync(_baseUrl + "/kategoriler/" + categoryId).GetAwaiter().GetResult())
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public KullaniciGirisModel Login(string kullaniciAdi, string sifre, out string token)
        {
            string json = _serializer.Serialize(new
            {
                KullaniciAdi = kullaniciAdi,
                Sifre = sifre
            });

            string response = SendJson(HttpMethod.Post, _baseUrl + "/auth/login", json);
            Dictionary<string, object> payload = _serializer.Deserialize<Dictionary<string, object>>(response);

            token = GetDictionaryText(payload, "Token");

            Dictionary<string, object> kullanici = payload != null && payload.ContainsKey("Kullanici")
                ? payload["Kullanici"] as Dictionary<string, object>
                : null;

            if (kullanici == null)
                return null;

            return new KullaniciGirisModel
            {
                KullaniciId = Convert.ToInt32(kullanici["KullaniciId"]),
                KullaniciAdi = GetDictionaryText(kullanici, "KullaniciAdi"),
                AdSoyad = GetDictionaryText(kullanici, "AdSoyad"),
                Rol = GetDictionaryText(kullanici, "Rol")
            };
        }

        public DataTable GetProducts(string search = "", int status = 1, int categoryId = 0)
        {
            string url = BuildUrl("urunler",
                "arama", search,
                "durum", status.ToString(),
                "kategoriId", categoryId.ToString());

            return GetDataTable(url);
        }

        public DataRow GetProductById(int productId)
        {
            string url = _baseUrl + "/urunler/" + productId;
            DataTable table = ConvertJsonObjectToDataTable(GetString(url));
            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public int InsertProduct(string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            string json = BuildProductJson(name, description, price, stock, imageUrl, isActive, categoryId);
            string response = SendJson(HttpMethod.Post, _baseUrl + "/urunler", json);
            Dictionary<string, object> payload = _serializer.Deserialize<Dictionary<string, object>>(response);
            return payload != null && payload.ContainsKey("UrunId")
                ? Convert.ToInt32(payload["UrunId"])
                : 0;
        }

        public void UpdateProduct(int id, string name, string description, decimal price, int stock, string imageUrl, bool isActive, int categoryId)
        {
            string json = BuildProductJson(name, description, price, stock, imageUrl, isActive, categoryId);
            SendJson(HttpMethod.Put, _baseUrl + "/urunler/" + id, json);
        }

        public void SetProductActiveStatus(int productId, bool isActive)
        {
            string json = BuildProductJson("", "", 0, 0, "", isActive, 1);
            SendJson(new HttpMethod("PATCH"), _baseUrl + "/urunler/" + productId + "/durum", json);
        }

        public void DeleteProduct(int productId)
        {
            using (HttpResponseMessage response = Client.DeleteAsync(_baseUrl + "/urunler/" + productId).GetAwaiter().GetResult())
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public DataTable GetOrders(int? magazaId = null, bool tumMagazalar = true)
        {
            string url = magazaId.HasValue
                ? BuildUrl("siparisler", "magazaId", magazaId.Value.ToString(), "tumMagazalar", tumMagazalar.ToString().ToLowerInvariant())
                : BuildUrl("siparisler", "tumMagazalar", tumMagazalar.ToString().ToLowerInvariant());

            return GetDataTable(url);
        }

        public int AddOrder(
            string customerName,
            string customerEmail,
            string customerPhone,
            int productId,
            int quantity,
            decimal totalPrice,
            int? magazaId,
            string siparisTipi,
            string siparisKaynagi,
            int? bayiYetkiliId)
        {
            string json = _serializer.Serialize(new
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
            });

            string response = SendJson(HttpMethod.Post, _baseUrl + "/siparisler", json);
            Dictionary<string, object> payload = _serializer.Deserialize<Dictionary<string, object>>(response);
            return payload != null && payload.ContainsKey("SiparisId")
                ? Convert.ToInt32(payload["SiparisId"])
                : 0;
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            string json = _serializer.Serialize(new
            {
                SiparisDurumu = status
            });

            SendJson(new HttpMethod("PATCH"), _baseUrl + "/siparisler/" + orderId + "/durum", json);
        }

        public void CancelOrder(int orderId)
        {
            SendJson(HttpMethod.Post, _baseUrl + "/siparisler/" + orderId + "/iptal", "{}");
        }

        private DataTable GetDataTable(string url)
        {
            return ConvertJsonArrayToDataTable(GetString(url));
        }

        private string GetString(string url)
        {
            using (HttpResponseMessage response = Client.GetAsync(url).GetAwaiter().GetResult())
            {
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }

        private string SendJson(HttpMethod method, string url, string json)
        {
            using (var request = new HttpRequestMessage(method, url))
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                request.Content = content;

                using (HttpResponseMessage response = Client.SendAsync(request).GetAwaiter().GetResult())
                {
                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
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
            {
                row[pair.Key] = NormalizeValue(pair.Value);
            }

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
                {
                    row[pair.Key] = NormalizeValue(pair.Value);
                }

                table.Rows.Add(row);
            }

            return table;
        }

        private void EnsureColumns(DataTable table, Dictionary<string, object> dictionary)
        {
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (table.Columns.Contains(pair.Key))
                    continue;

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

        private string GetDictionaryText(Dictionary<string, object> dictionary, string key)
        {
            if (dictionary == null || !dictionary.ContainsKey(key) || dictionary[key] == null)
                return string.Empty;

            return Convert.ToString(dictionary[key]) ?? string.Empty;
        }
    }
}
