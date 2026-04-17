using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto;

namespace GrantTrack.Service.ReviewFilterService;

public interface IReviewFilterService
{
    Task<List<ReviewFilterResponseDto>> GetPagedReviewsAsync(ReviewFilterRequestDto filter);
}
