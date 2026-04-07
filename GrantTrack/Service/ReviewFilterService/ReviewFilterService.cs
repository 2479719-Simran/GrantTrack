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


        var pagedData = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return pagedData;
    }

}
