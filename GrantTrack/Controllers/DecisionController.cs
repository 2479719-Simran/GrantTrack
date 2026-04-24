using GrantTrack.Helpers;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
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
        private readonly GrantTrackDbContext _context;
        public DecisionController(IDecisionService decisionService, GrantTrackDbContext context)
        {
            _decisionService = decisionService;
            _context = context;
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
        /// <summary>
        /// Endpoint for Auditors to verify governance.
        /// </summary>
        [Authorize(Roles = "Approver, Admin")]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int applicationId)
        {
            if (applicationId <= 0)
            {
                return BadRequest(new { message = "A valid ApplicationId is required." });
            }
            try
            {
                if (!await _decisionService.ApplicationExistsAsync(applicationId))
                {
                    return NotFound(new { message = "Application not found." });
                }
                var history = await _decisionService.GetDecisionHistoryAsync(applicationId);
                // AUDIT: Log this access for compliance
                await LogGovernanceView(applicationId);
                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Standard #1: Comment WHY - General exception handling to ensure API stability
                return StatusCode(500, new { message = "An internal error occurred.", details = ex.Message });
            }
        }
        private async Task LogGovernanceView(int appId)
        {
            var audit = new AuditLog
            {
                UserId = UserHelper.GetUserId(User), // Get ID from Token
                ActionId = 3, // Assuming 3 = 'View History'
                TimeStamp = DateTime.Now,
                Resource = $"Audited decision history for Application {appId}"
            };
            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
