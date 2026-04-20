using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.ComplianceCheckRepository;

public interface IComplianceCheckRepository
{
    //post
    Task AddAsync(ComplianceCheck check);

    // PATCH
    Task<ComplianceCheck?> GetByIdAsync(int id);
    Task SaveChangesAsync();
}
