namespace GrantTrack.Dto.ComplianceDtos;

/// <summary>
/// Full picture for the Compliance Officer when they drill into one
/// approved application: applicant + program + decision context, the
/// (single) Grant Report status, and every compliance check ever scheduled
/// against this application.
/// </summary>
public class ComplianceApplicationDetailDto
{
    public int ApplicationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }

    // Applicant
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ApplicantPhone { get; set; } = string.Empty;

    // Program
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ProgramDescription { get; set; } = string.Empty;
    public decimal ProgramBudget { get; set; }
    public DateTime ProgramStartDate { get; set; }
    public DateTime ProgramEndDate { get; set; }

    // Decision context — only Approved apps reach the CO, but we surface
    // who approved + their notes so the CO has the full grant context.
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? DecisionDate { get; set; }

    // Grant Report state — completion of a check requires a Report row.
    // Null when the applicant hasn't submitted a report yet.
    public int? GrantReportId { get; set; }
    public string? ReportStatus { get; set; }   // "Draft" | "Submitted" | "Returned" | "Verified"
    public string? ReportScope { get; set; }
    public string? ReportNotes { get; set; }
    public DateTime? ReportSubmittedDate { get; set; }

    // The actual checks for this application.
    public List<ComplianceCheckInfoDto> Checks { get; set; } = new();
}

public class ComplianceCheckInfoDto
{
    public int CheckId { get; set; }
    public int ApplicationId { get; set; }
    public string Type { get; set; } = string.Empty;       // "Financial" | "Operational"
    public string Result { get; set; } = string.Empty;     // raw enum: "Completed" | "Flagged"
    public DateTime Date { get; set; }
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Derived UX-level stage so the FE doesn't have to re-implement the
    /// "Flagged means scheduled OR Flagged means review-found-issues"
    /// logic. Computed from the joined GrantReport.Status:
    ///   • "Scheduled" — no GrantReport, or report not yet Verified/Returned
    ///   • "Verified"  — GrantReport.Status == Verified (Result == Completed)
    ///   • "Returned"  — GrantReport.Status == Returned (Result == Flagged)
    /// </summary>
    public string Stage { get; set; } = string.Empty;
}
