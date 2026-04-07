using System;

namespace GrantTrack.Dto.RecommendationDto;

public class RecommendationResponseDto
{
public int RecommendationId { get; set; }
    public int ApplicationId { get; set; }
    public string ReviewerName { get; set; } // Idhu Admin-ku paaka useful-ah irukkum
    public string Decision { get; set; }
    public string Notes { get; set; }
    public DateTime Date { get; set;}
}
