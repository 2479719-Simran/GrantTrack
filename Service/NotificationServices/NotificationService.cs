using System;
using AutoMapper;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.NotificationsDtos;
using GrantTrack.Repository.NotificationRepository;
using Microsoft.AspNetCore.SignalR;

namespace GrantTrack.Service.NotificationServices;

public class NotificationService : INotificationServices
{private readonly INotificationRepository _notificationRepo;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(INotificationRepository notificationRepo, IHubContext<NotificationHub> hubContext)
    {
        _notificationRepo = notificationRepo;
        _hubContext = hubContext;
    }

    // Implementation of the method that caused the error
    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
    {
        return await _notificationRepo.GetNotificationsByUserIdAsync(userId);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _notificationRepo.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await _notificationRepo.MarkAsReadAsync(notificationId);
    }

    public async Task SendNotificationAsync(NotificationsCreateDto dto)
    {
        // 1. Convert DTO to Entity
        var entity = new Notification
        {
            UserId = dto.UserId,
            ApplicationId = dto.ApplicantId,
            Message = dto.Message,
            Category = dto.Category,
            Status = NotificationStatus.Unread,
            CreatedDate = DateTime.Now
        };

        // 2. Logic is in Repository (DB Save)
        await _notificationRepo.AddNotificationAsync(entity);

        // 3. Real-time Push using HubContext
        await _hubContext.Clients.User(dto.UserId.ToString())
            .SendAsync("ReceiveNotification", new { 
                message = entity.Message, 
                category = entity.Category 
            });

    }
}