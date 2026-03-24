using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
namespace GrantTrack.Domain.Entities;


[Table("Operation")]
[PrimaryKey("ActionId")]
public class Operation
{
    [Key]
    public int ActionId { get; set; }
    [MaxLength(200)]
    public string Description { get; set; }
    // public virtual ICollection<Operation>Operations{get; set;}=new List<Operation>();

    public List<AuditLog> AuditLogs { get; set; }   = new List<AuditLog>();
}
