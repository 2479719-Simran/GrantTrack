using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ReviewDtos;
using GrantTrack.Dto;

namespace GrantTrack.Repository.ReviewRepository;

public class ReviewRepository : IReviewRepository
{
    private readonly GrantTrackDbContext _context;

    public ReviewRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Checks if all provided Application IDs exist in the database.
    /// </summary>
    public async Task<bool> ApplicationsExistAsync(List<int> appIds)
    {
        var uniqueIds = appIds.Distinct().ToList();
        var count = await _context.Applications
            .CountAsync(a => uniqueIds.Contains(a.ApplicationId));

        return count == uniqueIds.Count;
    }

    /// <summary>
    /// Checks if all provided Reviewer (User) IDs exist in the database.
    /// </summary>
    public async Task<bool> ReviewersExistAsync(List<int> reviewerIds)
    {
        var uniqueIds = reviewerIds.Distinct().ToList();
        var count = await _context.Users
            .CountAsync(u => uniqueIds.Contains(u.UserId));

        return count == uniqueIds.Count;
    }

    /// <summary>
    /// Counts reviews where Score is 0 (considered 'Pending' since Status column is absent).
    /// </summary>
    public async Task<int> GetPendingReviewCountAsync(int reviewerId)
    {
        return await _context.Reviews
            .Where(r => r.ReviewerId == reviewerId && r.Score == 0)
            .CountAsync();
    }

    /// <summary>
    /// Performs bulk insertion of new review assignments.
    /// </summary>
    public async Task<bool> BulkAssignAsync(BulkAssignmentDto dto)
    {
        var reviewsToCreate = dto.Assignments.Select(a => new Review
        {
            ApplicationId = a.ApplicationId,
            ReviewerId = a.ReviewerId,
            Score = 0, // Mark as pending
            Comments = "",
            Date = DateTime.UtcNow
        }).ToList();

        await _context.Reviews.AddRangeAsync(reviewsToCreate);

        // Returns true if records were successfully inserted
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<ReviewFilterResponseDto>> GetFilteredReviewsAsync(ReviewFilterRequestDto filter)
    {
        var query = from review in _context.Reviews
                    join app in _context.Applications on review.ApplicationId equals app.ApplicationId
                    join user in _context.Users on app.ApplicantId equals user.UserId
                    where review.ReviewerId == filter.ReviewerId
                    select new { review, app, user };

        if (filter.Decision.HasValue)
        {
            query = query.Where(x => _context.Recommendations
                .Any(rec => rec.ApplicationId == x.app.ApplicationId && rec.Decision == filter.Decision));
        }

        // Materialize first
        var results = await query.ToListAsync();

        // Remove duplicates in memory
        var distinctResults = results
            .GroupBy(x => new { x.review.ApplicationId, x.review.ReviewerId })
            .Select(g => g.First())
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ReviewFilterResponseDto
            {
                ReviewId = x.review.ReviewId,
                ApplicationId = x.app.ApplicationId,
                HolderName = x.user.Name,
                ReviewerId = x.review.ReviewerId,
                Decision = filter.Decision,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            })
            .ToList();

        return distinctResults;
    }

}