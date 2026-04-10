using System;
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

    public async Task<List<Review>> GetPagedReviewsAsync(ReviewFilterDto filter)
    {
        var query = _context.Reviews
            .Where(r => r.ReviewerId == filter.ReviewerId)
            .AsQueryable();

        //filter based on decision
        if (filter.Decision.HasValue)
        {
            query = query.Where(r => _context.Recommendations
                    .Any(rec => rec.ApplicationId == r.ApplicationId && rec.Decision == filter.Decision));
        }

        var pagedData = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return pagedData;
    }
}
