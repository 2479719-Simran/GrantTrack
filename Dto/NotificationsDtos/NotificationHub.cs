using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GrantTrack.Dto.NotificationsDtos;

/// <summary>
/// SignalR hub for live notification push. Authenticated via JWT — see
/// Program.cs OnMessageReceived for how the WS handshake reads the token
/// from the ?access_token= query param.
///
/// The hub itself only needs the [Authorize] attribute; SignalR's default
/// IUserIdProvider uses ClaimTypes.NameIdentifier (i.e. JwtRegisteredClaimNames.Sub)
/// from the JWT, so Clients.User(userId.ToString()) routes correctly to
/// the connected client.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Pushes a notification message to a specific user (by user-id).
    /// Called from server-side code (e.g. NotificationService) — clients
    /// don't invoke this directly.
    /// </summary>
    public async Task SendNotification(int userId, string message)
    {
        await Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
