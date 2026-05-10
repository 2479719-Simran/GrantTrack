using System;
using GrantTrack.Domain.Entities;
using Microsoft.Identity.Client;

namespace GrantTrack.Dto.NotificationsDtos;

public class NotificationsReadDto
{
   public int NotificationId { get; set; }
   public int UserId { get; set; }
   public int ApplicationId { get; set; }
   public string Message { get; set; }=string.Empty;
   public string Category { get; set; }=string.Empty;
   public NotificationStatus Status { get; set; }=NotificationStatus.Unread;
   public DateTime CreatedDate { get; set; }=DateTime.Now;

}
