using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DecisionDtos;
using GrantTrack.Repository.AuditLogRepoistories;
using GrantTrack.Repository.DecisionRepositories;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.DecisionServices
{
    public class DecisionService : IDecisionService
    {
        private readonly IDecisionRepository _decisionRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly GrantTrackDbContext _context;

        public DecisionService(
            IDecisionRepository decisionRepository,
            IAuditLogRepository auditLogRepository,
            GrantTrackDbContext context)
        {
            _decisionRepository = decisionRepository;
            _auditLogRepository = auditLogRepository;
            _context = context;
        }

        /// <summary>
        /// Processes the decision, updates application status, and creates an audit log.
        /// </summary>
        public async Task CreateDecisionAsync(DecisionDto dto)
        {
            // 1. Verify Application exists
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == dto.ApplicationId);

            if (application == null)
            {
                throw new KeyNotFoundException($"Application {dto.ApplicationId} not found.");
            }

            // --- NEW STATUS VALIDATIONS ---

            // 1. Prevent decisions on Draft applications (as discussed before)
            if (application.Status == ApplicationStatus.Draft)
            {
                throw new InvalidOperationException("Cannot record a decision for a Draft application. It must be submitted first.");
            }

            // 2. Prevent re-approving an already Approved application
            if (application.Status == ApplicationStatus.Approved)
            {
                throw new InvalidOperationException("This application has already been Approved and cannot be modified.");
            }

            // 3. Prevent re-rejecting an already Rejected application
            if (application.Status == ApplicationStatus.Rejected)
            {
                throw new InvalidOperationException("This application has already been Rejected and cannot be modified.");
            }

            // --- END NEW VALIDATIONS ---

            // 2. Prevent duplicate decision records in the Decision table
            if (await _decisionRepository.DecisionExistsAsync(dto.ApplicationId))
            {
                throw new InvalidOperationException("A decision record already exists for this application in the system.");
            }

            // 3. Save Decision
            var decision = new Decision
            {
                ApplicationId = dto.ApplicationId,
                UserId = dto.ApproverId,
                DecisionValue = dto.DecisionValue,
                Notes = dto.Notes,
                Date = dto.Date
            };
            await _decisionRepository.AddDecisionAsync(decision);

            // 4. Update Application Status 
            application.Status = dto.DecisionValue == DecisionStatus.Approved
                ? ApplicationStatus.Approved
                : ApplicationStatus.Rejected;

            _context.Applications.Update(application);
            await _context.SaveChangesAsync();

            // 5. Create Audit Log
            var auditLog = new AuditLog
            {
                UserId = dto.ApproverId,
                ActionId = dto.DecisionValue == DecisionStatus.Approved ? 1 : 2, // Ensure these match your DB
                Resource = "Decision",
                TimeStamp = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}