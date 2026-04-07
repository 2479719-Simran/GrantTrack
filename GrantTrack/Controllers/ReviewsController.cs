using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GrantTrack.Dto.ReviewDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GrantTrack.Service.ReviewService;
using Microsoft.AspNetCore.Authorization;

namespace GrantTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("assignment")]
        public async Task<IActionResult> BulkAssign([FromBody] BulkAssignmentDto dto)
        {
            // validation
            if (dto == null || dto.Assignments == null || !dto.Assignments.Any())
            {
                return BadRequest("Assignments list is empty!");
            }

            //service layer logic
            var result = await _reviewService.BulkAssignReviewersAsync(dto);

            //based on result response will be given
            if (result)
            {
                return Ok("Success");
            }

            return StatusCode(500, "Server Error");
        }
    }
}
