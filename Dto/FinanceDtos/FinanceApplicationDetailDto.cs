namespace GrantTrack.Dto.FinanceDtos;

/// <summary>
/// Full picture for the Finance Officer when they drill into a single
/// approved application: applicant + program info, the approver's decision
/// (so the FO knows the basis for funding), every disbursement tranche
/// with its payments, plus running totals.
/// </summary>
public class FinanceApplicationDetailDto
{
    public int ApplicationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }

    // Applicant block
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ApplicantPhone { get; set; } = string.Empty;

    // Program block
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string ProgramDescription { get; set; } = string.Empty;
    public decimal ProgramBudget { get; set; }
    public DateTime ProgramStartDate { get; set; }
    public DateTime ProgramEndDate { get; set; }

    // Decision block — only approved applications appear here, but we still
    // surface the approver name + notes so the FO understands the context.
    public int? DecisionId { get; set; }
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public string? DecisionValue { get; set; }
    public string? DecisionNotes { get; set; }
    public DateTime? DecisionDate { get; set; }

    // Disbursements + payments
    public List<FinanceDisbursementDto> Disbursements { get; set; } = new();

    // Rollups (echoed from list shape so the detail page doesn't have to recompute)
    public decimal TotalDisbursed { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Remaining { get; set; }
}

public class FinanceDisbursementDto
{
    public int DisbursementId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ScheduledDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string Status { get; set; } = string.Empty;     // "Pending" | "Scheduled" | "Paid" | "PartiallyPaid" | "Cancelled"

    public List<FinancePaymentDto> Payments { get; set; } = new();
    public decimal TotalPaid { get; set; }
    public decimal RemainingOnTranche { get; set; }        // Amount - TotalPaid
}

public class FinancePaymentDto
{
    public int PaymentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Method { get; set; } = string.Empty;     // "BankTransfer" | "Cheque" | "Cash" | "OnlineTransfer"
    public string Status { get; set; } = string.Empty;     // "Completed" usually
}
