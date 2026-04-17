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
        /// Schedules a compliance check only if the application has been 'Approved'.
        /// Note: Works with String-converted Enums as defined in DbContext.
        /// </summary>
     public async Task<ComplianceCheck> ScheduleCheckAsync(ComplianceCheckDto dto)
{
    // 1. Fetch Decision
    var decision = await _context.Decisions
        .FirstOrDefaultAsync(d => d.ApplicationId == dto.ApplicationId);

    if (decision == null)
    {
        throw new KeyNotFoundException($"No decision record found for Application ID {dto.ApplicationId}.");
    }

    // 2. Status Check
    if ((int)decision.DecisionValue != 0)
    {
        throw new InvalidOperationException("Compliance checks can only be initiated for 'Approved' applications.");
    }

    // 3. SAFE ENUM PARSING (This prevents 500 error)
    if (!Enum.TryParse<ComplianceType>(dto.Type, ignoreCase: true, out var complianceType))
    {
        // 500-ku badhila indha message user-ku pogaum (Managed as Bad Request in Controller)
        throw new ArgumentException($"Invalid Compliance Type: '{dto.Type}'. Valid values are: Financial, Operational.");
    }

    // 4. Create Entity
    var newCheck = new ComplianceCheck
    {
        ApplicationId = dto.ApplicationId,
        Type = complianceType, // Use the parsed value here
        Result = ComplianceResult.Flagged,
        Date = DateTime.UtcNow,
        Notes = dto.Notes
    };

    await _repository.AddAsync(newCheck);
    await _repository.SaveChangesAsync();

    return newCheck;
}

        /// <summary>
        /// Updates an existing compliance check to 'Completed'.
        /// </summary>
        public async Task<ComplianceCheck?> CompleteCheckAsync(int id, UpdateComplianceCheckDto dto)
        {
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
            {
                throw new KeyNotFoundException($"Compliance Check with ID {id} not found.");
            }

            // Prevent editing if already finalized
            if (check.Result == ComplianceResult.Completed)
            {
                throw new InvalidOperationException("This compliance check is already completed and cannot be modified.");
            }

            // Update the result if a valid outcome is provided
            if (Enum.TryParse<ComplianceResult>(dto.Result, ignoreCase: true, out var outcome))
            {
                check.Result = outcome;
            }
            
            check.Notes = dto.Notes;
            check.Date = DateTime.UtcNow; // Update timestamp to completion time

            await _repository.SaveChangesAsync();

            return check;
        }
    }
}