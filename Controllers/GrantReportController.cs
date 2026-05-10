using System.Security.Claims;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.GrantReportDtos;
using GrantTrack.Service.GrantReportServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Grant Reports — utilization reports the grantee submits after receiving
/// funding. The compliance officer doesn't write these directly; their
/// "complete check" flow updates the status (Verified or Returned) via
/// ComplianceCheckService.
///
/// Endpoints:
///   POST /api/v1/grantreport                          – Applicant: upsert report (owner-checked)
///   GET  /api/v1/grantreport/by-application/{appId}   – multi-role read
/// </summary>
[ApiController]
[Route("api/v1/grantreport")]
[Authorize]
public class GrantReportController : ControllerBase
{
    private readonly IGrantReportService _service;

    public GrantReportController(IGrantReportService service)
    {
        _service = service;
    }

    /// <summary>
    /// POST /api/v1/grantreport — create a new report or update an existing
    /// (Submitted/Returned) one for the calling applicant's application.
    /// Verified reports are rejected at the service layer.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    public async Task<IActionResult> Upsert([FromBody] GrantReportRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            var result = await _service.UpsertAsync(GetUserId(), dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (InvalidOperationException ex)   { return BadRequest(new { error = ex.Message }); }
        catch (ArgumentException ex)           { return BadRequest(new { error = ex.Message }); }
        catch (Exception)                      { return StatusCode(500, new { error = "Could not save grant report." }); }
    }

    /// <summary>
    /// GET /api/v1/grantreport/by-application/{applicationId}.
    /// Returns the latest report for this application, or 204 NoContent
    /// when none has been submitted yet (so the FE can distinguish
    /// "missing" from "error" cleanly).
    /// </summary>
    [HttpGet("by-application/{applicationId:int}")]
    [Authorize(Roles =
        nameof(UserRole.Applicant) + "," +
        nameof(UserRole.Admin) + "," +
        nameof(UserRole.Approver) + "," +
        nameof(UserRole.FinanceOfficer) + "," +
        nameof(UserRole.ComplianceOfficer))]
    public async Task<IActionResult> GetByApplication([FromRoute] int applicationId)
    {
        try
        {
            var report = await _service.GetByApplicationAsync(
                applicationId, GetUserId(), IsPrivilegedReader());
            if (report is null) return NoContent();
            return Ok(report);
        }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (Exception)                      { return StatusCode(500, new { error = "Could not load grant report." }); }
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User not authenticated.");
        return int.Parse(claim);
    }

    /// <summary>
    /// Roles that get blanket read of a grant report (no per-app owner
    /// check). Mirrors the docs / disbursement controllers.
    /// </summary>
    private bool IsPrivilegedReader() =>
        User.IsInRole(nameof(UserRole.Admin))
        || User.IsInRole(nameof(UserRole.Approver))
        || User.IsInRole(nameof(UserRole.FinanceOfficer))
        || User.IsInRole(nameof(UserRole.ComplianceOfficer));
}
