using eMarketing.Service.Dtos;
using eMarketing.Web.State;

namespace eMarketing.Web.Services;

public sealed class ProductApiClient : ApiClientBase
{
    public ProductApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(string search = "", int categoryId = 0, CancellationToken cancellationToken = default)
    {
        string query = $"urunler?durum=1&kategoriId={categoryId}&arama={Uri.EscapeDataString(search)}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<ProductDto>>(response, cancellationToken);
    }
}
