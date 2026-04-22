using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrantTrack.Dto.ComplianceCheckDtos;
using GrantTrack.Service.ComplianceCheckServices;

namespace GrantTrack.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "ComplianceOfficer")]
public class ComplianceCheckController : ControllerBase
{
    private readonly IComplianceCheckService _service;

    public ComplianceCheckController(IComplianceCheckService service)
    {
        _service = service;
    }

    // ROUTE 1: Assignment (POST)
    // Officer oru check-ai initiate pandradhu. Approved-ah illana 404 tharum.
    [HttpPost]
    public async Task<IActionResult> CreateComplianceCheck([FromBody] ComplianceCheckDto dto)
    {
        try
        {
            var result = await _service.ScheduleCheckAsync(dto);
            return StatusCode(201, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ROUTE 2: Update/Finalize (PATCH)
    // Evidence vandha aprom result update pandradhu. Notes mandatory.
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateComplianceCheckDto dto)
    {
        try
        {
            var updatedCheck = await _service.CompleteCheckAsync(id, dto);
            return StatusCode(201, new 
            { 
                message = "Compliance check finalized.",
                checkId = updatedCheck.ComplianceCheckId,
                status = updatedCheck.Result.ToString()
            });
        }
        catch (ArgumentException ex)
        {
            // Mandatory Notes missing-na inga catch aagum
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Evidence submit pannala-na inga catch aagum
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}