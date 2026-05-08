using System;
using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.EligibilityRulesDtos;

public class CreateEligibilityRuleRequestDto
{
    [Required]
    public int ProgramId { get; set; }

    [MaxLength(500)]
    [Required]
    public string RuleDescription { get; set; }

    [Required]
    public string RuleExpression { get; set; }
}
