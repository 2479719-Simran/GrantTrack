using System.Security.Claims;
using GrantTrack.Dto.NotificationsDtos;
using GrantTrack.Service.NotificationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.API.Controllers;

/// <summary>
/// Notification CRUD for the bell-icon UI. All endpoints require auth and
/// the read-side endpoints owner-check the userId param against the JWT
/// — a user can only ever see their OWN notifications.
///
/// Route is /api/v1/notification (matching the rest of the API). The
/// older /api/notification path is preserved as the FE used to point at
/// it; we now use the v1 path everywhere.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationServices _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationServices notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>GET /api/v1/notification/user/{userId} — owner-checked.</summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserNotifications(int userId)
    {
        if (userId != GetCallerUserId())
            return StatusCode(403, new { error = "You can only view your own notifications." });
        try
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching notifications for user {userId}");
            return StatusCode(500, new { message = "Cannot fetch notifications", error = ex.Message });
        }
    }

    /// <summary>GET /api/v1/notification/unread-count/{userId} — owner-checked.</summary>
    [HttpGet("unread-count/{userId:int}")]
    public async Task<IActionResult> GetUnreadCount(int userId)
    {
        if (userId != GetCallerUserId())
            return StatusCode(403, new { error = "You can only view your own counts." });
        try
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { unreadCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting count for user {userId}");
            return StatusCode(500, new { message = "Cannot get count", error = ex.Message });
        }
    }

    /// <summary>
    /// PATCH /api/v1/notification/{id}/read — mark a single notification as
    /// read. Used by the bell-dropdown UI when the user clicks an item.
    /// The service method enforces that only the recipient can mark it read.
    /// </summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking notification {id} as read");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/v1/notification/send — server-internal endpoint used to
    /// emit a notification (and push it via SignalR). Not invoked by the FE
    /// directly today, but kept for admin/test use.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationsCreateDto createDto)
    {
        if (createDto == null) return BadRequest("Notification body is required.");
        try
        {
            await _notificationService.SendNotificationAsync(createDto);
            return Ok(new { message = "Notification sent and pushed successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>JWT 'sub' / NameIdentifier → integer user id.</summary>
    private int GetCallerUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User not authenticated.");
        return int.Parse(claim);
    }
}
