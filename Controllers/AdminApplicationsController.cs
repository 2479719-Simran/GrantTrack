using GrantTrack.Domain.Entities;
using GrantTrack.Service.AdminApplicationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Admin's full visibility hub. Lists every application across the system
/// with status + pipeline rollups, and lets the admin drill into a single
/// application to see all reviewers, recommendations, decision, and docs.
/// </summary>
[ApiController]
[Route("api/v1/admin/applications")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class AdminApplicationsController : ControllerBase
{
    private readonly IAdminApplicationService _service;

    public AdminApplicationsController(IAdminApplicationService service)
    {
        _service = service;
    }

    /// <summary>GET /api/v1/admin/applications</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        try
        {
            var rows = await _service.ListAllAsync();
            return Ok(rows);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Could not load applications." });
        }
    }

    /// <summary>GET /api/v1/admin/applications/{id}</summary>
    [HttpGet("{applicationId:int}")]
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
