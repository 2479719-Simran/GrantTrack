using GrantTrack.Dto.RecommendationDto;
using GrantTrack.Service.RecommendationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }
        [HttpPost("Recommendation")]
        public async Task<IActionResult> SubmitReview(int ReviewId, int currentReviewerId, RecommendationRequestDto dto)
        {
            // 1. DTO check ingayae mudunjidum
            if (dto == null) return BadRequest("Request body is missing");

            try
            {
                // 2. Service-ah call panrom
                var result = await _recommendationService.SubmitReviewAsync(ReviewId, currentReviewerId, dto);

                // 3. Service 'false' kudutha NotFound/BadRequest inga decide pannuvom
                if (!result)
                    return NotFound("Review record not found or data is invalid.");

                return Ok("Success!");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }
}
