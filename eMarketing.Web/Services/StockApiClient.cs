using System.Net.Http.Json;
using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class StockApiClient : ApiClientBase
{
    public StockApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<StockItemDto>> GetStocksAsync(int storeId, string search = "", bool onlyCritical = false, CancellationToken cancellationToken = default)
    {
        string query = $"bayi-stoklari?magazaId={storeId}&arama={Uri.EscapeDataString(search)}&sadeceKritik={onlyCritical.ToString().ToLowerInvariant()}&sadeceAktif=true";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<StockItemDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(int storeId, int productId = 0, int count = 25, CancellationToken cancellationToken = default)
    {
        string query = $"bayi-stoklari/hareketler?magazaId={storeId}&urunId={productId}&kayitSayisi={count}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<StockMovementDto>>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetCentralMovementsAsync(int productId, int count = 25, CancellationToken cancellationToken = default)
    {
        string query = $"bayi-stoklari/merkez-hareketler?urunId={productId}&kayitSayisi={count}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<StockMovementDto>>(response, cancellationToken);
    }

    public async Task TransferFromCentralAsync(int storeId, int productId, int quantity, string note = "", CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("bayi-stoklari/hareket", new
        {
            MagazaId = storeId,
            UrunId = productId,
            HareketTipi = "ManuelGiris",
            Miktar = quantity,
            Aciklama = string.IsNullOrWhiteSpace(note) ? "Merkezden bayiye transfer" : note
        }, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task IncreaseCentralStockAsync(int productId, int quantity, string note = "", CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("bayi-stoklari/merkez-stok/artir", new
        {
            UrunId = productId,
            Miktar = quantity,
            Aciklama = string.IsNullOrWhiteSpace(note) ? "Merkez stok artırma" : note
        }, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }
}
