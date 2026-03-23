using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;
[Table("Report")]
[PrimaryKey("ReportId")]
public class Report
{
    [Key]
    public int ReportId { get; set; } 
    [ForeignKey("UserIdNavigation")]
    public int UserId { get; set; }
    [Column(TypeName = "VARCHAR(200)")]
    public string Scope { get; set; }  
    public string Metrix { get; set; }
    public DateTime GeneratedDate { get; set; }
    public virtual User ? UserIdNavigation { get; set; }
}
