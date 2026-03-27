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
    [ForeignKey("UserId")]
    public int UserId { get; set; }

    public DateTime TimeStamp { get; set; }
    [Required]
    [ForeignKey("ActionId")]
    public int ActionId { get; set; }

    [MaxLength(200)]
    [Required]
    public string? Resource { get; set; }

      //------------------ForeignKey-------------------//
      public virtual User  UserIDNavigation {get; set;}
      public virtual Operation  ActionIDNavigation {get; set;}
}
