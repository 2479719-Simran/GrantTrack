using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Domain.Entities;
public enum ComplianceType
{
    Financial,
    Operational
}

public enum ComplianceResult
{
    Completed,
    Flagged,
    Returned
}

[Table("ComplianceCheck")]
public class ComplianceCheck
{
    [Key]
    public int ComplianceCheckId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public virtual Application? ApplicationIdNavigation { get; set; }

    /// <summary>
    /// Stores the Enum as a string in the DB (Financial or Operational)
    /// </summary>
    [Required]
    [Column(TypeName = "VARCHAR(200)")]
    public ComplianceType Type { get; set; }

    [Column(TypeName = "VARCHAR(200)")]
    public ComplianceResult Result { get; set; }

    public DateTime Date { get; set; }

    public string Notes { get; set; }
}