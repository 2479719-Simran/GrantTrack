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
            // 1. Fetch the existing compliance check record
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
            {
                throw new KeyNotFoundException($"Compliance Check with ID {id} not found.");
            }

            // 2. Fetch the corresponding Grant Report for this application
            var report = await _context.GrantReports
                .FirstOrDefaultAsync(r => r.ApplicationId == check.ApplicationId);

            if (report == null)
            {
                throw new KeyNotFoundException($"No Grant Report found for Application ID {check.ApplicationId}. Compliance cannot be completed without a report.");
            }

            // 3. Business Rule: Prevent processing if the report is already finalized/verified
            if (report.Status == ReportStatus.Verified)
            {
                throw new InvalidOperationException("This report has already been verified and the compliance process is finalized.");
            }

            // 4. Prevent editing if the compliance record itself is already marked as 'Completed'
            if (check.Result == ComplianceResult.Completed)
            {
                throw new InvalidOperationException("This compliance check is already completed and cannot be modified.");
            }

            // 5. Parse the new result from DTO and sync statuses
            if (Enum.TryParse<ComplianceResult>(dto.Result, ignoreCase: true, out var outcome))
            {
                check.Result = outcome;

                // Sync: If compliance is 'Completed', mark the Grantee's report as 'Verified'
                if (outcome == ComplianceResult.Completed)
                {
                    report.Status = ReportStatus.Verified;
                }
                // Sync: If compliance is 'Flagged', mark the report for 'Resubmission/Returned'
                else if (outcome == ComplianceResult.Flagged)
                {
                    report.Status = ReportStatus.Returned;
                }
            }
            
            check.Notes = dto.Notes;
            check.Date = DateTime.UtcNow; // Log the completion timestamp

            // Save changes for both ComplianceCheck and GrantReport
            await _repository.SaveChangesAsync();
            await _context.SaveChangesAsync(); 

            return check;
        }
    }
}