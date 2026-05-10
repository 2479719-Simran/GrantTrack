using GrantTrack.Dto.ApproverDtos;

namespace GrantTrack.Service.ApproverServices;

public interface IApproverService
{
    /// <summary>
    /// Applications that have at least one Recommendation row and no Decision
    /// row yet. Sorted oldest-submitted first so the queue is FIFO-ish.
    /// </summary>
    Task<List<AwaitingDecisionDto>> GetAwaitingDecisionsAsync();

    /// <summary>
    /// All recommendations for the given application, joined with the matching
    /// Review row to surface the score + comments alongside decision + notes.
    /// </summary>
    Task<List<ApproverRecommendationDto>> GetRecommendationsForAsync(int applicationId);
}
