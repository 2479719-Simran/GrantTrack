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

            // 2. Prevent duplicate decisions
            if (await _decisionRepository.DecisionExistsAsync(dto.ApplicationId))
            {
                throw new InvalidOperationException("Decision already exists for this application.");
            }

            // 3. Save Decision
            var decision = new Decision
            {
                ApplicationId = dto.ApplicationId,
                UserId = dto.UserId,
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
                UserId = dto.UserId,
                ActionId = dto.DecisionValue == DecisionStatus.Approved ? 0 : 1, // 0:Approved, 1:Rejected
                Resource = "Decision",
                TimeStamp = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}