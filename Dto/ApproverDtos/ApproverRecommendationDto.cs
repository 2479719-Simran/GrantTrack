using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto.ApproverDtos;

/// <summary>
/// Combined view of a recommendation + its companion review row. We join
/// because the FE flow stores decision/notes on Recommendation but
/// score/comments on Review — both are written by the reviewer in a
/// single submission.
/// </summary>
public class ApproverRecommendationDto
{
    public int RecommendationId { get; set; }
    public int ApplicationId { get; set; }
    public int ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string ReviewerEmail { get; set; } = string.Empty;
    public ReviewDecision Decision { get; set; }
    public string RecommendationNotes { get; set; } = string.Empty;
    public int Score { get; set; }                   // 1..10 from the Review row, 0 if no review row
    public string ReviewComments { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
