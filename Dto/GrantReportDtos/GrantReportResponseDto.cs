namespace GrantTrack.Dto.GrantReportDtos;

/// <summary>
/// Read shape returned by the grant-report endpoints. The Status field is
/// the enum name as a string (Draft / Submitted / Returned / Verified) so
/// the FE never has to translate.
/// </summary>
public class GrantReportResponseDto
{
    public int GrantReportId { get; set; }
    public int ApplicationId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string Metrics { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string? Notes { get; set; }
    public string? EvidenceDocumentPath { get; set; }
}
