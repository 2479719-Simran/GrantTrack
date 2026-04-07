using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.RecommendationDto;
using GrantTrack.Dto.ReviewDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.RecommendationService;

public class RecommendationService : IRecommendationService
{
    private readonly GrantTrackDbContext _context;
    public RecommendationService(GrantTrackDbContext context)
    {
        _context = context;
    }

    public async Task<bool> SubmitReviewAsync(int ReviewId, int currentReviewerId, RecommendationRequestDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Review fetch - FindAsync badhula explicit-a ReviewId column vechu thedurom
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == ReviewId);

            if (review == null)
            {
                // Inga fail aana: ReviewId 7 database-la illa nu artham
                return false;
            }

            // 2. SECURITY CHECK
            if (review.ReviewerId != currentReviewerId)
            {
                throw new UnauthorizedAccessException("ReviewerId and CurrentReviewerId are not matched");
            }

            // 3. Related Recommendation fetch
            // Check: Unge table-la ApplicationId and ReviewerId correct-a irukanum
            var recommendation = await _context.Recommendations
                .FirstOrDefaultAsync(r => r.ApplicationId == review.ApplicationId
                                     && r.ReviewerId == review.ReviewerId);

            if (recommendation == null)
            {
                // Record illana, automatic-a ingaye create panniduvom macha!
                recommendation = new Recommendation
                {
                    ApplicationId = review.ApplicationId,
                    ReviewerId = review.ReviewerId,
                    Decision = dto.Decision, // Initial data
                    Notes = dto.Notes,
                    Date = DateTime.Now
                };
                await _context.Recommendations.AddAsync(recommendation);
            }
            else
            {
                // Record irundha, update pannuvom
                recommendation.Decision = dto.Decision;
                recommendation.Notes = dto.Notes;
                recommendation.Date = DateTime.Now;
                _context.Recommendations.Update(recommendation);
            }

            // 4. Update Review Table (Idhu kandippa venum)
            review.Score = dto.Score;
            review.Comments = dto.Comments;
            review.Date = DateTime.Now;
            _context.Reviews.Update(review);

            // 5. Save and Commit
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Console-la error-a paaka idhu help pannum
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}
