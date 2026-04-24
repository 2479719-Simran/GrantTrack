using GrantTrack.Dto.EligibilityRulesDtos;
using GrantTrack.Service.EligibilityRuleServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrantTrack.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EligibilityRuleController : ControllerBase
    {
        private readonly IEligibilityRuleService ruleService;

    public EligibilityRuleController(IEligibilityRuleService ruleService)
    {
        this.ruleService = ruleService;
    }

    // POST: api/EligibilityRules
    [HttpPost]
    public async Task<IActionResult> CreateRule([FromBody] CreateEligibilityRuleRequestDto request)
    {
        try
        {
            var response = await ruleService.CreateRule(request);
            return CreatedAtAction(nameof(GetRulesByProgramId), new { programId = response.ProgramId }, response);
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

    // GET: api/EligibilityRules
    [HttpGet("GetProgramById/{programId}")]
    public async Task<IActionResult> GetRules()
    {
        try
        {
            var response = await ruleService.GetRules();
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }

    // GET: api/EligibilityRules/program/5
    [HttpGet("program/{programId}")]
    public async Task<IActionResult> GetRulesByProgramId([FromRoute] int programId)
    {
        try
        {
            var response = await ruleService.GetRulesByProgramId(programId);
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

    // PUT: api/EligibilityRules/5
    [HttpPut("{ruleId}")]
    public async Task<IActionResult> UpdateRule([FromRoute] int ruleId, [FromBody] UpdateEligibilityRuleRequestDto request)
    {
        try
        {
            var response = await ruleService.UpdateRule(ruleId, request);
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

    // DELETE: api/EligibilityRules/5
    [HttpDelete("{ruleId}")]
    public async Task<IActionResult> DeleteRule([FromRoute] int ruleId)
    {
        try
        {
            var response = await ruleService.DeleteRule(ruleId);
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
}
