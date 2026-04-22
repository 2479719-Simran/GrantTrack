using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GrantTrack.Dto.GrantReportDtos;
using GrantTrack.Service.GrantReportServices;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GrantReportController : ControllerBase
{
    private readonly IGrantReportService _grantReportService;

    public GrantReportController(IGrantReportService grantReportService)
    {
        _grantReportService = grantReportService;
    }

    // Only Applicant can submit
    [HttpPost("submit")]
    [Authorize(Roles = nameof(UserRole.Applicant))]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitReport([FromForm] GrantReportDto dto)
    {
        try
        {
            var result = await _grantReportService.SubmitReportAsync(dto);
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Officer AND Applicant can download/view
    [HttpGet("evidence/{complianceCheckId}")]
    [Authorize(Roles = $"{nameof(UserRole.ComplianceOfficer)},{nameof(UserRole.Applicant)}")]
    public async Task<IActionResult> GetEvidence(int complianceCheckId)
    {
        var report = await _grantReportService.GetReportByCheckIdAsync(complianceCheckId);

        if (report == null || report.FileStream == null)
            return NotFound("No evidence found.");

        return File(report.FileStream, "application/pdf", report.FileName);
    }

    // Status check for both roles
    [HttpGet("status/{complianceCheckId}")]
    [Authorize(Roles = $"{nameof(UserRole.ComplianceOfficer)},{nameof(UserRole.Applicant)}")]
    public async Task<IActionResult> GetStatus(int complianceCheckId)
    {
        var report = await _grantReportService.GetReportByCheckIdAsync(complianceCheckId);
        if (report == null) return NotFound();

        return Ok(new { report.ComplianceCheckId, report.Status, report.SubmittedDate });
    }
}