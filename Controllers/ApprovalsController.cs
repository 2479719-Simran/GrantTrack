using GrantTrack.Domain.Entities;
using GrantTrack.Service.ApproverServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Approver-only endpoints. The Approver workflow needs:
///   • A queue of applications awaiting their decision.
///   • The recommendations from each Reviewer assigned to that application.
/// The actual decision POST stays in DecisionController.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = nameof(UserRole.Approver))]
public class ApprovalsController : ControllerBase
{
    private readonly IApproverService _service;

    public ApprovalsController(IApproverService service)
    {
        _service = service;
    }

    /// <summary>GET /api/v1/approvals/awaiting</summary>
    [HttpGet("awaiting")]
    public async Task<IActionResult> Awaiting()
    {
        try
        {
            var rows = await _service.GetAwaitingDecisionsAsync();
            return Ok(rows);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Could not load awaiting applications." });
        }
    }

    /// <summary>GET /api/v1/approvals/{applicationId}/recommendations</summary>
    [HttpGet("{applicationId:int}/recommendations")]
    public async Task<IActionResult> Recommendations([FromRoute] int applicationId)
    {
        try
        {
            var recs = await _service.GetRecommendationsForAsync(applicationId);
            return Ok(recs);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Could not load recommendations." });
        }
    }
}
