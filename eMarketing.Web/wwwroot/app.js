const state = {
  apiBase: localStorage.getItem("em_api") || "http://localhost:5088/api",
  token: localStorage.getItem("em_token") || "",
  user: null,
  stores: [],
  products: [],
  authorities: [],
  selectedStore: null,
  selectedAuthority: null,
  cart: []
};

const el = (id) => document.getElementById(id);
const money = (value) => Number(value || 0).toLocaleString("tr-TR", { style: "currency", currency: "TRY" });
const initials = (text) => String(text || "?").split(/\s+/).filter(Boolean).slice(0, 2).map((x) => x[0]).join("").toLocaleUpperCase("tr-TR");

function setMessage(target, text, ok = false) {
  target.textContent = text || "";
  target.classList.toggle("ok", ok);
}

async function api(path, options = {}) {
  const headers = Object.assign({ "Content-Type": "application/json" }, options.headers || {});
  if (state.token) headers.Authorization = `Bearer ${state.token}`;

  const response = await fetch(`${state.apiBase}${path}`, Object.assign({}, options, { headers }));
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }

  if (response.status === 204) return null;
  return response.json();
}

async function login() {
  state.apiBase = el("apiBase").value.trim().replace(/\/$/, "");
  localStorage.setItem("em_api", state.apiBase);
  setMessage(el("loginMessage"), "");

  try {
    const result = await api("/auth/login", {
      method: "POST",
      body: JSON.stringify({
        KullaniciAdi: el("username").value.trim(),
        Sifre: el("password").value
      })
    });

    state.token = result.Token;
    localStorage.setItem("em_token", state.token);
    await bootstrap();
  } catch (error) {
    setMessage(el("loginMessage"), cleanError(error));
  }
}

async function bootstrap() {
  state.user = await api("/me");
  el("loginPanel").classList.add("hidden");
  el("appPanel").classList.remove("hidden");
  el("userTitle").textContent = `${state.user.KullaniciAdi} / ${state.user.Rol}`;
  await Promise.all([loadStores(), loadProducts()]);
}

async function loadStores() {
  state.stores = await api("/magazalar/secim?sadeceAktif=true");
  if (!state.selectedStore && state.stores.length > 0) {
    state.selectedStore = state.stores[0];
  }
  renderStores();
  if (state.selectedStore) {
    await loadAuthorities();
  }
}

async function loadProducts() {
  state.products = await api("/urunler?durum=1");
  renderProducts();
}

async function loadAuthorities() {
  state.selectedAuthority = null;
  state.authorities = [];
  if (!state.selectedStore) {
    renderAuthority();
    return;
  }

  state.authorities = await api(`/magazalar/${state.selectedStore.MagazaId}/siparis-yetkilileri`);
  state.selectedAuthority = state.authorities.length > 0 ? state.authorities[0] : null;
  renderAuthority();
}

function renderStores() {
  el("stores").innerHTML = "";
  state.stores.forEach((store) => {
    const card = document.createElement("article");
    card.className = `store-card ${state.selectedStore && state.selectedStore.MagazaId === store.MagazaId ? "active" : ""}`;
    card.innerHTML = `
      <h4>${store.MagazaAdi}</h4>
      <p>${store.MusteriAdi}</p>
      <p>${store.Sehir || ""} ${store.Ilce ? "/ " + store.Ilce : ""}</p>
    `;
    card.addEventListener("click", async () => {
      state.selectedStore = store;
      state.cart = [];
      renderStores();
      renderCart();
      await loadAuthorities();
    });
    el("stores").appendChild(card);
  });
}

function renderProducts() {
  const query = el("productSearch").value.trim().toLocaleLowerCase("tr-TR");
  const products = state.products.filter((product) => {
    return !query || `${product.UrunAdi} ${product.KategoriAdi}`.toLocaleLowerCase("tr-TR").includes(query);
  });

  el("products").innerHTML = "";
  products.forEach((product) => {
    const card = document.createElement("article");
    card.className = "product-card";
    const image = product.GorselUrl && product.GorselUrl.trim() ? product.GorselUrl.replaceAll("\\", "/") : "";
    const fallback = initials(product.UrunAdi);
    card.innerHTML = `
      ${image ? `<img src="${image}" alt="${product.UrunAdi}" onerror="this.outerHTML='<div class=&quot;product-fallback&quot;>${fallback}</div>'">` : `<div class="product-fallback">${fallback}</div>`}
      <h4>${product.UrunAdi}</h4>
      <div class="meta">${product.KategoriAdi || "Kategori yok"} / Stok: ${product.Stok}</div>
      <div class="price">${money(product.Fiyat)}</div>
      <button class="secondary" ${product.Stok <= 0 ? "disabled" : ""}>Sepete ekle</button>
    `;
    card.querySelector("button").addEventListener("click", () => addToCart(product));
    el("products").appendChild(card);
  });
}

function renderAuthority() {
  const box = el("authorityBox");
  if (!state.selectedStore) {
    box.textContent = "Mağaza seç.";
    box.classList.add("muted");
  } else if (!state.selectedAuthority) {
    box.textContent = "Bu mağaza için aktif sipariş yetkilisi yok.";
    box.classList.add("muted");
  } else {
    box.textContent = `Sipariş Yetkilisi: ${state.selectedAuthority.AdSoyad}`;
    box.classList.remove("muted");
  }

  el("storeHint").textContent = state.selectedStore
    ? `${state.selectedStore.MusteriAdi} / ${state.selectedStore.MagazaAdi}`
    : "Sipariş için mağaza seç.";
  renderCart();
}

function addToCart(product) {
  const existing = state.cart.find((item) => item.UrunId === product.UrunId);
  if (existing) {
    existing.Quantity += 1;
  } else {
    state.cart.push({ ...product, Quantity: 1 });
  }
  renderCart();
}

function renderCart() {
  const list = el("cartItems");
  list.innerHTML = "";
  state.cart.forEach((item) => {
    const row = document.createElement("div");
    row.className = "cart-item";
    row.innerHTML = `
      <div>
        <strong>${item.UrunAdi}</strong>
        <div class="meta">${item.Quantity} adet / ${money(item.Fiyat * item.Quantity)}</div>
      </div>
      <button>Sil</button>
    `;
    row.querySelector("button").addEventListener("click", () => {
      state.cart = state.cart.filter((x) => x.UrunId !== item.UrunId);
      renderCart();
    });
    list.appendChild(row);
  });

  const total = state.cart.reduce((sum, item) => sum + Number(item.Fiyat) * item.Quantity, 0);
  el("cartTotal").textContent = money(total);
  el("checkoutButton").disabled = !state.selectedStore || !state.selectedAuthority || state.cart.length === 0;
}

async function checkout() {
  setMessage(el("orderMessage"), "");
  if (!state.selectedStore || !state.selectedAuthority || state.cart.length === 0) {
    setMessage(el("orderMessage"), "Mağaza, sipariş yetkilisi ve sepet zorunludur.");
    return;
  }

  try {
    for (const item of state.cart) {
      await api("/siparisler", {
        method: "POST",
        body: JSON.stringify({
          CustomerName: state.selectedStore.MusteriAdi,
          CustomerEmail: "",
          CustomerPhone: "",
          ProductId: item.UrunId,
          Quantity: item.Quantity,
          TotalPrice: Number(item.Fiyat) * item.Quantity,
          CustomerStoreId: state.selectedStore.MagazaId,
          OrderType: "Bayi",
          OrderSource: "Web",
          BayiYetkiliId: state.selectedAuthority.BayiYetkiliId
        })
      });
    }
    state.cart = [];
    renderCart();
    setMessage(el("orderMessage"), "Sipariş oluşturuldu.", true);
  } catch (error) {
    setMessage(el("orderMessage"), cleanError(error));
  }
}

function cleanError(error) {
  const message = String(error.message || error).replace(/^"+|"+$/g, "");
  try {
    const parsed = JSON.parse(message);
    return parsed.message || parsed.Message || message;
  } catch {
    return message;
  }
}

function logout() {
  localStorage.removeItem("em_token");
  state.token = "";
  state.user = null;
  state.cart = [];
  el("appPanel").classList.add("hidden");
  el("loginPanel").classList.remove("hidden");
}

el("apiBase").value = state.apiBase;
el("loginButton").addEventListener("click", login);
el("logoutButton").addEventListener("click", logout);
el("refreshButton").addEventListener("click", () => Promise.all([loadStores(), loadProducts()]));
el("checkoutButton").addEventListener("click", checkout);
el("productSearch").addEventListener("input", renderProducts);

if (state.token) {
  bootstrap().catch(() => logout());
}
