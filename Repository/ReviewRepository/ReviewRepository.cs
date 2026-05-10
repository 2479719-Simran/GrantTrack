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
    /// Returns just the application IDs that are still Draft so the
    /// bulk-assign workflow can refuse them with a useful error message.
    /// </summary>
    public async Task<List<int>> GetDraftApplicationIdsAsync(List<int> appIds)
    {
        var uniqueIds = appIds.Distinct().ToList();
        return await _context.Applications
            .Where(a => uniqueIds.Contains(a.ApplicationId)
                     && a.Status == ApplicationStatus.Draft)
            .Select(a => a.ApplicationId)
            .ToListAsync();
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
            Comments = a.Comments,
            Date = DateTime.UtcNow
        }).ToList();

        await _context.Reviews.AddRangeAsync(reviewsToCreate);

        // Returns true if records were successfully inserted
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<ReviewFilterResponseDto>> GetFilteredReviewsAsync(ReviewFilterRequestDto filter)
    {
        // Step 1: Pull every review for this reviewer, joined with the
        // applicant + program info AND the matching Recommendation's decision
        // (null when the reviewer hasn't yet submitted one).
        //
        // The previous version of this method had two real bugs that surfaced
        // in the FE Reviewer tabs:
        //   • `Decision = filter.Decision` echoed the filter value back as
        //     the row's decision, so "All" rendered every row as Pending.
        //   • The Pending filter checked `Recommendation.Decision == Pending`
        //     but Pending isn't a row that exists — pending means there is
        //     NO Recommendation yet. So that tab always came back empty.
        var rows = await (
            from review in _context.Reviews
            join app in _context.Applications on review.ApplicationId equals app.ApplicationId
            join user in _context.Users on app.ApplicantId equals user.UserId
            where review.ReviewerId == filter.ReviewerId
            select new ReviewWithDecision
            {
                ReviewId = review.ReviewId,
                ApplicationId = review.ApplicationId,
                ReviewerId = review.ReviewerId,
                HolderName = user.Name,
                // Correlated subquery: the recommendation this reviewer
                // submitted for this application, if any. Cast to nullable
                // so EF can express "no row" as null.
                Decision = _context.Recommendations
                    .Where(r => r.ApplicationId == review.ApplicationId
                             && r.ReviewerId == review.ReviewerId)
                    .Select(r => (ReviewDecision?)r.Decision)
                    .FirstOrDefault()
            }
        ).ToListAsync();

        // Step 2: Apply the decision filter. "Pending" is the absence of a
        // recommendation row; the others match the actual stored decision.
        IEnumerable<ReviewWithDecision> filtered = rows;
        if (filter.Decision.HasValue)
        {
            var decisionFilter = filter.Decision.Value;
            filtered = decisionFilter == ReviewDecision.Pending
                ? rows.Where(x => x.Decision == null)
                : rows.Where(x => x.Decision == decisionFilter);
        }

        // Step 3: Dedupe (the BE may hold multiple Review rows per
        // application+reviewer because submit creates a new row instead of
        // updating the bulk-assign one), paginate, and project.
        return filtered
            .GroupBy(x => new { x.ApplicationId, x.ReviewerId })
            .Select(g => g.First())
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ReviewFilterResponseDto
            {
                ReviewId = x.ReviewId,
                ApplicationId = x.ApplicationId,
                HolderName = x.HolderName,
                ReviewerId = x.ReviewerId,
                // Surface the real status. Pending when no recommendation
                // exists yet, otherwise the recommendation's decision.
                Decision = x.Decision ?? ReviewDecision.Pending,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            })
            .ToList();
    }

    /// <summary>
    /// Internal projection holding a Review row alongside its matching
    /// Recommendation decision (null when the reviewer hasn't acted yet).
    /// </summary>
    private class ReviewWithDecision
    {
        public int ReviewId { get; set; }
        public int ApplicationId { get; set; }
        public int ReviewerId { get; set; }
        public string HolderName { get; set; } = string.Empty;
        public ReviewDecision? Decision { get; set; }
    }
}