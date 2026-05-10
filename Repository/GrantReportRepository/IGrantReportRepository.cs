using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.GrantReportRepository;

public interface IGrantReportRepository
{
    /// <summary>The current report for this application, or null when none.</summary>
    Task<GrantReport?> GetByApplicationIdAsync(int applicationId);

    Task<GrantReport> AddAsync(GrantReport entity);
    Task UpdateAsync(GrantReport entity);

    /// <summary>
    /// Light projection used to verify ownership without loading the full
    /// Application graph — mirrors the helper on the document repo.
    /// </summary>
    Task<int?> GetApplicantIdForApplicationAsync(int applicationId);

    /// <summary>True when the application has an Approved decision.</summary>
    Task<bool> IsApprovedAsync(int applicationId);
}
