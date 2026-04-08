using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ReviewDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ReviewService;

public class ReviewService : IReviewService
{
    private readonly GrantTrackDbContext _context;

    public ReviewService(GrantTrackDbContext context)
    {
        _context = context;
    }

    public async Task<bool> BulkAssignReviewersAsync(BulkAssignmentDto dto)
    {
        // Transaction start - All or Nothing!

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in dto.Assignments)
            {
                //it will check weather reviewer has less then 5 records in pending list
                int pendingCount = await _context.Reviews.CountAsync(r => r.ReviewerId == item.ReviewerId && r.Score == 0);

                //if there is less then 5 then it will continue the process
                if (pendingCount >= 5) continue;

                // Check if assignment already exists to find duplicates
                bool exists = await _context.Reviews.AnyAsync<Review>(r =>
                    r.ApplicationId == item.ApplicationId &&
                    r.ReviewerId == item.ReviewerId);

                if (!exists)
                {
                    //create Review entry
                    var newReview = new Review
                    {
                        ApplicationId = item.ApplicationId,
                        ReviewerId = item.ReviewerId,
                        Date = DateTime.Now,
                        Comments = item.Comments,
                        Score = item.Score
                    };
                    _context.Reviews.Add(newReview);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

}