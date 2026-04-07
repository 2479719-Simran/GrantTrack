using System;
using System.ComponentModel.DataAnnotations;

namespace GrantTrack.Dto.RecommendationDto;

public class RecommendationRequestDto
{
    public int ApplicationId { get; set; }
    public int ReviewerId { get; set; }
    public string Decision { get; set; } // "Approved", "Rejected", "Needs Modification"
    public string Notes { get; set; }
    [RegularExpression(@"^([1-9]|10)$", ErrorMessage = "Score must be between 1 and 10, macha!")]
    public int Score { get; set; }
    public string Comments { get; set; }

}
