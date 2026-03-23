using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
[Table("Operation")]
[PrimaryKey("ActionId")]
public class Operation
{
    [Key]
    public int ActionId { get; set; }
    [MaxLength(200)]
    public string Description { get; set; }
    //--------------one to many relationship-------------------------//
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
