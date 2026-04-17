using GrantTrack.Dto.ReviewDtos;
using GrantTrack.Repository.ReviewRepository;

namespace GrantTrack.Service.ReviewService;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;

    public ReviewService(IReviewRepository reviewRepo)
    {
        _reviewRepo = reviewRepo;
    }

    public async Task<(bool Success, string Message)> BulkAssignReviewersAsync(BulkAssignmentDto dto)
    {
        //Validate if all Application IDs exist
        var appIds = dto.Assignments.Select(a => a.ApplicationId).Distinct().ToList();
        if (!await _reviewRepo.ApplicationsExistAsync(appIds))
        {
            return (false, "One or more Application IDs are invalid.");
        }

        //Validate if all Reviewer IDs exist
        var reviewerIds = dto.Assignments.Select(a => a.ReviewerId).Distinct().ToList();
        if (!await _reviewRepo.ReviewersExistAsync(reviewerIds))
        {
            return (false, "One or more Reviewer IDs do not exist.");
        }

        //Workload Check (Max 5 Pending)
        foreach (var reviewerId in reviewerIds)
        {
            int pendingCount = await _reviewRepo.GetPendingReviewCountAsync(reviewerId);
            int newIncoming = dto.Assignments.Count(a => a.ReviewerId == reviewerId);

            if ((pendingCount + newIncoming) > 5)
            {
                return (false, $"Reviewer ID {reviewerId} already has {pendingCount} pending reviews. Assignment failed.");
            }
        }

        //Save to DB using Repository
        var result = await _reviewRepo.BulkAssignAsync(dto);

        if (result)
            return (true, "Success");
        else
            return (false, "Database error during assignment.");
    }
}