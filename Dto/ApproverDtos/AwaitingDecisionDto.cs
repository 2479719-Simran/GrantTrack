namespace GrantTrack.Dto.ApproverDtos;

/// <summary>
/// Row shape on the Approver's queue page. Each row represents an
/// application that has at least one recommendation but no Decision yet —
/// i.e. the reviewers are done and the approver needs to act.
/// </summary>
public class AwaitingDecisionDto
{
    public int ApplicationId { get; set; }
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;       // Application.Status as string

    // Reviewer aggregates so the approver can triage at a glance.
    public int RecommendationCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public double AverageScore { get; set; }                  // 0 when no scores yet
}
