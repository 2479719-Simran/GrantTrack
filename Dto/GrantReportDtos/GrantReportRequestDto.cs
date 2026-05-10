using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.GrantReportDtos;

/// <summary>
/// Body shape for POST /api/v1/grantreport. The same DTO covers create
/// and update — the service treats it as an upsert keyed by ApplicationId.
/// </summary>
public class GrantReportRequestDto
{
    [Required]
    public int ApplicationId { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Scope can be at most 200 characters.")]
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Free-form metrics — typically quantitative outcomes the grantee
    /// achieved (people reached, units delivered, money spent on each
    /// activity, etc.). Stored as TEXT in the DB so any length is fine.
    /// </summary>
    [Required]
    public string Metrics { get; set; } = string.Empty;

    /// <summary>Optional commentary on the report.</summary>
    public string? Notes { get; set; }

    /// <summary>Optional link/path to evidence (PDF URL, drive link, etc.).</summary>
    [StringLength(500)]
    public string? EvidenceDocumentPath { get; set; }
}
