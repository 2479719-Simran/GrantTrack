using GrantTrack.Domain.Entities;
using GrantTrack.Dto.ComplianceCheckDtos;

namespace GrantTrack.Service.ComplianceCheckServices
{
    public interface IComplianceCheckService
    {
        // STEP 1: Officer assigns a check (POST)
        // Strict notes validation implemented in Service
        Task<ComplianceCheck> ScheduleCheckAsync(ComplianceCheckDto dto);

        // STEP 2: Officer finalizes the result (PATCH)
        // Feedback notes are mandatory
        Task<ComplianceCheck?> CompleteCheckAsync(int id, UpdateComplianceCheckDto dto);
    }
}