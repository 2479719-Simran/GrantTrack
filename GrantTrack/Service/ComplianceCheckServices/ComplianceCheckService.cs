using GrantTrack.Controllers;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto;
using GrantTrack.Repository.ComplianceCheckRepository;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.ComplianceCheckServices
{
    public class ComplianceCheckService : IComplianceCheckService
    {
          /// <summary>
        /// POST: Schedules a new compliance check for an application.
        /// </summary>
        public async Task<ComplianceCheck> ScheduleCheckAsync(ComplianceCheckDto dto)
        {
            // 1. Verify Application exists (DecisionService-la irundha maari validation)
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == dto.ApplicationId);

            if (application == null)
            {
                throw new KeyNotFoundException($"Application {dto.ApplicationId} not found.");
            }

            // 2. Map and Save new Compliance Check
            var newCheck = new ComplianceCheck
            {
                ApplicationId = dto.ApplicationId,
                Type = Enum.Parse<ComplianceType>(dto.Type),
                Result = ComplianceResult.Flagged, // Default initial status
                Date = DateTime.UtcNow,           // Auto-set scheduling time
                Notes = dto.Notes
            };

            await _repository.AddAsync(newCheck);
            await _repository.SaveChangesAsync();
            
            return newCheck;
        }

        /// <summary>
        /// PATCH: Stores outcome, notes, and triggers the Completion event.
        /// </summary>
        public async Task<Domain.Entities.ComplianceCheck?> CompleteCheckAsync(int id, UpdateComplianceCheckDto dto)
        {
            // 1. Find the existing check
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
            {
                throw new KeyNotFoundException($"Compliance Check ID {id} not found.");
            }

            // 2. Prevent re-completing an already completed check (Validation)
            if (check.Result == ComplianceResult.Completed)
            {
                throw new InvalidOperationException("This compliance check is already marked as Completed.");
            }

            // 3. Store outcome and notes
            if (Enum.TryParse<ComplianceResult>(dto.Result, out var outcome))
            {
                check.Result = outcome;
            }
            check.Notes = dto.Notes;

            // 4. Update the Date to exact completion time (Automatic)
            check.Date = DateTime.UtcNow;

            await _repository.SaveChangesAsync();

            // 5. Logic: event ComplianceCheck.Completed trigger
            if (check.Result == ComplianceResult.Completed)
            {
                // Inga event trigger logic implementation (e.g., Audit logs or notifications)
                Console.WriteLine($"[LOG]: Event ComplianceCheck.Completed for ID {id}");
            }

            return check;
        }

        private readonly IComplianceCheckRepository _repository;
        private readonly GrantTrackDbContext _context;

        public ComplianceCheckService(
            IComplianceCheckRepository repository, 
            GrantTrackDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        
    }
}