using System;
using GrantTrack.Dto.EligibilityRulesDtos;

namespace GrantTrack.Service.EligibilityRuleServices;

public interface IEligibilityRuleService
{
    Task<CreateEligibilityRuleResponseDto> CreateRule(CreateEligibilityRuleRequestDto request);
    Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRules();
    Task<IEnumerable<GetEligibilityRuleResponseDto>> GetRulesByProgramId(int programId);
    Task<UpdateEligibilityRuleResponseDto> UpdateRule(int ruleId, UpdateEligibilityRuleRequestDto request);
    Task<DeleteEligibilityRuleResponseDto> DeleteRule(int ruleId);
}
