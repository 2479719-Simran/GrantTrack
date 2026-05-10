using System.Security.Claims;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ApplicationDtos;
using GrantTrack.Service.ApplicationServices;
using GrantTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Manages grant applications — creation and submission by applicants.
/// All endpoints require authentication. Role-specific endpoints require the "Applicant" role.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;
    public ApplicationsController(IApplicationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Creates a new grant application in Draft status for the authenticated applicant.
    /// POST /api/v1/applications
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateApplicationDto dto)
    {
        try
        {
            var applicantId = GetCurrentUserId();
            var result = await _service.CreateDraftAsync(dto, applicantId);
            return CreatedAtAction(nameof(Submit), new { id = result.ApplicationId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = Messages.Forbidden });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = Messages.UnexpectedError });
        }
    }

    /// <summary>
    /// Returns the current state of an application — used by the FE to
    /// refresh its local cache so the applicant sees the live status
    /// (e.g. flips from Submitted to Approved once the Approver decides).
    /// Owner-checked at the service layer.
    /// GET /api/v1/applications/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var applicantId = GetCurrentUserId();
            var result = await _service.GetByIdAsync(id, applicantId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = Messages.ApplicationNotFound });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = Messages.Forbidden });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = Messages.UnexpectedError });
        }
    }

    /// <summary>
    /// Submits an existing Draft application, transitioning its status to Submitted.
    /// Only the applicant who owns the application can submit it.
    /// POST /api/v1/applications/{id}/submit
    /// </summary>
    [HttpPost("{id}/submit")]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    [ProducesResponseType(typeof(ApplicationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            var applicantId = GetCurrentUserId();
            var result = await _service.SubmitAsync(id, applicantId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = Messages.ApplicationNotFound });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = Messages.Forbidden });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = Messages.UnexpectedError });
        }
    }
    /// <summary>
    /// Extracts the authenticated user's ID from their JWT claims.
    /// </summary>
    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(Messages.UserNotAuthenticated);

        return int.Parse(claim);
    }
}