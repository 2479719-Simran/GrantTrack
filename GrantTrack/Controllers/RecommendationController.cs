using GrantTrack.Dto.RecommendationDto;
using GrantTrack.Dto.ReviewDtos;
using GrantTrack.Service.RecommendationService;
using GrantTrack.Service.ReviewService;
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
        private readonly IReviewService _reviewService;
        //<summary>
        //purpose: The RecommendationController is responsible for handling the submission of recommendations and scores by reviewers for specific reviews.
        /// Initializes a new instance of the <see cref="RecommendationController"/> class.
        /// </summary>
        /// <param name="recommendationService">The recommendation service.</param>
        public RecommendationController(IRecommendationService recommendationService, IReviewService reviewService)
        {
            _recommendationService = recommendationService;
            _reviewService = reviewService;
        }

        [HttpPost("recommendation")]
        [Authorize(Roles = "Reviewer")]
        public async Task<IActionResult> SubmitRecommendation([FromBody] RecommendationRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _recommendationService.SubmitReviewAsync(dto.ReviewerId, dto);
                if (!result)
                {
                    return BadRequest(new { error = "Failed to submit recommendation." });
                }
                return Ok(new { message = "Recommendation submitted successfully!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }
    }
}
