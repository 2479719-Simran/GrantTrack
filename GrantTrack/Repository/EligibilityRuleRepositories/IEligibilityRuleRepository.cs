using System;
using GrantTrack.Dto.EligibilityRulesDtos;

namespace GrantTrack.Repository.EligibilityRuleRepositories;

public interface IEligibilityRuleRepository
{
    Task<bool> ContainsProgramId(int programId);
    Task<bool> ContainsRuleId(int ruleId);
    Task<CreateEligibilityRuleResponseDto> CreateRule(CreateEligibilityRuleRequestDto request);
    Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRules();
    Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRulesByProgramId(int programId);
    Task<UpdateEligibilityRuleResponseDto> UpdateRule(int ruleId, UpdateEligibilityRuleRequestDto request);
    Task<DeleteEligibilityRuleResponseDto> DeleteRule(int ruleId);
}
