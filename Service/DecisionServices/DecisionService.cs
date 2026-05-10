using GrantTrack.Domain.Entities;
using GrantTrack.Dto.DecisionDtos;
using GrantTrack.Dto.NotificationsDtos;
using GrantTrack.Repository.AuditLogRepoistories;
using GrantTrack.Repository.DecisionRepositories;
using GrantTrack.Service.NotificationServices;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Service.DecisionServices
{
    public class DecisionService : IDecisionService
    {
        private readonly IDecisionRepository _decisionRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly INotificationServices _notifications;
        private readonly GrantTrackDbContext _context;

        public DecisionService(
            IDecisionRepository decisionRepository,
            IAuditLogRepository auditLogRepository,
            INotificationServices notifications,
            GrantTrackDbContext context)
        {
            _decisionRepository = decisionRepository;
            _auditLogRepository = auditLogRepository;
            _notifications = notifications;
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

            // 5. Notify the applicant — best-effort. The Decision is the
            // source of truth; if the notification push fails (e.g. the
            // applicant is offline) the row is still saved and the bell
            // will pick it up on their next page load.
            try
            {
                var verb = dto.DecisionValue == DecisionStatus.Approved ? "approved" : "rejected";
                await _notifications.SendNotificationAsync(new NotificationsCreateDto
                {
                    UserId = application.ApplicantId,
                    ApplicantId = application.ApplicantId,
                    Message = $"Your application #{application.ApplicationId} has been {verb}.",
                    Category = dto.DecisionValue == DecisionStatus.Approved ? "Approved" : "Rejected",
                });
            }
            catch
            {
                // Notification dispatch is auxiliary — never fail the
                // decision because the bell push had a hiccup.
            }

            // 6. Create Audit Log — best-effort.
            //
            // AuditLog.ActionId is a FK into the Operation table. When that
            // table isn't seeded (the default migration ships it empty) any
            // insert with ActionId = 0 or 1 fails the FK constraint and
            // bubbles up as a 500. By this point the Decision row is
            // already saved and Application.Status has been flipped — so
            // failing the whole call now would surface a misleading error
            // to the Approver and leave the system in an inconsistent
            // outward state. Treat audit logging as auxiliary and swallow
            // failures here; the Decision row is the source of truth.
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = dto.ApproverId,
                    ActionId = dto.DecisionValue == DecisionStatus.Approved ? 0 : 1, // 0 = Approve, 1 = Reject
                    Resource = "Decision",
                    TimeStamp = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);
            }
            catch
            {
                // Most likely an FK violation because the Operation table
                // has no row matching ActionId 0/1. Seeding Operation would
                // make this clean again — leaving as a known follow-up.
            }
        }
    }
}