using System;
using GrantTrack.Domain.Entities;
using AppEntity = GrantTrack.Domain.Entities.Application; 
namespace GrantTrack.Repository.NotificationRepository;
public interface INotificationRepository
{
  //User notifications fetch here
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId);

    //Unread count
    Task<int> GetUnreadCountAsync(int userId);

    //save notification
    Task AddNotificationAsync(Notification notification);

    //to save multiple informations
    Task AddRangeNotificationsAsync(IEnumerable<Notification> notifications);

    //status =="read"
    Task MarkAsReadAsync(int notificationId);

    //category based alert
    Task CreateAlertAsync(int userId, int appId, string message, string category);

    //to return the entity and save it
    Task<Notification> AddToDbAsync(Notification notification);

    //Application details will fetch 
    Task<AppEntity?> GetApplicationDetailsAsync(int applicationId);
}