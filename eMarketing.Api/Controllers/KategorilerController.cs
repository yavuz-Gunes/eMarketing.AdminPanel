using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/kategoriler")]
[Authorize]
public sealed class KategorilerController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public KategorilerController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Get([FromQuery] string? arama = "", [FromQuery] int durum = 1, CancellationToken cancellationToken = default)
    {
        return Ok(await _categoryService.GetCategoriesAsync(arama, durum, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewProducts")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        CategoryDto? category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        if (category == null)
            return NotFound("Kategori bulunamadı.");

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageCatalog")]
    public async Task<ActionResult<object>> Create([FromBody] CategorySaveRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.KategoriAdi))
            return BadRequest("Kategori adı zorunludur.");

        int categoryId = await _categoryService.CreateCategoryAsync(request.KategoriAdi, cancellationToken);
        return Ok(new { KategoriId = categoryId });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageCatalog")]
    public async Task<IActionResult> Update(int id, [FromBody] CategorySaveRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.KategoriAdi))
            return BadRequest("Kategori adı zorunludur.");

        await _categoryService.UpdateCategoryAsync(id, request.KategoriAdi, request.AktifMi, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:int}/durum")]
    [Authorize(Policy = "CanManageCatalog")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] CategorySaveRequest request, CancellationToken cancellationToken)
    {
        await _categoryService.SetCategoryStatusAsync(id, request.AktifMi, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageCatalog")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        return NoContent();
    }
}
