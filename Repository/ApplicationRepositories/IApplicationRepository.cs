using System.Collections.Generic;
using System.Threading.Tasks;
// Indha alias dhaan "Magic" line:
using AppEntity = GrantTrack.Domain.Entities.Application; 

namespace GrantTrack.Repository.Interface; // Inga unga correct namespace kudunga

public interface IApplicationRepository
{
    Task<AppEntity> CreateAsync(AppEntity application);
    Task<AppEntity?> GetByIdAsync(int id);
    Task<AppEntity> UpdateAsync(AppEntity application);
    Task<bool> ExistsForApplicantAsync(int applicantId, int programId);
    Task<AppEntity> GetApplicationByIdAsync(int id);
}