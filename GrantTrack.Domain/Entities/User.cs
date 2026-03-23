using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;
[Table("User")]
[PrimaryKey("UserId")]
public class User
{
    [Key]
    public int UserId { get; set; }
    [Required]
    public string? Name { get; set; }
    [ForeignKey("RoleIdNavigation")]
    [Required]
    public int RoleId { get; set; }
    public string? Email { get; set; }
    public int Phone { get; set; }
    public bool Status { get; set; }
    [Required]
    [Column(TypeName = "varchar(max)")]
    public string PasswordHash{get; set;} 
    [Required]
    [Column(TypeName = "varchar(max)")]
    public string PasswordSalt{get; set;}
    //-------------------One to many relationship---------------------//
    public virtual ICollection<AuditLog>AuditLogs{get; set;}=new List<AuditLog>();
    public virtual ICollection<Notification>Notifications{get; set;}=new List<Notification>();
    //-------------------ForeignKey---------------------//
    public virtual Role? RoleIdNavigation {get; set;}
}
