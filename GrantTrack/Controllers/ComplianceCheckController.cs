using Microsoft.AspNetCore.Mvc;
using GrantTrack.Domain.Entities;
using GrantTrack.Repository.ComplianceCheckRepository;
using GrantTrack.Dto.ComplianceCheckDtos;

namespace GrantTrack.Controllers;
[ApiController]
[Route("api/compliance-checks")]
public class ComplianceCheckController : ControllerBase
{
    private readonly IComplianceCheckRepository _repository;

    public ComplianceCheckController(IComplianceCheckRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Requirement: POST /compliance-checks
    /// Logic: Schedules a new check. Result starts as 'Flagged' (Pending).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ComplianceCheckDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var newCheck = new Domain.Entities.ComplianceCheck
        {
            ApplicationId = dto.ApplicationId,
            Type = Enum.Parse<ComplianceType>(dto.Type),
            Result = ComplianceResult.Flagged, // Starting status
            Date = DateTime.UtcNow,           // Scheduled time
            Notes = dto.Notes
        };

        await _repository.AddAsync(newCheck);
        await _repository.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newCheck.CheckId }, newCheck);
    }

    /// <summary>
    /// Requirement: PATCH /compliance-checks/{id}
    /// Logic: Stores outcome, updates automatic time, and triggers completion.
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateComplianceCheckDto dto)
    {
        var check = await _repository.GetByIdAsync(id);

        if (check == null)
        {
            return NotFound(new { message = $"Check ID {id} not found." });
        }

        // 1. Store Outcome (Enum conversion)
        if (Enum.TryParse<ComplianceResult>(dto.Result, out var outcome))
        {
            check.Result = outcome;
        }
        else
        {
            return BadRequest("Invalid Result value. Use 'Completed' or 'Flagged'.");
        }

        // 2. Store Notes
        check.Notes = dto.Notes;

        // 3. Logic: Update automatic time to the exact moment of completion
        check.Date = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        // AC Requirement: event ComplianceCheck.Completed
        if (check.Result == ComplianceResult.Completed)
        {
            // Inga neenga system events trigger pannalaam (e.g. Email notification)
            return Ok(new { 
                eventTriggered = "ComplianceCheck.Completed",
                id = check.CheckId,
                status = "Success",
                completionTime = check.Date 
            });
        }

        return Ok(new { message = "Outcome recorded successfully.", currentResult = check.Result.ToString() });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var check = await _repository.GetByIdAsync(id);
        return check == null ? NotFound() : Ok(check);
    }
}

