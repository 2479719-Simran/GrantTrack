using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ComplianceCheckDtos;
using GrantTrack.Repository.ComplianceCheckRepository;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ComplianceCheckServices;

public class ComplianceCheckService : IComplianceCheckService
{
    private readonly IComplianceCheckRepository _repository;
    private readonly GrantTrackDbContext _context;

    public ComplianceCheckService(IComplianceCheckRepository repository, GrantTrackDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    /// <summary>
    /// POST: Schedule a check. 
    /// Notes are strictly mandatory for accountability.
    /// </summary>
    public async Task<ComplianceCheck> ScheduleCheckAsync(ComplianceCheckDto dto)
    {
        // 1. Strict Validation: Notes cannot be empty
        if (string.IsNullOrWhiteSpace(dto.Notes))
        {
            throw new ArgumentException("Notes are mandatory when initiating a compliance check.");
        }

        // 2. Business Logic: Must be an approved application
        var decision = await _context.Decisions
            .FirstOrDefaultAsync(d => d.ApplicationId == dto.ApplicationId);

        if (decision == null || (int)decision.DecisionValue != 0)
        {
            throw new KeyNotFoundException($"No approved decision found for Application ID {dto.ApplicationId}.");
        }

        if (!Enum.TryParse<ComplianceType>(dto.Type, true, out var cType))
            throw new ArgumentException("Invalid Compliance Type.");

        var newCheck = new ComplianceCheck
        {
            ApplicationId = dto.ApplicationId,
            Type = cType,
            Result = ComplianceResult.Flagged, 
            Date = DateTime.UtcNow,
            Notes = dto.Notes
        };

        await _repository.AddAsync(newCheck);
        await _repository.SaveChangesAsync();
        return newCheck;
    }

        /// <summary>   
        /// PATCH: Finalize the check result.
        /// Notes are mandatory for feedback and audit trail.   
        /// Strict linkage: Can only finalize if evidence has been submitted for this specific check.
        /// </summary>
        
    public async Task<ComplianceCheck?> CompleteCheckAsync(int id, UpdateComplianceCheckDto dto)
    {
        // 1. Strict Validation: Feedback notes mandatory
        if (string.IsNullOrWhiteSpace(dto.Notes))
        {
            throw new ArgumentException("Feedback notes are mandatory when updating a compliance result.");
        }

        var check = await _repository.GetByIdAsync(id);
        if (check == null) throw new KeyNotFoundException($"Check ID {id} not found.");

        // 2. Linkage: Find the report for this specific check
        var report = await _context.GrantReports.FirstOrDefaultAsync(r => r.ComplianceCheckId == id);

        if (report == null || report.Status == ReportStatus.Draft)
        {
            throw new InvalidOperationException("Applicant has not submitted evidence for this specific check yet.");
        }

        if (!Enum.TryParse<ComplianceResult>(dto.Result, true, out var outcome))
            throw new ArgumentException("Invalid Result Value.");

        // Update tables
        check.Result = outcome;
        check.Notes = dto.Notes;
        check.Date = DateTime.UtcNow;

        // Sync logic
        if (outcome == ComplianceResult.Completed)
            report.Status = ReportStatus.Verified;
        else if (outcome == ComplianceResult.Returned)
            report.Status = ReportStatus.Returned;
        else
            report.Status = ReportStatus.Submitted;

        await _repository.SaveChangesAsync();
        await _context.SaveChangesAsync();
        return check;
    }
}