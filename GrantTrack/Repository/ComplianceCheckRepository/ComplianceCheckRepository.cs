using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.ComplianceCheckRepository;

public class ComplianceCheckRepository:IComplianceCheckRepository
{
private readonly GrantTrackDbContext _context;

    public ComplianceCheckRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

    // logic for Create (POST)
    public async Task AddAsync(ComplianceCheck check)
    {
        await _context.ComplianceChecks.AddAsync(check);
    }

    // logic for Update (PATCH)
    public async Task<ComplianceCheck?> GetByIdAsync(int id)
    {
        return await _context.ComplianceChecks.FindAsync(id);
    }

      public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    Task<ComplianceCheck?> IComplianceCheckRepository.GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}

