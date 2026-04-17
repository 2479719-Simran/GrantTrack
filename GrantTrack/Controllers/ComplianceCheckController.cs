using Microsoft.AspNetCore.Mvc;
using GrantTrack.Domain.Entities;
using GrantTrack.Repository.ComplianceCheckRepository;
using GrantTrack.Dto.ComplianceCheckDtos;
using GrantTrack.Service.ComplianceCheckServices; // Service use panna idhu thevai

namespace GrantTrack.Controllers;

[ApiController]
[Route("api/compliance-checks")]
public class ComplianceCheckController : ControllerBase
{
    private readonly IComplianceCheckService _service; // Controller ippo service kooda dhaan pesanum

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
        // Only actual code crashes or DB connection issues will show 500
        return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
    }
}

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateComplianceCheckDto dto)
    {
        try
        {
            var updatedCheck = await _service.CompleteCheckAsync(id, dto);
            
            if (updatedCheck.Result == ComplianceResult.Completed)
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
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [ApiExplorerSettings(IgnoreApi = true)] //it doesn't need to be visible in Swagger, but we need it for internal use in the service layer
    public IActionResult GetById(int id)
    {
        return Ok();
    }
}