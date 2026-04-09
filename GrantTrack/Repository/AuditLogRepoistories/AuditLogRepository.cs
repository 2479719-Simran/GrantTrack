using System;
using GrantTrack.Domain.Entities;
namespace GrantTrack.Repository.AuditLogRepoistories;

public class AuditLogRepository: IAuditLogRepository
{
        private readonly GrantTrackDbContext _context;

        public AuditLogRepository(GrantTrackDbContext context)
        {
            _context = context;
        }

        public async Task AddAuditLogAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }
}
