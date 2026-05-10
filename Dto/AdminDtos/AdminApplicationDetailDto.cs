using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto.AdminDtos;

/// <summary>
/// Full audit-style view of a single application for the Admin: program info,
/// applicant info, every reviewer assigned, every recommendation submitted,
/// the final decision (if any), and document metadata.
/// Documents are listed by id + filename only — actual download still goes
/// through the existing per-application download endpoint with its auth checks.
/// </summary>
public class AdminApplicationDetailDto
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
    public DateTime ProgramStartDate { get; set; }
    public DateTime ProgramEndDate { get; set; }
    public decimal ProgramBudget { get; set; }

    // Pipeline rollups
    public List<AdminReviewerInfoDto> Reviewers { get; set; } = new();
    public List<AdminRecommendationInfoDto> Recommendations { get; set; } = new();
    public AdminDecisionInfoDto? Decision { get; set; }
    public List<AdminDocumentInfoDto> Documents { get; set; } = new();
}

public class AdminReviewerInfoDto
{
    public int ReviewId { get; set; }
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Comments { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public bool HasSubmittedRecommendation { get; set; }
}

public class AdminRecommendationInfoDto
{
    public int RecommendationId { get; set; }
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public ReviewDecision Decision { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class AdminDecisionInfoDto
{
    public int DecisionId { get; set; }
    public int ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverEmail { get; set; } = string.Empty;
    public string DecisionValue { get; set; } = string.Empty;     // "Approved" | "Rejected"
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class AdminDocumentInfoDto
{
    public int DocumentId { get; set; }
    public string DocType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}
