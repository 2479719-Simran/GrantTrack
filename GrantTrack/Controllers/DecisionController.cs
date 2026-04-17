using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
                int approverId = GetCurrentUserId();
                await _decisionService.CreateDecisionAsync(dto, approverId);
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
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                     if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))

            {
                throw new InvalidOperationException("Unable to resolve user ID from JWT.");
            }
        return userId;
        }
    }
}







   
