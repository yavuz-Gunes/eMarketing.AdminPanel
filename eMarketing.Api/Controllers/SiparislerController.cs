using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/siparisler")]
[Authorize]
public sealed class SiparislerController : ControllerBase
{
    private readonly IOrderService _orderService;

    public SiparislerController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewOrders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> Get(
        [FromQuery] int? magazaId = null,
        [FromQuery] bool tumMagazalar = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _orderService.GetOrdersAsync(magazaId, tumMagazalar, cancellationToken));
    }

    [HttpGet("ozet")]
    [Authorize(Policy = "CanViewOrders")]
    public async Task<ActionResult<Dictionary<string, object>>> GetSummary(
        [FromQuery] int? magazaId = null,
        [FromQuery] bool tumMagazalar = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _orderService.GetOrderSummaryAsync(magazaId, tumMagazalar, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "CanViewOrders")]
    public async Task<ActionResult<OrderDetailResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        OrderDetailResponseDto? order = await _orderService.GetOrderDetailAsync(id, cancellationToken);
        return order == null ? NotFound("Sipariş bulunamadı.") : Ok(order);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<ActionResult<object>> Create([FromBody] OrderCreateRequest request, CancellationToken cancellationToken)
    {
        string validationMessage = ValidateCreateRequest(request);
        if (!string.IsNullOrWhiteSpace(validationMessage))
            return BadRequest(validationMessage);

        int orderId = await _orderService.CreateOrderAsync(request, cancellationToken);
        return Ok(new { SiparisId = orderId });
    }

    [HttpPost("sepet")]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<ActionResult<object>> CreateCart([FromBody] CartOrderCreateRequest request, CancellationToken cancellationToken)
    {
        string validationMessage = ValidateCartRequest(request);
        if (!string.IsNullOrWhiteSpace(validationMessage))
            return BadRequest(validationMessage);

        int orderId = await _orderService.CreateCartOrderAsync(request, cancellationToken);
        return Ok(new { SiparisId = orderId });
    }

    [HttpPatch("{id:int}/durum")]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SiparisDurumu))
            return BadRequest("Sipariş durumu zorunludur.");

        await _orderService.UpdateOrderStatusAsync(id, request.SiparisDurumu, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/iptal")]
    [Authorize(Policy = "CanManageOrders")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _orderService.CancelOrderAsync(id, cancellationToken);
        return NoContent();
    }

    private static string ValidateCreateRequest(OrderCreateRequest request)
    {
        if (request.CustomerStoreId == null || request.CustomerStoreId <= 0)
            return "Sipariş için bayi/mağaza seçimi zorunludur.";

        if (request.ProductId <= 0)
            return "Ürün seçimi zorunludur.";

        if (request.Quantity <= 0)
            return "Adet sıfırdan büyük olmalıdır.";

        if (request.TotalPrice < 0)
            return "Toplam tutar negatif olamaz.";

        return string.Empty;
    }

    private static string ValidateCartRequest(CartOrderCreateRequest request)
    {
        if (request.CustomerStoreId == null || request.CustomerStoreId <= 0)
            return "Sipariş için bayi/mağaza seçimi zorunludur.";

        if (request.BayiYetkiliId == null || request.BayiYetkiliId <= 0)
            return "Sipariş yetkilisi seçimi zorunludur.";

        if (request.Items.Count == 0)
            return "Sepette ürün bulunmalıdır.";

        foreach (OrderCreateItemRequest item in request.Items)
        {
            if (item.ProductId <= 0)
                return "Ürün seçimi zorunludur.";

            if (item.Quantity <= 0)
                return "Adet sıfırdan büyük olmalıdır.";

            if (item.TotalPrice < 0)
                return "Toplam tutar negatif olamaz.";
        }

        return string.Empty;
    }
}
