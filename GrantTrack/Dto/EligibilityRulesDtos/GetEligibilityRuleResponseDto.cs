using System;

namespace GrantTrack.Dto.EligibilityRulesDtos;

public class GetEligibilityRuleResponseDto
{
    public int RuleId { get; set; }
    public int ProgramId { get; set; }
    public string RuleDescription { get; set; }
    public string RuleExpression { get; set; }
}
