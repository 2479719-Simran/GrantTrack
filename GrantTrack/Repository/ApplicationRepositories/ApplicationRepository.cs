using GrantTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrantTrack.Repository.ApplicationRepositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly GrantTrackDbContext _db;
    public ApplicationRepository(GrantTrackDbContext db)
    {
        _db = db;
    }
    public async Task<Application> CreateAsync(Application application)
    {
        _db.Applications.Add(application);
        await _db.SaveChangesAsync();
        return application;
    }
    public async Task<Application?> GetByIdAsync(int id)
        => await _db.Applications
            .Include(a => a.ProgramIDNavigation)
            .Include(a => a.ApplicantIDNavigation)
            .FirstOrDefaultAsync(a => a.ApplicationId == id);

    public async Task<Application> UpdateAsync(Application application)
    {
        _db.Applications.Update(application);
        await _db.SaveChangesAsync();
        return application;
    }

    // Returns true if the applicant already has any application for this program.
    public async Task<bool> ExistsForApplicantAsync(int applicantId, int programId)
        => await _db.Applications
            .AnyAsync(a => a.ApplicantId == applicantId && a.ProgramId == programId);

    // Loads the application along with its documents and applicant for evaluation.
    public async Task<Application?> GetForEvaluationAsync(int applicationId)
        => await _db.Applications
            .Include(a => a.Documents)
            .Include(a => a.ApplicantIDNavigation)
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);

    // Returns all eligibility rules defined for the given program.
    public async Task<List<EligibilityRule>> GetRulesAsync(int programId)
        => await _db.EligibilityRules
            .Where(r => r.ProgramId == programId)
            .ToListAsync();

    // Returns mandatory required documents for the given program.
    public async Task<List<RequiredDocument>> GetRequiredDocsAsync(int programId)
        => await _db.RequiredDocuments
            .Where(d => d.ProgramId == programId && d.Mandatory)
            .ToListAsync();

    // Bulk inserts validation results for an application.
    public async Task AddValidationsAsync(IEnumerable<ApplicationValidation> validations)
    {
        await _db.ApplicationValidations.AddRangeAsync(validations);
        await _db.SaveChangesAsync();
    }
}