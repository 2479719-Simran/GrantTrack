using GrantTrack.Domain.Entities;
using GrantTrack.Dto.RequiredDocumentsDtos;
using GrantTrack.Service.RequiredDocumentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class RequiredDocumentController : ControllerBase
{
    private readonly IRequiredDocumentService documentService;

    public RequiredDocumentController(IRequiredDocumentService documentService)
    {
        this.documentService = documentService;
    }

    // POST: api/v1/RequiredDocument/CreateRequiredDocument
    [HttpPost("CreateRequiredDocument")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateDocument([FromBody] CreateRequiredDocumentRequestDto request)
    {
        try
        {
            var response = await documentService.CreateDocument(request);
            return CreatedAtAction(nameof(GetDocumentsByProgramId), new { programId = response.ProgramId }, response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }

    // GET: api/v1/RequiredDocument/GetRequiredDocuments
    [HttpGet("GetRequiredDocuments")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetDocuments()
    {
        try
        {
            var response = await documentService.GetDocuments();
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }

    // GET: api/v1/RequiredDocument/GetDocumentsByProgramId/{programId}
    [HttpGet("GetDocumentsByProgramId/{programId}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetDocumentsByProgramId([FromRoute] int programId)
    {
        try
        {
            var response = await documentService.GetDocumentsByProgramId(programId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }

    // PUT: api/v1/RequiredDocument/UpdateRequiredDocument/{documentId}
    [HttpPut("UpdateRequiredDocument/{documentId}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateDocument([FromRoute] int documentId, [FromBody] UpdateRequiredDocumentRequestDto request)
    {
        try
        {
            var response = await documentService.UpdateDocument(documentId, request);
            return Ok(response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }

    // DELETE: api/v1/RequiredDocument/DeleteRequiredDocument/{documentId}
    [HttpDelete("DeleteRequiredDocument/{documentId}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteDocument([FromRoute] int documentId)
    {
        try
        {
            var response = await documentService.DeleteDocument(documentId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }
}