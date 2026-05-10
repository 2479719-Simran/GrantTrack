using System;
using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AppEntity = GrantTrack.Domain.Entities.Application; 

namespace GrantTrack.Repository.NotificationRepository;

public class NotificationRepository : INotificationRepository
{
    private readonly GrantTrackDbContext _context;

    public NotificationRepository(GrantTrackDbContext context)
    {
        // Dependency Injection of Database Context
        _context = context; 
    }

    // Retrieves all notifications for a specific user ordered by newest first
    public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId)
    {
        return await _context.notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
    }

    // Gets the total count of notifications that are still marked as 'Unread' for a user
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.notifications
            .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.Unread);
    }

    // Persists a single notification object into the database
    public async Task AddNotificationAsync(Notification notification)
    {
        await _context.notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    // Persists a list of notification objects in a single database transaction (Bulk Insert)
    public async Task AddRangeNotificationsAsync(IEnumerable<Notification> notifications)
    {
        await _context.notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
    }

    // Updates a specific notification's status to 'Read' after a user views it
    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Read;
            _context.notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
    }

    // Creates and saves an alert with specific details like Category and Application ID
    public async Task CreateAlertAsync(int userId, int appId, string message, string category)
    {
        var notification = new Notification
        {
            UserId = userId,
            ApplicationId = appId,
            Message = message,
            Category = category,
            Status = NotificationStatus.Unread,
            CreatedDate = DateTime.Now
        };

        await _context.notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    // Adds a notification to the DB and returns the saved entity (useful for retrieving the generated ID)
    public async Task<Notification> AddToDbAsync(Notification notification)
    {
        await _context.notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    // Fetches detailed information about a specific Application linked to a notification
    public async Task<AppEntity?> GetApplicationDetailsAsync(int applicationId)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
    }
}