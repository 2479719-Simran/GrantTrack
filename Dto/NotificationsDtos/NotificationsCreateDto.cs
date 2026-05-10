using System;
using System.Runtime.InteropServices;

namespace GrantTrack.Dto.NotificationsDtos;

public class NotificationsCreateDto
{
    public int UserId { get; set; }
    public int ApplicantId { get; set; }
    public string Message { get; set; }=string.Empty;
    public string Category { get; set; }=string.Empty;

}
