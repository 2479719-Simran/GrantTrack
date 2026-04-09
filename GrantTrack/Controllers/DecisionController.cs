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
        [Authorize(Roles = "Approver")]
        [HttpPost]
        public async Task<IActionResult> CreateDecision([FromBody] DecisionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _decisionService.CreateDecisionAsync(dto);
                return StatusCode(StatusCodes.Status201Created, "Decision recorded successfully.");
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
                // Comment WHY: Catch-all to ensure the API doesn't crash on unhandled logic errors
                return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
            }
        }
    }
}