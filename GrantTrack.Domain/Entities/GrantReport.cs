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
    public int ComplianceCheckId { get; set; } 
    [Required]
    public int ApplicationId { get; set; } 
    [Required]
    public string Scope { get; set; }
    public string Metrics { get; set; } 
    [Required]
    public ReportStatus Status { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string? FileName { get; set; }      // Original File Name (bill.pdf)
    public byte[]? FileStream { get; set; }    // DB-la bytes-ah store aagum
    public string? Notes { get; set; }
}