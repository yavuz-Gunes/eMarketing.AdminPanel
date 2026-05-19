using eMarketing.Service.Dtos;
using eMarketing.Web.State;
using System.Net.Http.Json;

namespace eMarketing.Web.Services;

public sealed class ProductApiClient : ApiClientBase
{
    public ProductApiClient(IHttpClientFactory httpClientFactory, AuthSession authSession)
        : base(httpClientFactory, authSession)
    {
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(string search = "", int categoryId = 0, int status = 1, CancellationToken cancellationToken = default)
    {
        string query = $"urunler?durum={status}&kategoriId={categoryId}&arama={Uri.EscapeDataString(search)}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<ProductDto>>(response, cancellationToken);
    }

    public async Task<int> CreateProductAsync(ProductSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("urunler", request, cancellationToken);
        ProductCreateResult result = await ReadRequiredAsync<ProductCreateResult>(response, cancellationToken);
        return result.UrunId;
    }

    public async Task UpdateProductAsync(int productId, ProductSaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PutAsJsonAsync($"urunler/{productId}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetProductStatusAsync(int productId, bool active, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"urunler/{productId}/durum", new StatusUpdateRequest { AktifMi = active }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(string search = "", int status = -1, CancellationToken cancellationToken = default)
    {
        string query = $"kategoriler?durum={status}&arama={Uri.EscapeDataString(search)}";
        HttpResponseMessage response = await CreateClient().GetAsync(query, cancellationToken);
        return await ReadRequiredAsync<IReadOnlyList<CategoryDto>>(response, cancellationToken);
    }

    public async Task<int> CreateCategoryAsync(CategorySaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PostAsJsonAsync("kategoriler", request, cancellationToken);
        CategoryCreateResult result = await ReadRequiredAsync<CategoryCreateResult>(response, cancellationToken);
        return result.KategoriId;
    }

    public async Task UpdateCategoryAsync(int categoryId, CategorySaveRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PutAsJsonAsync($"kategoriler/{categoryId}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task SetCategoryStatusAsync(int categoryId, bool active, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await CreateClient().PatchAsJsonAsync($"kategoriler/{categoryId}/durum", new StatusUpdateRequest { AktifMi = active }, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private sealed class StatusUpdateRequest
    {
        public bool AktifMi { get; set; }
    }

    private sealed class ProductCreateResult
    {
        public int UrunId { get; set; }
    }

    private sealed class CategoryCreateResult
    {
        public int KategoriId { get; set; }
    }
}
