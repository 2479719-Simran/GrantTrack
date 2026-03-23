using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace GrantTrack.Domain.Entities;
public class AuditLog
{
    [Key]
    public int AuditId { get; set; }

    [Required]
    [ForeignKey("UserIdNavigation")]
    public int UserId { get; set; }

    public DateTime TimeStamp { get; set; }
    [Required]
    [ForeignKey("ActionIdNavigation")]
    public int ActionId { get; set; }

    [MaxLength(200)]
    [Required]
    public string? Resource { get; set; }

      //------------------ForeignKey-------------------//
    public virtual User ? UserIdNavigation {get; set;}
    public virtual Operation ? ActionIdNavigation {get; set;}
}
