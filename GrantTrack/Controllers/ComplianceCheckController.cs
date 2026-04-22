using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrantTrack.Dto.ComplianceCheckDtos;
using GrantTrack.Service.ComplianceCheckServices;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Api.Controllers;

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
          
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
           
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}