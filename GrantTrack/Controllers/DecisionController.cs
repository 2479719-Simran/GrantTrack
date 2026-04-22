using GrantTrack.Helpers;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DecisionDtos;
using GrantTrack.Service.DecisionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DecisionController : ControllerBase
    {
        private readonly IDecisionService _decisionService;

        public DecisionController(IDecisionService decisionService)
        {
            _decisionService = decisionService;
        }

        /// <summary>
        /// Approves or rejects an application.
        /// </summary>
        [Authorize(Roles = nameof(UserRole.Approver))]
        [HttpPost]
        public async Task<IActionResult> CreateDecision([FromBody] DecisionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // CHANGE: Using the Common Helper class instead of a private method
                int approverId = UserHelper.GetUserId(User); 

                await _decisionService.CreateDecisionAsync(dto, approverId);
                return StatusCode(StatusCodes.Status201Created, "Decision recorded successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle cases where the helper can't find the user ID
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
            }
        }
    }
}