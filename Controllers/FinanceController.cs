using GrantTrack.Domain.Entities;
using GrantTrack.Service.FinanceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Read-side endpoints for the Finance Officer's hub. Write operations
/// (create disbursement, update tranche, record payment) live in
/// DisbursementController; this controller exists only to give the FO a
/// queue + drill-in over their approved-application workload.
/// </summary>
[ApiController]
[Route("api/v1/finance")]
[Authorize(Roles = nameof(UserRole.FinanceOfficer))]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _service;

    public FinanceController(IFinanceService service)
    {
        _service = service;
    }

    /// <summary>GET /api/v1/finance/applications</summary>
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

    /// <summary>GET /api/v1/finance/applications/{id}</summary>
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
