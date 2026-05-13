using System.Net.Http.Json;
using eMarketing.Service.Dtos;
using eMarketing.Web.Models;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class OrderApiClient : ApiClientBase
{
    public OrderApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(int? storeId, CancellationToken cancellationToken = default)
    {
        string query = storeId.HasValue
            ? $"siparisler?magazaId={storeId.Value}&tumMagazalar=false"
            : "siparisler?tumMagazalar=false";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<OrderDto>>(response, cancellationToken);
    }

    public async Task<OrderDetailResponseDto> GetOrderDetailAsync(int orderId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().GetAsync($"siparisler/{orderId}", cancellationToken);
        return await ReadRequiredAsync<OrderDetailResponseDto>(response, cancellationToken);
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"siparisler/{orderId}/durum", new OrderStatusUpdateRequest
        {
            SiparisDurumu = status
        }, cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CancelOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync($"siparisler/{orderId}/iptal", new { }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<int> CreateCartOrderAsync(StoreDto store, SiparisYetkilisiDto authority, IReadOnlyList<CartItem> items, CancellationToken cancellationToken = default)
    {
        var request = new CartOrderCreateRequest
        {
            CustomerName = store.MusteriAdi,
            CustomerEmail = string.Empty,
            CustomerPhone = store.Telefon,
            CustomerStoreId = store.MagazaId,
            OrderType = "Bayi",
            OrderSource = "Web",
            BayiYetkiliId = authority.BayiYetkiliId,
            Items = items.Select(item => new OrderCreateItemRequest
            {
                ProductId = item.Product.UrunId,
                Quantity = item.Quantity,
                TotalPrice = item.LineTotal
            }).ToList()
        };

        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("siparisler/sepet", request, cancellationToken);
        Dictionary<string, int> result = await ReadRequiredAsync<Dictionary<string, int>>(response, cancellationToken);
        return result.TryGetValue("SiparisId", out int orderId) ? orderId : 0;
    }
}
