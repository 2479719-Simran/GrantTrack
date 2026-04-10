using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.DecisionRepositories
{
    public class DecisionRepository : IDecisionRepository
    {
        private readonly GrantTrackDbContext _context;

        public DecisionRepository(GrantTrackDbContext context)
        {
            _context = context;
        }

        public async Task<bool> DecisionExistsAsync(int applicationId)
        {
            // Check if a decision record already exists to prevent duplicates
            return await _context.Decisions.AnyAsync(d => d.ApplicationId == applicationId);
        }

        public async Task AddDecisionAsync(Decision decision)
        {
            await _context.Decisions.AddAsync(decision);
            await _context.SaveChangesAsync();
        }
    }
}
