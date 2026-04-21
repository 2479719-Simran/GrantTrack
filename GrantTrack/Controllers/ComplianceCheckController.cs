using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Required for Authorize attribute
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ComplianceCheckDtos;
using GrantTrack.Service.ComplianceCheckServices;

namespace GrantTrack.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
// Restricted to users with the 'ComplianceChecker' role
[Authorize(Roles = nameof(UserRole.ComplianceOfficer))] 
public class ComplianceCheckController : ControllerBase
{
    private readonly IComplianceCheckService _service;

    public ComplianceCheckController(IComplianceCheckService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateComplianceCheck([FromBody] ComplianceCheckDto dto)
    {
        try
        {
            var result = await _service.ScheduleCheckAsync(dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            // Returns 404 if the Application or Decision record is missing
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Returns 400 if the Enum Type is invalid
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // Returns 400 if the business rule (Approved status) fails
            return BadRequest(new { message = ex.Message });
        }
        
        catch (Exception ex)
        {
            // General 500 for unexpected system crashes
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateComplianceCheckDto dto)
    {
        try
        {
            var updatedCheck = await _service.CompleteCheckAsync(id, dto);
            
            // If the check is finalized, return a specific success event object
            if (updatedCheck != null && updatedCheck.Result == ComplianceResult.Completed)
            {
                return Ok(new { 
                    eventTriggered = "ComplianceCheck.Completed",
                    id = updatedCheck.CheckId,
                    status = "Success",
                    completionTime = updatedCheck.Date 
                });
            }

            return Ok(updatedCheck);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Update failed", detail = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ApiExplorerSettings(IgnoreApi = true)] 
    public IActionResult GetById(int id)
    {
        // This remains for internal routing/checks if needed
        return Ok();
    }
}