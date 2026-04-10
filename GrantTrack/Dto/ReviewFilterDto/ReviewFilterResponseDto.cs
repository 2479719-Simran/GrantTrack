using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto;

public class ReviewFilterResponseDto
{
    public int ReviewerId { get; set; }
    public int PageNumber { get; set; } = 1; // Default page 1
    public int PageSize { get; set; } = 10; // Default 10 items
    public int ReviewId { get; set; }
    public int ApplicationId { get; set; }
    public string HolderName { get; set; }
    public ReviewDecision? Decision { get; set; }

}
