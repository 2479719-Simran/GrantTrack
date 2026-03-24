using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;
[Table("ComplianceCheck")]
[PrimaryKey("CheckId")]
public class ComplianceCheck
{
    [Key]
    public int CheckId { get; set; }
    [ForeignKey("ApplicationIdNavigation")]
    [Required]
    public int ApplicationId { get; set; }//foreign key to Application

    [Column(TypeName = "VARCHAR(200)")]
    public string Type { get; set; }

    [Column(TypeName = "VARCHAR(200)")]
    public string Result { get; set; }

    public DateTime Date { get; set; }

    public string Notes { get; set; }

    public virtual Application? ApplicationIdNavigation { get; set; }
}
