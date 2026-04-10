using System;
using GrantTrack.Dto.ReviewDtos;

namespace GrantTrack.Service.ReviewService;

public interface IReviewService
{
    Task<bool> BulkAssignReviewersAsync(BulkAssignmentDto dto);
}
