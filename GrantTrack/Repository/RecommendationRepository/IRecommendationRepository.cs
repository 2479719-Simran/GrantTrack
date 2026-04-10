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
        Task<bool> ReviewerExistsAsync(int reviewerId);
        Task<Recommendation?> GetRecommendationByApplicationAndReviewerAsync(int applicationId, int reviewerId);
    }
}
