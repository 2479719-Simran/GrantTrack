using System;
using GrantTrack.Dto;
using GrantTrack.Dto.ReviewDtos;

namespace GrantTrack.Repository.ReviewRepository;

public interface IReviewRepository
{
    // Application IDs valid check
    Task<bool> ApplicationsExistAsync(List<int> appIds);

    /// <summary>
    /// Returns the application IDs from <paramref name="appIds"/> that are
    /// still in Draft status — i.e. ones that should NOT have a reviewer
    /// assigned. Empty list means everything is good to assign.
    /// </summary>
    Task<List<int>> GetDraftApplicationIdsAsync(List<int> appIds);

    // Reviewer (User) IDs valid check
    Task<bool> ReviewersExistAsync(List<int> reviewerIds);
    Task<int> GetPendingReviewCountAsync(int reviewerId);
    Task<bool> BulkAssignAsync(BulkAssignmentDto dto);
    Task<List<ReviewFilterResponseDto>> GetFilteredReviewsAsync(ReviewFilterRequestDto filter);
}
