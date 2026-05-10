using System.Security.Claims;
using GrantTrack.Domain.Entities;
using GrantTrack.Service.FinanceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Read-only disbursement + payment view for a single application,
/// shared across roles. Mirrors the pattern used by DocumentsController:
///   • Applicant — owner-checked (must own the application)
///   • Admin / Approver / FinanceOfficer / ComplianceOfficer — blanket access
///
/// The data shape is identical to what the Finance Officer's detail page
/// consumes (FinanceDisbursementDto), so the FE can render one shared
/// "funding panel" component on every page that needs it.
/// </summary>
[ApiController]
[Route("api/v1/applications/{applicationId:int}/disbursements")]
[Authorize] // class-level: must be authenticated. Per-method narrows.
public class ApplicationDisbursementsController : ControllerBase
{
    private readonly IFinanceService _service;

    public ApplicationDisbursementsController(IFinanceService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/v1/applications/{applicationId}/disbursements
    /// Returns the list of disbursement tranches with embedded payments.
    /// </summary>
    [HttpGet]
    [Authorize(Roles =
        nameof(UserRole.Applicant) + "," +
        nameof(UserRole.Admin) + "," +
        nameof(UserRole.Approver) + "," +
        nameof(UserRole.FinanceOfficer) + "," +
        nameof(UserRole.ComplianceOfficer))]
    public async Task<IActionResult> List([FromRoute] int applicationId)
    {
        try
        {
            // Applicants can only see their own application's tranches —
            // every other role gets blanket read because they have a
            // legitimate operational reason to see the funding state.
            if (User.IsInRole(nameof(UserRole.Applicant))
                && !User.IsInRole(nameof(UserRole.Admin)))
            {
                var ownerId = await _service.GetApplicantIdAsync(applicationId);
                if (ownerId is null)
                    return NotFound(new { message = "Application not found." });

                var currentUserId = GetUserId();
                if (ownerId != currentUserId)
                    return StatusCode(403, new { error = "You don't have access to this application's disbursements." });
            }

            var rows = await _service.ListDisbursementsForApplicationAsync(applicationId);
            return Ok(rows);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Could not load disbursements." });
        }
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User not authenticated.");
        return int.Parse(claim);
    }
}
