namespace GrantTrack.Dto.AdminDtos;

/// <summary>
/// Row shape on the Admin's "all applications" hub. Surfaces enough context
/// for the admin to triage at a glance without drilling in.
/// </summary>
public class AdminApplicationListDto
{
    public int ApplicationId { get; set; }
    public int ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public int ProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;        // Application.Status as string
    public DateTime SubmittedDate { get; set; }

    // Pipeline progress at a glance.
    public int ReviewerCount { get; set; }
    public int RecommendationCount { get; set; }
    public int DocumentCount { get; set; }
    /// <summary>"Approved", "Rejected" or null when no Decision row exists yet.</summary>
    public string? FinalDecision { get; set; }
    public DateTime? DecisionDate { get; set; }
}
