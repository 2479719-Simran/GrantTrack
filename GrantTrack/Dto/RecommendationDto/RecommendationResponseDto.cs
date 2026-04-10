using System;
using GrantTrack.Domain.Entities;
using GrantTrack.Dto.RecommendationDto;

namespace GrantTrack.Dto.RecommendationDto;

public class RecommendationResponseDto
{
public int RecommendationId { get; set; }
    public int ApplicationId { get; set; }
    public string ReviewerName { get; set; } 
    public ReviewDecision Decision { get; set; }
    public string Notes { get; set; }
    public DateTime Date { get; set;}
}
