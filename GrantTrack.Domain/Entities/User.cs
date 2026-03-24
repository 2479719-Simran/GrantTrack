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
    [ForeignKey("RoleId")]
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
    //-------------------PrimaryKey---------------------//

    public List<Recommendation> Recommendations { get; set; }
    public List<Application> Applications { get; set; } = new List<Application>();
    public List<Review> Reviews { get; set; } = new List<Review>(); 
    
    public List<Decision> Decisions { get; set; } = new List<Decision>();
    // public virtual ICollection<User> Users{get; set;}=new List<User>(); 
    //-------------------ForeignKey---------------------//
    public virtual Role ? RoleIDNavigation {get; set;}
    public List<AuditLog> AuditLogs { get; set; } = new List<AuditLog>(); 
    public List<Notification> Notifications { get; set; }  = new List<Notification>(); 

    public List<Report> Reports { get; set; } = new List<Report>(); 
}
