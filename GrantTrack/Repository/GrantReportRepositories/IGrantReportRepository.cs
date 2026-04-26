using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.GrantReportRepositories
{
    public interface IGrantReportRepository
    {
        // Fetch by Report PK
        Task<GrantReport?> GetByIdAsync(int reportId);
        // NEW: Fetch report linked to a specific Compliance Check (Hero method)
        Task<GrantReport?> GetByCheckIdAsync(int checkId);
        // Fetch all reports for an application (History)
        Task<IEnumerable<GrantReport>> GetAllByApplicationIdAsync(int applicationId);
        // Standard CRUD operations
        Task AddAsync(GrantReport report);
        void Update(GrantReport report);
        Task SaveChangesAsync();
        // Helper to check if the application exists and is approved
        Task<bool> IsApplicationApprovedAsync(int applicationId);
    }
}