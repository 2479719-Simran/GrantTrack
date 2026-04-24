using System;
using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.ComplianceCheckRepository;

public class ComplianceCheckRepository:IComplianceCheckRepository
{

        private readonly GrantTrackDbContext _context;

        public ComplianceCheckRepository(GrantTrackDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ComplianceCheck check)
        {
            await _context.ComplianceChecks.AddAsync(check);
        }

        
        public async Task<ComplianceCheck?> GetByIdAsync(int id)
        {
            return await _context.ComplianceChecks
                .FirstOrDefaultAsync<ComplianceCheck>(c => c.ComplianceCheckId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
}

}