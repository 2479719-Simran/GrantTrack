using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.AuditLogRepoistories;

    /// <summary>
    /// Defines operations for persisting audit logs.
    /// </summary>
public interface IAuditLogRepository
{

        /// <summary>
        /// Asynchronously adds a new audit log entry.
        /// </summary>
        Task AddAuditLogAsync(AuditLog auditLog);
    }

