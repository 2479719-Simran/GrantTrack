using System;
using GrantTrack.Dto.ReviewDtos;

namespace GrantTrack.Service.ReviewService;

public interface IReviewService
{
    Task<(bool Success, string Message)> BulkAssignReviewersAsync(BulkAssignmentDto dto);

}
