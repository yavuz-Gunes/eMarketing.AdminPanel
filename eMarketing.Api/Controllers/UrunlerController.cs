using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/urunler")]
[Authorize]
public sealed class UrunlerController : ControllerBase
{
    private readonly IProductService _productService;

    public UrunlerController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Get(
        [FromQuery] string? arama = "",
        [FromQuery] int durum = 1,
        [FromQuery] int kategoriId = 0,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _productService.GetProductsAsync(arama, durum, kategoriId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        ProductDto? product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
            return NotFound("Ürün bulunamadı.");

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<ActionResult<object>> Create([FromBody] ProductSaveRequest request, CancellationToken cancellationToken)
    {
        string validationMessage = ValidateProduct(request);
        if (!string.IsNullOrWhiteSpace(validationMessage))
            return BadRequest(validationMessage);

        int productId = await _productService.CreateProductAsync(request, cancellationToken);
        return Ok(new { UrunId = productId });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductSaveRequest request, CancellationToken cancellationToken)
    {
        string validationMessage = ValidateProduct(request);
        if (!string.IsNullOrWhiteSpace(validationMessage))
            return BadRequest(validationMessage);

        await _productService.UpdateProductAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/durum")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] ProductStatusRequest request, CancellationToken cancellationToken)
    {
        await _productService.SetProductStatusAsync(id, request.AktifMi, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(id, cancellationToken);
        return NoContent();
    }

    private static string ValidateProduct(ProductSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UrunAdi))
            return "Ürün adı zorunludur.";

        if (request.KategoriId <= 0)
            return "Kategori seçimi zorunludur.";

        if (request.Fiyat < 0 || request.Stok < 0)
            return "Fiyat ve stok negatif olamaz.";

        return string.Empty;
    }
}

public sealed class ProductStatusRequest
{
    public bool AktifMi { get; set; }
}
