using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.GrantReportRepository;

public class GrantReportRepository : IGrantReportRepository
{
    private readonly GrantTrackDbContext _context;

    public GrantReportRepository(GrantTrackDbContext context)
    {
        _context = context;
    }

    public Task<GrantReport?> GetByApplicationIdAsync(int applicationId) =>
        _context.GrantReports
            .OrderByDescending(r => r.SubmittedDate)
            .FirstOrDefaultAsync(r => r.ApplicationId == applicationId);

    public async Task<GrantReport> AddAsync(GrantReport entity)
    {
        _context.GrantReports.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(GrantReport entity)
    {
        _context.GrantReports.Update(entity);
        await _context.SaveChangesAsync();
    }

    public Task<int?> GetApplicantIdForApplicationAsync(int applicationId) =>
        _context.Applications
            .Where(a => a.ApplicationId == applicationId)
            .Select(a => (int?)a.ApplicantId)
            .FirstOrDefaultAsync();

    public Task<bool> IsApprovedAsync(int applicationId) =>
        _context.Decisions
            .AnyAsync(d => d.ApplicationId == applicationId
                        && d.DecisionValue == DecisionStatus.Approved);
}
