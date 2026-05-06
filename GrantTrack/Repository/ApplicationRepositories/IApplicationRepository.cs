using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Repository.ApplicationRepositories;

public interface IApplicationRepository
{
     Task<Application> CreateAsync(Application application);
    Task<Application?> GetByIdAsync(int id);
    Task<Application> UpdateAsync(Application application);
    Task<bool> ExistsForApplicantAsync(int applicantId, int programId); // duplicate guard

    // Added for eligibility validation
    Task<Application?> GetForEvaluationAsync(int applicationId);
    Task<List<EligibilityRule>> GetRulesAsync(int programId);
    Task<List<RequiredDocument>> GetRequiredDocsAsync(int programId);
    Task AddValidationsAsync(IEnumerable<ApplicationValidation> validations);
  
}
