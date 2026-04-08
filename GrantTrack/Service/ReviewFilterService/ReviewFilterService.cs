using System;
using System.Text.Json;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ReviewFilterService;

public class ReviewFilterService : IReviewFilterService
{
    private readonly GrantTrackDbContext _context;
    public ReviewFilterService(GrantTrackDbContext context)
    {
        _context = context;
    }

   public async Task<List<ReviewFilterDto>> GetPagedReviewsAsync(ReviewFilterRequestDto filter)
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

    var pagedData = await query
        .Skip((filter.PageNumber - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .Select(x => new ReviewFilterDto
        {
            ReviewId = x.review.ReviewId,
            ApplicationId = x.app.ApplicationId,
            HolderName = x.user.Name,
            ReviewerId = x.review.ReviewerId,
            Decision = filter.Decision,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        })
        .ToListAsync();

    return pagedData; 
}

}
