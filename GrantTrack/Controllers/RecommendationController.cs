using GrantTrack.Dto.RecommendationDto;
using GrantTrack.Service.RecommendationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        //<summary>
        //purpose: The RecommendationController is responsible for handling the submission of recommendations and scores by reviewers for specific reviews.
        /// Initializes a new instance of the <see cref="RecommendationController"/> class.
        /// </summary>
        /// <param name="recommendationService">The recommendation service.</param>
        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }
        [HttpPost("recommendations")]
        [Authorize(Roles = "Reviewer")]
        // This endpoint allows reviewers to submit their recommendations and scores for a specific review. It ensures that only the assigned reviewer can submit the recommendation for their review.
        public async Task<IActionResult> SubmitReview(int ReviewId, int currentReviewerId, RecommendationRequestDto dto)
        {
            // Validation: Check if the request body is null   
            if (dto == null) return BadRequest("Request body is missing");

            try
            {
                var result = await _recommendationService.SubmitReviewAsync(ReviewId, currentReviewerId, dto);
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
