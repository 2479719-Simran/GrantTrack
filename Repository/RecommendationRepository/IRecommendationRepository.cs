using GrantTrack.Domain.Entities;
using GrantTrack.Dto.RecommendationDto;
using GrantTrack.Dto.ReviewDtos;

namespace GrantTrack.Repository.RecommendationRepository
{
    public interface IRecommendationRepository
    {
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<bool> SubmitReviewAndRecommendationAsync(Review review, RecommendationRequestDto dto);
        Task<bool> BulkAssignAsync(BulkAssignmentDto dto);
        Task<Recommendation?> GetRecommendationByReviewAsync(int reviewId);
        Task<bool> ApplicationExistsAsync(int applicationId);

        /// <summary>
        /// True when the application is still in Draft status. Used by
        /// SubmitReviewAsync to refuse recommendations on un-submitted
        /// applications (mirrors the guard in ReviewService.BulkAssign).
        /// </summary>
        Task<bool> IsApplicationInDraftAsync(int applicationId);

        Task<bool> ReviewerExistsAsync(int reviewerId);
        Task<Recommendation?> GetRecommendationByApplicationAndReviewerAsync(int applicationId, int reviewerId);
    }
}
