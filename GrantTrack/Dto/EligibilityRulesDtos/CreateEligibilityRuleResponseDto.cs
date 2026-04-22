using System;
using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.EligibilityRulesDtos;

public class CreateEligibilityRuleResponseDto
{
    [Required]
    public int RuleId { get; set; }
    [Required]
    public int ProgramId { get; set; }

    [Required]
    [MaxLength(500)]
    public string RuleDescription { get; set; }

    [Required]
    public string RuleExpression { get; set; }
}
