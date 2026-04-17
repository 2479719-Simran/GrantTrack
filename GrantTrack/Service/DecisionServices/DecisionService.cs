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
        /// Extracts the Approver identity from the JWT token via the Controller.
        /// </summary>
        
        public async Task CreateDecisionAsync(DecisionDto dto,int approverId)
        {
            // 1. Verify Application exists
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationId == dto.ApplicationId);

            if (application == null)
            {
                throw new KeyNotFoundException($"Application {dto.ApplicationId} not found.");
            }

            // --- BUSINESS LOGIC VALIDATIONS ---

            // A. Prevent decisions on Draft applications
            if (application.Status == ApplicationStatus.Draft)
            {
                throw new InvalidOperationException("Cannot record a decision for a Draft application. It must be submitted first.");
            }

            // B. Prevent re-approving an already Approved application
            if (application.Status == ApplicationStatus.Approved)
            {
                throw new InvalidOperationException("This application has already been Approved and cannot be modified.");
            }

            // C. Prevent re-rejecting an already Rejected application
            if (application.Status == ApplicationStatus.Rejected)
            {
                throw new InvalidOperationException("This application has already been Rejected and cannot be modified.");
            }

            // D. Prevent duplicate decision records in the Decision table
            if (await _decisionRepository.DecisionExistsAsync(dto.ApplicationId))
            {
                throw new InvalidOperationException("A formal decision record already exists for this application in the database.");
            }

            // --- DATA PERSISTENCE ---

            // 2. Map DTO to Decision Entity
            var decision = new Decision
            {
                ApplicationId = dto.ApplicationId,
                UserId = approverId ,
                DecisionValue = dto.DecisionValue,
                Notes = dto.Notes,
                Date = DateTime.UtcNow// Best practice: use UTC for server timestamps
            };
            
            await _decisionRepository.AddDecisionAsync(decision);

            // 3. Update the Application Status based on the decision
            application.Status = dto.DecisionValue == DecisionStatus.Approved
                ? ApplicationStatus.Approved
                : ApplicationStatus.Rejected;

            _context.Applications.Update(application);
            
            // 4. Save changes to both Application and Decision tables
            await _context.SaveChangesAsync();

            var actId = dto.DecisionValue == DecisionStatus.Approved ? 1 : 2;

            // 5. Create an Audit Log for tracking the action
            // Note: Verify if ActionId 0/1 matches your 'Operation' table IDs
            var auditLog = new AuditLog
            {
                UserId = approverId,
                ActionId = actId, // 1 for Approve, 2 for Reject (ensure these match your 'Operation' table)
                Resource = "Decision",
                TimeStamp = DateTime.UtcNow
            };
            
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }

    }
}


