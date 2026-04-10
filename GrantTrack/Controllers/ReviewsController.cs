using GrantTrack.Dto.ReviewDtos;
using GrantTrack.Service.ReviewService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Bulk assigns reviewers to applications with strict validation.
        /// </summary>
        /// <param name="dto">The list of assignments.</param>
        /// <returns>ActionResult indicating success or specific validation error.</returns>
        [HttpPost("assignments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkAssign([FromBody] BulkAssignmentDto dto)
        {
            //Basic request validation
            if (dto == null || dto.Assignments == null || !dto.Assignments.Any())
            {
                return BadRequest(new { error = "Assignments list is empty!" });
            }

            // 2. Service layer logic call
            //ApplicationId exist check, ReviewerId exist check, 
            // Duplicate assignment check, and Workload check (max 5) nadakkum.
            var result = await _reviewService.BulkAssignReviewersAsync(dto);

            // 3. Response handling
            if (result.Success)
            {
                return Ok(new { message = "Bulk assignment successful!" });
            }
            return BadRequest(new { error = result.Message });
        }
    }
}