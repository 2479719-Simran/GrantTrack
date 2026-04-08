using System;
using GrantTrack.Domain.Entities;

namespace GrantTrack.Dto;

public class ReviewFilterRequestDto
{
    public int ReviewerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public ReviewDecision? Decision { get; set; }
}

