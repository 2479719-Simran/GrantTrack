namespace GrantTrack.Dto.ComplianceDtos;

/// <summary>
/// Row shape on the Compliance Officer's queue page. Each row is an
/// approved application (compliance only applies after approval) with
/// rollups so the CO can see at a glance which apps need a check, which
/// have been verified, and which were flagged.
/// </summary>
public class ComplianceApplicationListDto
{
    public int ApplicationId { get; set; }
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public DateTime DecisionDate { get; set; }

    // Per-type completion flags so the CO can scan which checks are missing.
    public bool HasFinancialCheck { get; set; }
    public bool HasOperationalCheck { get; set; }

    // Aggregates across all checks for this application.
    public int CheckCount { get; set; }
    public int VerifiedCount { get; set; }     // checks that finalised as Completed
    public int FlaggedCount { get; set; }      // checks that finalised as Flagged (issues found)
    public int ScheduledCount { get; set; }    // checks created but not yet finalised

    /// <summary>
    /// The current GrantReport.Status for this application, if any:
    /// "Draft" | "Submitted" | "Returned" | "Verified" | null when no report exists.
    /// Used by the FE to clarify check stage (BE check.Result alone is ambiguous).
    /// </summary>
    public string? ReportStatus { get; set; }
}
