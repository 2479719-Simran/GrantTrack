using Microsoft.AspNetCore.Mvc;
using GrantTrack.Dto;
using GrantTrack.Service.ReviewFilterService;
using Microsoft.AspNetCore.Authorization;

namespace GrantTrack.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReviewFilterController : ControllerBase
    {
        private readonly IReviewFilterService _reviewService;
        //purpose: The ReviewFilterController is responsible for handling requests related to filtering and retrieving reviews based on specific criteria. It provides an endpoint for reviewers to fetch their assigned reviews with pagination and filtering options.    
        public ReviewFilterController(IReviewFilterService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("reviews")]
        [Authorize(Roles = "Reviewer")]
        // This endpoint allows reviewers to fetch their assigned reviews based on the provided filter criteria, such as pagination and reviewer ID. It ensures that only authenticated reviewers can access their assigned reviews.
        public async Task<IActionResult> GetAssignedReviews([FromQuery] ReviewFilterDto filter)
        {
            var reviews = await _reviewService.GetPagedReviewsAsync(filter);
            return Ok(reviews);
        }
    }
}
