using GrantTrack.Domain.Entities;
using GrantTrack.Dto.RecommendationDto;
using GrantTrack.Dto.ReviewDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.RecommendationRepository
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly GrantTrackDbContext _context;

        public RecommendationRepository(GrantTrackDbContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task<bool> SubmitReviewAndRecommendationAsync(Review review, RecommendationRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var recommendation = await _context.Recommendations
                    .FirstOrDefaultAsync(r => r.ApplicationId == review.ApplicationId
                                           && r.ReviewerId == review.ReviewerId);

                if (recommendation == null)
                {
                    recommendation = new Recommendation
                    {
                        ApplicationId = review.ApplicationId,
                        ReviewerId = review.ReviewerId,
                        Decision = dto.Decision,
                        Notes = dto.Notes,
                        Date = DateTime.Now
                    };
                    await _context.Recommendations.AddAsync(recommendation);
                }
                else
                {
                    recommendation.Decision = dto.Decision;
                    recommendation.Notes = dto.Notes;
                    recommendation.Date = DateTime.Now;
                    _context.Recommendations.Update(recommendation);
                }

                review.Score = dto.Score;
                review.Comments = dto.Comments;
                review.Date = DateTime.Now;
                _context.Reviews.Update(review);

                var result = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return result > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> BulkAssignAsync(BulkAssignmentDto dto)
        {
            var existingAppIds = await _context.Applications
                .Select(a => a.ApplicationId)
                .ToListAsync();

            foreach (var item in dto.Assignments)
            {
                if (!existingAppIds.Contains(item.ApplicationId))
                {
                    throw new ArgumentException($"Application ID {item.ApplicationId} does not exist.");
                }
            }

            var existingAssignments = await _context.Reviews
                .Select(r => new { r.ApplicationId, r.ReviewerId })
                .ToListAsync();

            foreach (var item in dto.Assignments)
            {
                if (existingAssignments.Any(a => a.ApplicationId == item.ApplicationId && a.ReviewerId == item.ReviewerId))
                {
                    throw new InvalidOperationException($"Application ID {item.ApplicationId} is already assigned to Reviewer {item.ReviewerId}.");
                }
            }

            var reviewsToCreate = dto.Assignments.Select(a => new Review
            {
                ApplicationId = a.ApplicationId,
                ReviewerId = a.ReviewerId,
                Score = a.Score,
                Comments = a.Comments ?? "",
                Date = DateTime.UtcNow
            }).ToList();

            await _context.Reviews.AddRangeAsync(reviewsToCreate);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<Recommendation?> GetRecommendationByReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            if (review == null) return null;

            return await _context.Recommendations
                .FirstOrDefaultAsync(r => r.ApplicationId == review.ApplicationId && r.ReviewerId == review.ReviewerId);
        }

        public async Task<bool> ApplicationExistsAsync(int applicationId)
        {
            return await _context.Applications.AnyAsync(a => a.ApplicationId == applicationId);
        }

        public Task<bool> IsApplicationInDraftAsync(int applicationId) =>
            _context.Applications
                .AnyAsync(a => a.ApplicationId == applicationId
                            && a.Status == ApplicationStatus.Draft);

        public async Task<bool> ReviewerExistsAsync(int reviewerId)
        {
            return await _context.Reviews.AnyAsync(r => r.ReviewerId == reviewerId);
        }

        public async Task<Recommendation?> GetRecommendationByApplicationAndReviewerAsync(int applicationId, int reviewerId)
        {
            return await _context.Recommendations
                .FirstOrDefaultAsync(r => r.ApplicationId == applicationId && r.ReviewerId == reviewerId);
        }
    }
}
