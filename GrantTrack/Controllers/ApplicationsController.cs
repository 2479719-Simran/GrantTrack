using System.Security.Claims;
using GrantTrack.Dto.ApplicationDtos;
using GrantTrack.Service.ApplicationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class ApplicationsController(IApplicationService service) : ControllerBase
{
    // POST /api/applications → creates a Draft
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApplicationDto dto)
    {
        var applicantId = GetCurrentUserId();
        var result = await service.CreateDraftAsync(dto, applicantId);
        return CreatedAtAction(nameof(Submit), new { id = result.ApplicationId }, result);
    }

    // POST /api/applications/{id}/submit → status Submitted
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var applicantId = GetCurrentUserId();
        var result = await service.SubmitAsync(id, applicantId);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated.");
        return int.Parse(claim);
    }
}
