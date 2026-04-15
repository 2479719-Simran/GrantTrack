using GrantTrack.Controllers;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto;
using ComplianceCheck = GrantTrack.Domain.Entities.ComplianceCheck;

namespace GrantTrack.Service.ComplianceCheckServices;

public interface IComplianceCheckService
{
Task<ComplianceCheck> ScheduleCheckAsync(ComplianceCheckDto dto);

    // PATCH Requirement: Complete a check with outcome/notes
    Task<ComplianceCheck?> CompleteCheckAsync(int id, UpdateComplianceCheckDto dto);
}
