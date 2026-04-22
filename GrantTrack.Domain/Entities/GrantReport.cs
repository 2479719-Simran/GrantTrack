using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrantTrack.Domain.Entities;

public enum ReportStatus
{
    Draft,        // Grantee saves for later
    Submitted,    // Sent for compliance check
    Returned,     // Compliance officer asks for changes
    Verified      // Compliance check passed
}

[Table("GrantReport")]
public class GrantReport
{
    [Key]
    public int GrantReportId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public virtual Application? ApplicationIdNavigation { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR(200)")]
    public string Scope { get; set; }

    [Column(TypeName = "TEXT")]
    public string Metrics { get; set; } // 'Metrix' spelling 'Metrics' nu mathirukken

    [Required]
    [Column(TypeName = "VARCHAR(200)")]
    public ReportStatus Status { get; set; }

    public DateTime SubmittedDate { get; set; }

    // Evidence/Documents upload panna path inga irukanum
    [Column(TypeName = "VARCHAR(500)")]
    public string? EvidenceDocumentPath { get; set; }

    public string? Notes { get; set; }
}