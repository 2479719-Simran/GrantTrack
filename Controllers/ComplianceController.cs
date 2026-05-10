using GrantTrack.Domain.Entities;
using GrantTrack.Service.ComplianceListServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Read-side endpoints for the Compliance Officer's hub. Write operations
/// (schedule a check, complete a check) live on the existing
/// ComplianceCheckController; this controller provides the queue + drill-in
/// the FE needs to present that workflow.
/// </summary>
[ApiController]
[Route("api/v1/compliance")]
[Authorize(Roles = nameof(UserRole.ComplianceOfficer))]
public class ComplianceController : ControllerBase
{
    private readonly IComplianceListService _service;

    public ComplianceController(IComplianceListService service)
    {
        _service = service;
    }

    /// <summary>GET /api/v1/compliance/applications</summary>
    [HttpGet("applications")]
    public async Task<IActionResult> ListApproved()
    {
        try
        {
            var rows = await _service.ListApprovedAsync();
            return Ok(rows);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Could not load approved applications." });
        }
    }

    /// <summary>GET /api/v1/compliance/applications/{id}</summary>
    [HttpGet("applications/{applicationId:int}")]
    public async Task<IActionResult> GetDetail([FromRoute] int applicationId)
    {
        try
        {
            var detail = await _service.GetDetailAsync(applicationId);
            if (detail is null)
                return NotFound(new { message = "Application not found." });
            return Ok(detail);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Could not load application detail." });
        }
    }
}
