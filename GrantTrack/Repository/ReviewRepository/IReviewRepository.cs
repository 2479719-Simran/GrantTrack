using System;
using GrantTrack.Dto;
using GrantTrack.Dto.ReviewDtos;

namespace GrantTrack.Repository.ReviewRepository;

public interface IReviewRepository
{
    // Application IDs valid check
    Task<bool> ApplicationsExistAsync(List<int> appIds);
    // Reviewer (User) IDs valid check 
    Task<bool> ReviewersExistAsync(List<int> reviewerIds);
    Task<int> GetPendingReviewCountAsync(int reviewerId);
    Task<bool> BulkAssignAsync(BulkAssignmentDto dto);
    Task<List<ReviewFilterResponseDto>> GetFilteredReviewsAsync(ReviewFilterRequestDto filter);
}
