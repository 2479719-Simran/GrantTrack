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

        // Only Submitted (or further-along) applications can have reviewers
        // assigned. Without this guard, a Draft can leak through the Reviewer
        // queue and into the Approver queue, where the BE rejects the
        // decision with "Cannot record a decision for a Draft application."
        // Catching it here surfaces the issue at the earliest point.
        var draftIds = await _reviewRepo.GetDraftApplicationIdsAsync(appIds);
        if (draftIds.Count > 0)
        {
            return (false,
                $"Application(s) {string.Join(", ", draftIds.Select(id => $"#{id}"))} are still in Draft. " +
                "The applicant must submit them before reviewers can be assigned.");
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