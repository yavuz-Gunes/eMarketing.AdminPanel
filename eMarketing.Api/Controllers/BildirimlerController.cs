using eMarketing.Service.Dtos;
using eMarketing.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eMarketing.Api.Controllers;

[ApiController]
[Route("api/bildirimler")]
[Authorize(Policy = "CanViewDashboard")]
public sealed class BildirimlerController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public BildirimlerController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetList(
        [FromQuery] int? magazaId = null,
        [FromQuery] bool sadeceOkunmamis = false,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _notificationService.GetNotificationsAsync(magazaId, sadeceOkunmamis, limit, cancellationToken));
    }

    [HttpGet("okunmamis-sayisi")]
    public async Task<ActionResult<NotificationCountDto>> GetUnreadCount([FromQuery] int? magazaId = null, CancellationToken cancellationToken = default)
    {
        int count = await _notificationService.GetUnreadCountAsync(magazaId, cancellationToken);
        return Ok(new NotificationCountDto { OkunmamisSayisi = count });
    }

    [HttpPost("{id:int}/okundu")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("tumunu-okundu")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] int? magazaId = null, CancellationToken cancellationToken = default)
    {
        await _notificationService.MarkAllAsReadAsync(magazaId, cancellationToken);
        return NoContent();
    }

    [HttpPost("sistem-kontrol")]
    [Authorize(Policy = "CanManageNotifications")]
    public async Task<IActionResult> GenerateSystem([FromQuery] int? magazaId = null, [FromQuery] bool tumMagazalar = true, CancellationToken cancellationToken = default)
    {
        await _notificationService.GenerateSystemNotificationsAsync(magazaId, tumMagazalar, cancellationToken);
        return NoContent();
    }

    [HttpGet("admin")]
    [Authorize(Policy = "CanManageNotifications")]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAdmin(
        [FromQuery] string arama = "",
        [FromQuery] string tip = "",
        [FromQuery] int durum = -1,
        [FromQuery] int? magazaId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _notificationService.GetAdminNotificationsAsync(arama, tip, durum, magazaId, cancellationToken));
    }

    [HttpPost("admin")]
    [Authorize(Policy = "CanManageNotifications")]
    public async Task<ActionResult<object>> Create([FromBody] NotificationSaveRequest request, CancellationToken cancellationToken)
    {
        int id = await _notificationService.CreateNotificationAsync(request, cancellationToken);
        return Ok(new { BildirimId = id });
    }

    [HttpPatch("admin/{id:int}/durum")]
    [Authorize(Policy = "CanManageNotifications")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] NotificationStatusRequest request, CancellationToken cancellationToken)
    {
        await _notificationService.SetStatusAsync(id, request.AktifMi, cancellationToken);
        return NoContent();
    }

    [HttpDelete("admin/{id:int}")]
    [Authorize(Policy = "CanManageNotifications")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _notificationService.DeleteNotificationAsync(id, cancellationToken);
        return NoContent();
    }
}

public sealed class NotificationStatusRequest
{
    public bool AktifMi { get; set; }
}
