using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace GrantTrack.Domain.Entities;

public class AuditLog
{
  [Key]
    public int AuditId { get; set; }

    public int UserId { get; set; }       // FK property
    public int ActionId { get; set; }     // FK property

    public DateTime TimeStamp { get; set; }

    [MaxLength(200)]
    [Required]
    public string? Resource { get; set; }

    // Navigation Properties
    [ForeignKey("UserId")]                // Points to the FK property above
    public virtual User UserNavigation { get; set; }

    [ForeignKey("ActionId")]             // Points to the FK property above
    public virtual Operation ActionNavigation { get; set; }
}
