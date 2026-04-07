using Microsoft.AspNetCore.Mvc;
using GrantTrack.Dto;
using GrantTrack.Service.ReviewFilterService;

namespace GrantTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewFilterController : ControllerBase
{
    private readonly IReviewFilterService _reviewService; // Interface use pannunga

    public ReviewFilterController(IReviewFilterService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignedReviews([FromQuery] ReviewFilterDto filter)
    {
        var reviews = await _reviewService.GetPagedReviewsAsync(filter);
        return Ok(reviews);
    }
}
}
