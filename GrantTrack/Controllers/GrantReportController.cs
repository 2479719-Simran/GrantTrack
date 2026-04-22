using Microsoft.AspNetCore.Mvc;
using GrantTrack.Dto.GrantReportDtos;
using GrantTrack.Service.GrantReportServices;

namespace GrantTrack.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GrantReportController : ControllerBase
    {
        private readonly IGrantReportService _grantReportService;

        public GrantReportController(IGrantReportService grantReportService)
        {
            _grantReportService = grantReportService;
        }

        /// <summary>
        /// APPLICANT: Submits report metadata and the actual file stream.
        /// Using [FromForm] to handle multipart/form-data.
        /// </summary>
        [HttpPost("submit")]
        [Consumes("multipart/form-data")] // Required for file upload
        public async Task<IActionResult> SubmitReport([FromForm] GrantReportDto dto)
        {
            try
            {
                if (dto.EvidenceFile == null || dto.EvidenceFile.Length == 0)
                {
                    return BadRequest(new { message = "Evidence file is required." });
                }

                // Submit everything in one go (File + Data)
                var report = await _grantReportService.SubmitReportAsync(dto);

                return StatusCode(201, new 
                { 
                    message = "Report and evidence submitted successfully.", 
                    checkId = report.ComplianceCheckId,
                    status = report.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// OFFICER: View/Download the evidence file for a specific Check ID.
        /// This is how the Compliance Officer sees the file stored in DB.
        /// </summary>
        [HttpGet("evidence/{checkId}")]
        public async Task<IActionResult> GetEvidence(int checkId)
        {
            var report = await _grantReportService.GetReportByCheckIdAsync(checkId);

            if (report == null || report.FileStream == null)
            {
                return NotFound(new { message = "No evidence found for this check." });
            }

            // Return the byte array as a file stream to the browser
            // Content-Type is set to 'application/pdf' as a standard; you can make it dynamic based on file extension.
            return File(report.FileStream, "application/pdf", report.FileName);
        }

        /// <summary>
        /// STATUS TRACKER: Check report status by Check ID.
        /// </summary>
        [HttpGet("status/{checkId}")]
        public async Task<IActionResult> GetReportStatus(int checkId)
        {
            var report = await _grantReportService.GetReportByCheckIdAsync(checkId);
            
            if (report == null)
            {
                return NotFound(new { message = "No report found for this compliance check." });
            }

            return Ok(new {
                checkId = report.ComplianceCheckId,
                applicationId = report.ApplicationId,
                status = report.Status.ToString(),
                submittedDate = report.SubmittedDate,
                fileName = report.FileName,
                notes = report.Notes 
            });
        }
    }
}