using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ComplianceCheckDtos;
using GrantTrack.Repository.ComplianceCheckRepository;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ComplianceCheckServices
{
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
        /// Initiates a new compliance check record.
        /// Only allowed for applications that have an 'Approved' decision status.
        /// </summary>
        public async Task<ComplianceCheck> ScheduleCheckAsync(ComplianceCheckDto dto)
        {
            // 1. Fetch the decision record for the given application
            var decision = await _context.Decisions
                .FirstOrDefaultAsync(d => d.ApplicationId == dto.ApplicationId);

            if (decision == null)
            {
                throw new KeyNotFoundException($"No decision record found for Application ID {dto.ApplicationId}.");
            }

            // 2. Validate if the application is approved (Assuming 0 = Approved)
            if ((int)decision.DecisionValue != 0)
            {
                throw new InvalidOperationException("Compliance checks can only be initiated for 'Approved' applications.");
            }

            // 3. Safe Enum Parsing for ComplianceType (e.g., Financial, Operational)
            if (!Enum.TryParse<ComplianceType>(dto.Type, ignoreCase: true, out var complianceType))
            {
                throw new ArgumentException($"Invalid Compliance Type: '{dto.Type}'. Valid values are: Financial, Operational.");
            }

            // 4. Map DTO to Entity and set default initial result
            var newCheck = new ComplianceCheck
            {
                ApplicationId = dto.ApplicationId,
                Type = complianceType,
                Result = ComplianceResult.Flagged, // Default status before review
                Date = DateTime.UtcNow,
                Notes = dto.Notes
            };

            await _repository.AddAsync(newCheck);
            await _repository.SaveChangesAsync();

            return newCheck;
        }

        /// <summary>
        /// Completes a compliance check and synchronizes the status with the Grantee's Report.
        /// Prevents modifications if the report is already verified.
        /// </summary>
       public async Task<ComplianceCheck?> CompleteCheckAsync(int id, UpdateComplianceCheckDto dto)
{
    // 1. Fetch the compliance check record
    var check = await _repository.GetByIdAsync(id);

    if (check == null)
    {
        throw new KeyNotFoundException($"Compliance Check with ID {id} not found.");
    }

    // 2. Fetch the corresponding Grant Report
    var report = await _context.GrantReports
        .FirstOrDefaultAsync(r => r.ApplicationId == check.ApplicationId);

    if (report == null)
    {
        throw new KeyNotFoundException("No Grant Report found to verify.");
    }

    // 3. Business Rule: If already verified, do not allow further compliance checks
    if (report.Status == ReportStatus.Verified)
    {
        throw new InvalidOperationException("This report has already been verified.");
    }

    // 4. Parse the result (Completed or Flagged)
    if (Enum.TryParse<ComplianceResult>(dto.Result, ignoreCase: true, out var outcome))
    {
        check.Result = outcome;

        // 5. UPDATE REPORT STATUS BASED ON COMPLIANCE RESULT
        if (outcome == ComplianceResult.Completed)
        {
            // If check is successful, the report is now officially Verified
            report.Status = ReportStatus.Verified;
        }
        else if (outcome == ComplianceResult.Flagged)
        {
            // If the officer flags it, we mark the report as Returned 
            // so the Applicant knows they need to check the notes and fix it.
            report.Status = ReportStatus.Returned;
        }
    }
    
    check.Notes = dto.Notes;
    check.Date = DateTime.UtcNow;

    // Save changes to both tables
    await _repository.SaveChangesAsync();
    await _context.SaveChangesAsync(); 

    return check;
}
    }
}