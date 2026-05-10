namespace GrantTrack.Dto.FinanceDtos;

/// <summary>
/// Row shape on the Finance Officer's queue page. Each row is an approved
/// application along with rollups so the FO can triage at a glance:
/// total disbursed so far, total paid out, remaining headroom against the
/// program budget, and a per-status counter on the tranches.
/// </summary>
public class FinanceApplicationListDto
{
    public int ApplicationId { get; set; }
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public decimal ProgramBudget { get; set; }
    public DateTime DecisionDate { get; set; }

    // Rollups
    public int DisbursementCount { get; set; }
    public decimal TotalDisbursed { get; set; }              // sum of every tranche regardless of status
    public decimal TotalPaid { get; set; }                   // sum of completed payments
    public decimal Remaining { get; set; }                   // ProgramBudget - TotalDisbursed (headroom)

    public int PendingCount { get; set; }
    public int ScheduledCount { get; set; }
    public int PaidCount { get; set; }
    public int PartiallyPaidCount { get; set; }
    public int CancelledCount { get; set; }
}
