using System.Security.Claims;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DocumentDtos;
using GrantTrack.Service.DocumentServices;
using GrantTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Manages document uploads and downloads for grant applications.
/// </summary>
[ApiController]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentsController(IDocumentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Uploads a document for a grant application.
    /// POST /api/v1/documents
    /// Body: multipart/form-data with fields: ApplicationId, DocType, File
    /// </summary>
    [HttpPost("api/v1/documents")]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_485_760)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_485_760)]
    [ProducesResponseType(typeof(UploadDocumentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentRequestDto dto)
    {
        try
        {
            var applicantId = GetCurrentUserId();
            var result = await _service.UploadAsync(applicantId, dto);
            return CreatedAtAction(nameof(Upload), new { id = result.DocumentId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex) when (ex.Message == Messages.UnsupportedContentType)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message == Messages.FileSizeExceeded)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message == Messages.EmptyFile)
        {
            return BadRequest(new { error = ex.Message });
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
    /// Downloads a document file.
    /// Applicants can download their own documents; admins and reviewers can download any.
    /// GET /api/v1/documents/{documentId}
    /// </summary>
    [HttpGet("api/v1/documents/{documentId:int}")]
    [Authorize(Roles = nameof(UserRole.Applicant) + "," + nameof(UserRole.Admin) + "," + nameof(UserRole.Reviewer))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Download(int documentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            var result = await _service.DownloadAsync(userId, role, documentId);

            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
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

    // Private helpers
    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(Messages.UserNotAuthenticated);

        return int.Parse(claim);
    }

    private UserRole GetCurrentUserRole()
    {
        var roleClaim = User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException(Messages.UserNotAuthenticated);

        return Enum.Parse<UserRole>(roleClaim, ignoreCase: true);
    }
}