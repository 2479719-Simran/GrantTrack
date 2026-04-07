using System;
using GrantTrack.Dto.RecommendationDto;

namespace GrantTrack.Service.RecommendationService;

public interface IRecommendationService
{
    Task<bool> SubmitReviewAsync(int ReviewId, int currentReviewerId, RecommendationRequestDto dto);

}
