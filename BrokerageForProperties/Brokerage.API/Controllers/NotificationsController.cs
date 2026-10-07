using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Brokerage.API.Controllers;


// Provides authenticated users with access to their notifications.

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Buyer,Seller,Admin")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(
        NotificationService notificationService)
    {
        _notificationService =
            notificationService;
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int userId))
        {
            return Unauthorized();
        }

        List<NotificationDto> notifications =
            await _notificationService
                .GetUnreadAsync(
                    userId,
                    cancellationToken);

        return Ok(notifications);
    }

    [HttpPost("{notificationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(
        int notificationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int userId))
        {
            return Unauthorized();
        }

        bool marked =
            await _notificationService
                .MarkAsReadAsync(
                    userId,
                    notificationId,
                    cancellationToken);

        if (!marked)
        {
            return NotFound();
        }

        return Ok(
            new
            {
                success = true
            });
    }

    private bool TryGetCurrentUserId(
        out int userId)
    {
        string? userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return int.TryParse(
            userIdClaim,
            out userId);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int userId))
        {
            return Unauthorized();
        }

        List<NotificationDto> notifications =
            await _notificationService
                .GetAllAsync(
                    userId,
                    cancellationToken);

        return Ok(notifications);
    }
}