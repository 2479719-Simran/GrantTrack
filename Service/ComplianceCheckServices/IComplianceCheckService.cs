using GrantTrack.Domain.Entities;
using GrantTrack.Dto;
using GrantTrack.Dto.ComplianceCheckDtos;

namespace GrantTrack.Service.ComplianceCheckServices
{
    public interface IComplianceCheckService
    {
        Task<ComplianceCheck> ScheduleCheckAsync(ComplianceCheckDto dto);
        Task<ComplianceCheck?> CompleteCheckAsync(int id, UpdateComplianceCheckDto dto);
    }
}
    
