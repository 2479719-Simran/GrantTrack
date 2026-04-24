using GrantTrack.Domain.Entities;
using GrantTrack.Dto.GrantReportDtos;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.GrantReportServices;

public class GrantReportService : IGrantReportService
{
    private readonly GrantTrackDbContext _context;

    public GrantReportService(GrantTrackDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Applicant submits evidence linked to a specific CheckId.
    /// Logic: File is stored as a byte array (FileStream) in the database.
    /// </summary>
    public async Task<GrantReport> SubmitReportAsync(GrantReportDto dto)
    {
        // 1. Dependency Guard: Check if the specific Compliance Check exists and is 'Flagged'
        var check = await _context.ComplianceChecks
            .FirstOrDefaultAsync(c => c.ComplianceCheckId == dto.CheckId);

        if (check == null)
        {
            throw new KeyNotFoundException($"No compliance check assignment found for Check ID {dto.CheckId}.");
        }

        if (check.Result != ComplianceResult.Flagged)
        {
            throw new InvalidOperationException("This check is not in a pending state. You cannot submit documents for a completed or returned check.");
        }

        // 2. Fetch existing report for this specific CheckId if it exists (for Re-submission)
        var existingReport = await _context.GrantReports
            .FirstOrDefaultAsync(r => r.ComplianceCheckId == dto.CheckId);

        // 3. Convert uploaded IFormFile to Byte Array (FileStream logic)
        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await dto.EvidenceFile.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        // CASE A: Fresh Submission for this Check
        if (existingReport == null)
        {
            var newReport = new GrantReport
            {
                ComplianceCheckId = dto.CheckId,
                ApplicationId = check.ApplicationId, // Derived from the check assignment
                Scope = dto.Scope,
                Metrics = dto.Metrics,
                FileName = dto.EvidenceFile.FileName,
                FileStream = fileBytes, // Actual data stored in DB
                Status = ReportStatus.Submitted,
                SubmittedDate = DateTime.UtcNow
            };

            await _context.GrantReports.AddAsync(newReport);
            await _context.SaveChangesAsync();
            return newReport;
        }

        // CASE B: Update / Re-submission Logic
        // Guard: If already Verified, prevent any further edits.
        if (existingReport.Status == ReportStatus.Verified)
        {
            throw new InvalidOperationException("This specific report has already been verified and is locked.");
        }

        // Update existing entry
        existingReport.Scope = dto.Scope;
        existingReport.Metrics = dto.Metrics;
        existingReport.FileName = dto.EvidenceFile.FileName;
        existingReport.FileStream = fileBytes;
        existingReport.Status = ReportStatus.Submitted;
        existingReport.SubmittedDate = DateTime.UtcNow;

        _context.GrantReports.Update(existingReport);
        await _context.SaveChangesAsync();
        return existingReport;
    }

    /// <summary>
    /// Fetches the report and evidence for a specific Check ID.
    /// Crucial for officers to see the document for a specific condition.
    /// </summary>
    public async Task<GrantReport?> GetReportByCheckIdAsync(int checkId)
    {
        return await _context.GrantReports
            .FirstOrDefaultAsync(r => r.ComplianceCheckId == checkId);
    }

    /// <summary>
    /// Retrieves a specific report by its primary key.
    /// </summary>
    public async Task<GrantReport?> GetReportByIdAsync(int reportId)
    {
        return await _context.GrantReports
            .FirstOrDefaultAsync(r => r.GrantReportId == reportId);
    }
}