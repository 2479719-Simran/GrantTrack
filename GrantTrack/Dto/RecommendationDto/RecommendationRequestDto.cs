using System;

namespace GrantTrack.Dto.RecommendationDto;

public class RecommendationRequestDto
{
    public int ApplicationId { get; set; }
    public int ReviewerId { get; set; }
    public string Decision { get; set; } // "Approved", "Rejected", "Needs Modification"
    public string Notes { get; set; }
    public int Score { get; set; }
    public string Comments { get; set; }

}
