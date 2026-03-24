using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;
[Table("Notification")]
[PrimaryKey("NotificationId")]
public class Notification
{
    [Key]
    public int NotificationId { get; set; }
    [ForeignKey("UserId")]
    public int UserId { get; set; }
    [ForeignKey("ApplicationId")]
    public int ApplicationId { get; set; }
    [Column(TypeName = "VARCHAR(MAX)")]
    public string Message { get; set; }
    [Column(TypeName = "VarChar(50)")]
    public string Category { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public virtual User ? UserIdNavigation { get; set; }
    public virtual Application ? ApplicationIdNavigation { get; set; }

}
