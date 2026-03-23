using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;
[Table("GrantReport")]
[PrimaryKey("GrantReportId")]
public class GrantReport
{
    [Key]
    public int GrantReportId { get; set; }
    [ForeignKey("ApplicationIdNavigation")]
    public int ApplicationId { get; set; }
    [Column(TypeName = "VARCHAR(200)")]
    public string Scope { get; set; }
    [Column(TypeName = "Text")]
    public string Metrix { get; set; }
    public DateTime SubmittedDate { get; set; }
    public bool Status { get; set; }

    //------------------ForeignKey-------------------//
    public virtual Application ? ApplicationIdNavigation { get; set; }
}
