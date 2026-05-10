using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.NotificationsDtos;

namespace GrantTrack.Service.NotificationServices;

public interface INotificationServices
{
   Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int notificationId);
    Task SendNotificationAsync(NotificationsCreateDto createDto);

}
