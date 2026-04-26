using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.GrantReportRepositories
{
    public class GrantReportRepository : IGrantReportRepository
    {
        private readonly GrantTrackDbContext _context;

        public GrantReportRepository(GrantTrackDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetches a report by its primary key.
        /// </summary>
        public async Task<GrantReport?> GetByIdAsync(int reportId)
        {
            return await _context.GrantReports
                .FirstOrDefaultAsync(r => r.GrantReportId == reportId);
        }

        /// <summary>
        /// NEW: Fetches the report specifically linked to a unique Compliance Check.
        /// Essential for the 1-Application-to-Many-Checks logic.
        /// </summary>
        public async Task<GrantReport?> GetByCheckIdAsync(int checkId)
        {
            return await _context.GrantReports
                .FirstOrDefaultAsync(r => r.ComplianceCheckId == checkId);
        }

        /// <summary>
        /// Retrieves all reports for an application (Full history).
        /// </summary>
        public async Task<IEnumerable<GrantReport>> GetAllByApplicationIdAsync(int applicationId)
        {
            return await _context.GrantReports
                .Where(r => r.ApplicationId == applicationId)
                .ToListAsync();
        }

        public async Task AddAsync(GrantReport report)
        {
            await _context.GrantReports.AddAsync(report);
        }

        public void Update(GrantReport report)
        {
            _context.GrantReports.Update(report);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates if the application exists and is officially approved in the Decision table.
        /// </summary>
        public async Task<bool> IsApplicationApprovedAsync(int applicationId)
        {
            // Assuming DecisionValue 0 = Approved
            return await _context.Decisions
                .AnyAsync(d => d.ApplicationId == applicationId && (int)d.DecisionValue == 0);
        }
    }
}