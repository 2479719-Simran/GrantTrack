using System.Security.Claims;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DocumentDtos;
using GrantTrack.Service.DocumentServices;
using GrantTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Manages document uploads for grant applications.
/// Phase 1 — POST upload-url to get the upload URL + token.
/// Phase 2 — PUT the file to the returned URL using that token.
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
    /// Returns a short-lived upload URL and headers for direct file upload.
    /// POST /api/v1/documents/upload-url
    /// </summary>
    [HttpPost("api/v1/documents/upload-url")]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    [ProducesResponseType(typeof(GenerateUploadUrlResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUploadUrl([FromBody] GenerateUploadUrlRequestDto dto)
    {
        try
        {
            var applicantId = GetCurrentUserId();
            var result = await _service.GenerateUploadUrlAsync(applicantId, dto);
            var token = result.Headers["X-Upload-Token"];
            result.Url = $"/api/v1/documents/upload?token={token}";

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message == Messages.FileSizeExceeded)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new { error = ex.Message });
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
    /// Receives the file, saves to local storage, computes SHA-256 hash,
    /// and updates the document record with hash + version.
    /// PUT /api/v1/documents/upload?token={token}
    /// Body: multipart/form-data with fields: ApplicationId, DocumentId, File
    /// </summary>
    [HttpPut("api/v1/documents/upload")]
    [AllowAnonymous] // Auth handled by the upload token, not the user JWT
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_485_760)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_485_760)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Upload(
        [FromQuery] string token,
        [FromForm] ConfirmUploadRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { error = Messages.InvalidUploadToken });

        if (dto.File == null || dto.File.Length == 0)
            return BadRequest(new { error = Messages.EmptyFile });

        try
        {
            await _service.ConfirmUploadAsync(
                dto.ApplicationId,
                dto.DocumentId,
                token,
                dto.File.OpenReadStream(),
                dto.File.ContentType);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex) when (ex.Message == Messages.ContentTypeMismatch)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message == Messages.FileSizeExceeded)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge, new { error = ex.Message });
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

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(Messages.UserNotAuthenticated);

        return int.Parse(claim);
    }
}