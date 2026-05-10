using System.Security.Claims;
using GrantTrack.Domain.Entities;
using GrantTrack.Service.DocumentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

/// <summary>
/// Per-application document upload / list / delete / download. Mounted under
/// /api/v1/applications/{applicationId}/documents to keep URLs hierarchical.
///
/// Authorisation per HTTP method:
///   • POST upload, DELETE — Applicant only (and must own the application).
///   • GET list, GET download — Applicant OR Reviewer assigned to the
///     application. The service performs the ownership / assignment check
///     so the role pill on the JWT is never enough on its own.
/// </summary>
[ApiController]
[Route("api/v1/applications/{applicationId:int}/documents")]
[Authorize] // base requirement: must be authenticated; per-action attrs below narrow the role
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentsController(IDocumentService service)
    {
        _service = service;
    }

    /// <summary>
    /// POST /api/v1/applications/{applicationId}/documents
    /// multipart/form-data: file (required) + docType (optional)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB hard cap
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromRoute] int applicationId,
        [FromForm] IFormFile file,
        [FromForm] string? docType)
    {
        try
        {
            var result = await _service.UploadAsync(applicationId, GetUserId(), file, docType);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)              { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException ex)           { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex)    { return StatusCode(403, new { error = ex.Message }); }
        catch (Exception)                         { return StatusCode(500, new { error = "Upload failed." }); }
    }

    /// <summary>GET /api/v1/applications/{applicationId}/documents</summary>
    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Applicant) + "," + nameof(UserRole.Reviewer)
                       + "," + nameof(UserRole.Approver) + "," + nameof(UserRole.ComplianceOfficer)
                       + "," + nameof(UserRole.Admin) + "," + nameof(UserRole.FinanceOfficer))]
    public async Task<IActionResult> List([FromRoute] int applicationId)
    {
        try
        {
            var result = await _service.ListAsync(applicationId, GetUserId(), IsPrivilegedReader());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (Exception)                      { return StatusCode(500, new { error = "Could not list documents." }); }
    }

    /// <summary>DELETE /api/v1/applications/{applicationId}/documents/{documentId}</summary>
    [HttpDelete("{documentId:int}")]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    public async Task<IActionResult> Delete(
        [FromRoute] int applicationId,
        [FromRoute] int documentId)
    {
        try
        {
            await _service.DeleteAsync(documentId, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (Exception)                      { return StatusCode(500, new { error = "Delete failed." }); }
    }

    /// <summary>
    /// GET /api/v1/applications/{applicationId}/documents/{documentId}/download
    /// Streams the file with a Content-Disposition: attachment header so the
    /// browser shows a save dialog.
    /// </summary>
    [HttpGet("{documentId:int}/download")]
    [Authorize(Roles = nameof(UserRole.Applicant) + "," + nameof(UserRole.Reviewer)
                       + "," + nameof(UserRole.Approver) + "," + nameof(UserRole.ComplianceOfficer)
                       + "," + nameof(UserRole.Admin) + "," + nameof(UserRole.FinanceOfficer))]
    public async Task<IActionResult> Download(
        [FromRoute] int applicationId,
        [FromRoute] int documentId)
    {
        try
        {
            var (path, contentType, fileName) = await _service.GetDownloadAsync(documentId, GetUserId(), IsPrivilegedReader());
            return PhysicalFile(path, contentType, fileName);
        }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (FileNotFoundException)          { return NotFound(new { error = "File missing on server." }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
        catch (Exception)                      { return StatusCode(500, new { error = "Download failed." }); }
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User not authenticated.");
        return int.Parse(claim);
    }

    /// <summary>
    /// True for roles that get blanket read access (no per-application check).
    /// The class-level [Authorize(Roles=…)] on each action is the first gate;
    /// this just tells the service whether to perform the second check.
    /// </summary>
    private bool IsPrivilegedReader() =>
        User.IsInRole(nameof(UserRole.Approver))
        || User.IsInRole(nameof(UserRole.ComplianceOfficer))
        || User.IsInRole(nameof(UserRole.Admin))
        || User.IsInRole(nameof(UserRole.FinanceOfficer));
}
